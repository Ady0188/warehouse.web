using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Warehouse.Web.Orders.Data
{
    internal class OrderDbContext : DbContext
    {
        private readonly IDomainEventDispatcher _dispatcher;
        public OrderDbContext(DbContextOptions<OrderDbContext> options, IDomainEventDispatcher dispatcher) : base(options)
        {
            _dispatcher = dispatcher;
        }
        public DbSet<Outbox> Outboxes { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Orders");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private bool _flushingOutbox;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_flushingOutbox)
                return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var entitiesWithEvents = ChangeTracker.Entries<IHaveDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents != null && e.DomainEvents.Any())
                .ToArray();

            var pendingEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            await using var tx = await Database.BeginTransactionAsync(cancellationToken);

            var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            foreach (var ev in pendingEvents)
            {
                if (ev is OrderHistoryEvent he)
                {
                    var oldJson = he.OldOrder?.ToJson();
                    var newJson = he.NewOrder.ToJson();

                    Outboxes.Add(Outbox.FromEvent(
                        storeName: he.StoreName ?? "unknown",
                        userName: he.UserName ?? "unknown",
                        method: (short)he.Method,
                        objectId: he.NewOrder.Id,
                        objectName: "Order",
                        oldData: oldJson,
                        newData: newJson
                    ));
                }
            }

            _flushingOutbox = true;
            try
            {
                await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _flushingOutbox = false;
            }

            await tx.CommitAsync(cancellationToken);

            if (_dispatcher != null && entitiesWithEvents.Length > 0)
                await _dispatcher.DispatchAndClearEvents(entitiesWithEvents).ConfigureAwait(false);

            return result;
        }
    }
}

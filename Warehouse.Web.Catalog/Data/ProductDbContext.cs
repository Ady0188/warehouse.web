
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Warehouse.Web.Catalog.Data;

internal class ProductDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;
    public ProductDbContext(DbContextOptions<ProductDbContext> options, IDomainEventDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<Outbox> Outboxes { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Products");
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
            if (ev is ProductHistoryEvent he)
            {
                var oldJson = he.OldProduct?.ToJson();
                var newJson = he.NewProduct.ToJson();

                Outboxes.Add(Outbox.FromEvent(
                    storeName: he.UserStoreName ?? "unknown",
                    userName: he.UserName ?? "unknown",
                    method: (short)he.Method,
                    objectId: he.NewProduct.Id,
                    objectName: "Product",
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

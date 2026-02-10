using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Warehouse.Web.Managers.Data;

internal class ManagerDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;
    public ManagerDbContext(DbContextOptions<ManagerDbContext> options, IDomainEventDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<Outbox> Outboxes { get; set; } = null!;
    public DbSet<Manager> Managers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Managers");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private bool _flushingOutbox; // чтобы не зациклиться на втором SaveChanges

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_flushingOutbox) // второй заход — просто сохраняем
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 1) Собираем доменные события ДО сохранения
        var entitiesWithEvents = ChangeTracker.Entries<IHaveDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents != null && e.DomainEvents.Any())
            .ToArray();

        // Сохраняем ссылку на события, потому что будем чистить их позже
        var pendingEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // 2) Явная транзакция, чтобы aggregate и outbox были атомарны
        await using var tx = await Database.BeginTransactionAsync(cancellationToken);

        // 3) Первый Save — генерируются ключи (Id)
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 4) Построить записи Outbox ТЕПЕРЬ, когда Id уже известны
        foreach (var ev in pendingEvents)
        {
            if (ev is ManagerHistoryEvent mhe)
            {
                // здесь mhe.NewManager.Id уже заполнен реальным значением
                var oldJson = mhe.OldManager?.ToJson();
                var newJson = mhe.NewManager.ToJson();

                Outboxes.Add(Outbox.FromEvent(
                    storeName: mhe.StoreName ?? "unknown",
                    userName: mhe.UserName ?? "unknown",
                    method: (short)mhe.Method,
                    managerId: mhe.NewManager.Id,   // <-- теперь не 0
                    objectName: "Manager",
                    oldData: oldJson,
                    newData: newJson
                ));
            }
        }

        // 5) Второй Save — сохраняем outbox в той же транзакции
        _flushingOutbox = true;
        try
        {
            await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _flushingOutbox = false;
        }

        // 6) Коммитим транзакцию
        await tx.CommitAsync(cancellationToken);

        // 7) Диспетчим и очищаем события ПОСЛЕ успешного коммита
        if (_dispatcher != null && entitiesWithEvents.Length > 0)
            await _dispatcher.DispatchAndClearEvents(entitiesWithEvents).ConfigureAwait(false);

        return result;
    }
}

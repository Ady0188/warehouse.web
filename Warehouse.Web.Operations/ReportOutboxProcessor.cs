using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Warehouse.Web.Contracts;
using Warehouse.Web.Operations.Data;

namespace Warehouse.Web.Operations;

internal class ReportOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportOutboxProcessor> _logger;

    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(10);
    private const int BatchSize = 50;

    public ReportOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<ReportOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OperationDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);

                var batch = await db.ReportOutboxes
                    .FromSqlRaw(@"
                        SELECT *
                        FROM ""Operations"".""ReportOutboxes""
                        WHERE ""ProcessedDate"" IS NULL
                        ORDER BY ""Id""
                        LIMIT {0}
                        FOR UPDATE SKIP LOCKED
                    ", BatchSize)
                    .ToListAsync(stoppingToken);

                if (batch.Count == 0)
                {
                    await tx.CommitAsync(stoppingToken);
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                foreach (var item in batch)
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<OperationReportOutboxPayload>(item.Payload);
                        if (payload == null || payload.Operation == null)
                            throw new InvalidOperationException("Outbox payload is empty or invalid.");

                        await mediator.Publish(new OperationIntegrationEvent(payload.Operation, payload.Method), stoppingToken);
                        item.MarkProcessed();
                    }
                    catch (Exception ex)
                    {
                        item.MarkFailed(ex.Message);
                        _logger.LogError(ex, "Failed to process report outbox item {OutboxId}", item.Id);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
                await tx.CommitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Report outbox processor failed.");
                await Task.Delay(ErrorDelay, stoppingToken);
            }
        }
    }
}

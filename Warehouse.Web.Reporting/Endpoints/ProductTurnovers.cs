using FastEndpoints;
using MediatR;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Reporting.Integrations;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Endpoints;

internal class AgentTurnovers : Endpoint<TurnoverPagedRequest, AgentTurnoversResponse>
{
    private readonly IMediator _mediator;
    private readonly AgentRemainsIngestionService _agentRemainsIngestionService;
    private readonly ILogger<AgentTurnovers> _logger;

    public AgentTurnovers(IMediator mediator, AgentRemainsIngestionService agentRemainsIngestionService, ILogger<AgentTurnovers> logger)
    {
        _mediator = mediator;
        _agentRemainsIngestionService = agentRemainsIngestionService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Reports.AgentTurnovers);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(TurnoverPagedRequest request, CancellationToken ct)
    {
        try
        {
            var toDate = DateTime.Now;
            var fromDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 0, 0, 0);
            if (!string.IsNullOrEmpty(request.FromDate))
            {
                if (!DateTime.TryParseExact(request.FromDate, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
                {
                    AddError("Invalid from date format. Expected ddMMyyyyHHmmss.");
                    await SendErrorsAsync(400, ct);
                    return;
                }
                fromDate = parsedFrom;
            }
            if (!string.IsNullOrEmpty(request.ToDate))
            {
                if (!DateTime.TryParseExact(request.ToDate, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
                {
                    AddError("Invalid to date format. Expected ddMMyyyyHHmmss.");
                    await SendErrorsAsync(400, ct);
                    return;
                }
                toDate = parsedTo;
            }

            if (request.AgentId.HasValue)
            {
                var agentRemains = await _agentRemainsIngestionService.GetDebtsByIdAsync(request.AgentId.Value);

                var _agentStartRemains = agentRemains.Where(x => x.Date < fromDate).ToList();
                var _agentEndRemains = agentRemains.Where(x => x.Date < toDate).ToList();
                agentRemains = agentRemains.Where(x => x.Date >= fromDate && x.Date <= toDate).ToList();

                await SendAsync(new AgentTurnoversResponse
                {
                    Total = agentRemains.Count(),
                    StartRemains = _agentStartRemains.Sum(x => x.Amount - x.Discount),
                    EndRemains = _agentEndRemains.Sum(x => x.Amount - x.Discount),
                    Items = agentRemains.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                        .Select(t => new AgentTurnoverResponse
                        {
                            StoreId = t.StoreId,
                            StoreName = t.StoreName,
                            AgentId = t.AgentId,
                            AgentName = t.AgentName,
                            Code = t.ObjectCode,
                            Operation = t.ObjectName.GetOperationName(t.ObjectType.ToString()),
                            Amount = t.Amount,
                            Discount = t.Discount,
                            Date = t.Date
                        }).ToList()
                });

                return;
            }

            await SendAsync(new AgentTurnoversResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get agent turnovers.");
            await SendErrorsAsync(500);
        }
    }
}
internal class ProductTurnovers : Endpoint<TurnoverPagedRequest, TurnoversResponse>
{
    private readonly IMediator _mediator;
    private readonly ProductTurnoverIngestionService _productTurnoverIngestionService;
    private readonly ILogger<ProductTurnovers> _logger;

    public ProductTurnovers(IMediator mediator, ProductTurnoverIngestionService productTurnoverIngestionService, ILogger<ProductTurnovers> logger)
    {
        _mediator = mediator;
        _productTurnoverIngestionService = productTurnoverIngestionService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Reports.Turnovers);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(TurnoverPagedRequest request, CancellationToken ct)
    {
        try
        {
            var toDate = DateTime.Now;
            var fromDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 0, 0, 0);
            if (!string.IsNullOrEmpty(request.FromDate))
            {
                if (!DateTime.TryParseExact(request.FromDate, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
                {
                    AddError("Invalid from date format. Expected ddMMyyyyHHmmss.");
                    await SendErrorsAsync(400, ct);
                    return;
                }
                fromDate = parsedFrom;
            }
            if (!string.IsNullOrEmpty(request.ToDate))
            {
                if (!DateTime.TryParseExact(request.ToDate, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
                {
                    AddError("Invalid to date format. Expected ddMMyyyyHHmmss.");
                    await SendErrorsAsync(400, ct);
                    return;
                }
                toDate = parsedTo;
            }

            long storeId = request.StoreId ?? 0;

            List<ProductTurnover> turnovers = new();
            if (storeId == 0)
                turnovers = await _productTurnoverIngestionService.GetAllTurnoversWithProductsAsync(toDate);
            else
                turnovers = await _productTurnoverIngestionService.GetTurnoversWithProductsByStoreIdAsync(storeId, fromDate, toDate);

            turnovers = turnovers.Where(x => x.Products is not null && x.Products.Any(p => p.ProductId == request.ProductId)).ToList();

            var lastAuditDate = await _productTurnoverIngestionService.GetLastAuditProductsDateAsync(storeId, toDate);

            List<ProductTurnover> _turnovers = await _productTurnoverIngestionService.GetTurnoversWithProductsByStoreIdAsync(storeId, lastAuditDate, toDate);

            await SendAsync(new TurnoversResponse
            {
                Total = turnovers.Count,
                StartRemains = _turnovers.SelectMany(x => x.Products.Where(p => p.ProductId == request.ProductId).Select(p => p.Quantity)).Sum(x => (long)x),
                EndRemains = _turnovers.SelectMany(x => x.Products.Where(p => p.ProductId == request.ProductId).Select(p => p.Quantity)).Sum(x => (long)x),
                Items = turnovers.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                    .SelectMany(t => t.Products.Where(p => p.ProductId == request.ProductId).Select(p => new TurnoverResponse
                    {
                        StoreId = t.StoreId,
                        StoreName = t.StoreName,
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        AgentId = t.AgentId,
                        AgentName = t.AgentName,
                        Operation = t.ObjectName.GetOperationName(t.ObjectType.ToString()),
                        Price = GetPrice(p.Price, t.Amount, t.Discount),
                        Quantity = p.Quantity,
                        Amount = GetPrice(p.Price, t.Amount, t.Discount) * p.Quantity,
                        Date = t.Date
                    }) ?? Enumerable.Empty<TurnoverResponse>())
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product turnovers.");
            await SendErrorsAsync(500);
        }
    }

    decimal GetPrice(decimal productPrice, decimal operationAmount, decimal operationDiscount)
    {
        var percentage = operationAmount == 0 ? 0 : operationDiscount * 100m / operationAmount;

        return productPrice - (productPrice * percentage / 100m);
    }
}

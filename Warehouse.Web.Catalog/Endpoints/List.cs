using FastEndpoints;
using MediatR;
using Warehouse.Web.Agents;
using Warehouse.Web.Catalog.UseCases.Queries;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Catalog.Endpoints;

public class List : Endpoint<PagedRequest, ProductsResponse>
{
    private readonly IMediator _mediator;

    public List(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Products.GetAll);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        var query = new GetAllProductsQuery(request.ToOptions());
        var queryResult = await _mediator.Send(query);

        if (queryResult is null || !queryResult.IsSuccess)
        {
            await SendErrorsAsync(500, ct);
            return;
        }

        await SendAsync(queryResult.Value);
    }
}

public class ExpoerToExcel : Endpoint<PagedRequest, ProductsResponse>
{
    private readonly IMediator _mediator;
    private readonly ExportFileService _exportFileService;

    public ExpoerToExcel(IMediator mediator, ExportFileService exportFileService)
    {
        _mediator = mediator;
        _exportFileService = exportFileService;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Products.ExportRemains);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        var query = new GetAllProductsQuery(request.ToOptions(50000));
        var queryResult = await _mediator.Send(query);

        if (queryResult is null || !queryResult.IsSuccess)
        {
            await SendErrorsAsync(500, ct);
            return;
        }

        var export = await _exportFileService.ExportRemains(queryResult.Value.Items, queryResult.Value.Stores);

        await SendBytesAsync(
            export.Bytes,
            contentType: export.ContentType,
            fileName: export.FileName,
            cancellation: ct);
    }
}
using FastEndpoints;
using MediatR;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Shared;
using System.Security.Claims;
using Warehouse.Web.Operations.UseCases.Queries;

namespace Warehouse.Web.Operations.Endpoints;

public class ExportList : Endpoint<PagedRequest, ExportFileResult>
{
    private readonly IMediator _mediator;
    private readonly ExportFileService _exportFileService;

    public ExportList(IMediator mediator, ExportFileService exportFileService)
    {
        _mediator = mediator;
        _exportFileService = exportFileService;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Operations.Export);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        long storeId = 0;
        if (!User.IsInRole("Admin"))
        {
            storeId = long.Parse(User.FindFirstValue("StoreId")!);
        }

        var query = new GetAllOperationsQuery(storeId, (OperationType)request.OperationType, request.ToOptions(storeId));
        var queryResult = await _mediator.Send(query);

        if (queryResult is null || !queryResult.IsSuccess)
        {
            await SendErrorsAsync(500);
            return;
        }

        var export = await _exportFileService.Export(queryResult.Value.Items);

        await SendBytesAsync(
            export.Bytes,
            contentType: export.ContentType,
            fileName: export.FileName,
            cancellation: ct);
    }
}

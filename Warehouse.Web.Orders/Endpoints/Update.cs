using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Globalization;
using Warehouse.Web.Orders.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Orders.Endpoints
{
    public class Update : Endpoint<UpdateOrderRequest>
    {
        private readonly IMediator _mediator;

        public Update(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put(ApiEndpoints.V1.Orders.Update);
            Roles(new string[] { "User" });
        }
        public override async Task HandleAsync(UpdateOrderRequest req, CancellationToken ct)
        {
            if (!DateTime.TryParseExact(req.Date, "ddMMyyyyHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                AddError("Invalid date format. Expected ddMMyyyyHHmm.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var command = new UpdateOrderCommand(req.Id, dt, req.DocId, req.StoreId, req.AgentId, req.Amount, req.Comment, req.Type);
            var commandResult = await _mediator.Send(command);

            if (commandResult.Status == ResultStatus.NotFound)
            {
                await SendNotFoundAsync();
                return;
            }
            else if (!commandResult.IsSuccess)
            {
                await SendErrorsAsync(500);
                return;
            }

            await SendOkAsync();
        }
    }
}

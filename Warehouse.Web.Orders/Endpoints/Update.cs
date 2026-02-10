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
            var dt = DateTime.ParseExact(req.Date, "ddMMyyyyHHmm", CultureInfo.InvariantCulture);

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

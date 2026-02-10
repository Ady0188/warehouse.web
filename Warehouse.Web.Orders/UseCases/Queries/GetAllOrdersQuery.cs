using Ardalis.Result;
using MediatR;
using System.Globalization;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Reporting.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;
using static Warehouse.Web.Shared.ApiEndpoints.V1;

namespace Warehouse.Web.Orders.UseCases.Queries
{
    internal record GetAllOrdersQuery(long StoreId, GetAllOptions Options, bool IncludeDebts = false, string? DateFrom = null, string? DateTo = null) : IRequest<Result<OrdersResponse>>;
    internal class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<OrdersResponse>>
    {
        private readonly IReadOnlyOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        public GetAllOrdersQueryHandler(IReadOnlyOrderRepository orderRepository, IMediator mediator)
        {
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<Result<OrdersResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var list = await _orderRepository.ListWithTotalAsync(request.Options);
            var orders = list.Result;

            var storesQuery = new GetAllStoresQuery(true);
            var storesQueryResult = await _mediator.Send(storesQuery);

            var managers = storesQueryResult.Value.SelectMany(x => x.Managers);
            long managerId = 0;
            if (request.Options.Filter is not null && request.Options.Filter.Contains("ManagerId"))
            {
                var splited = request.Options.Filter.Split(")and(").First(x => x.Contains("ManagerId")).Trim('(', ')').Split(',')!;
                var idStr = Uri.UnescapeDataString(splited[1]?.Trim() ?? string.Empty);

                if (!string.IsNullOrEmpty(idStr))
                    managerId = long.Parse(idStr);
            }

            Dictionary<long, StoreResponse> stores = storesQueryResult.Value
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .ToDictionary(s => s.Id, s => s);

            Dictionary<long, AgentResponse> agents = new();

            if (request.StoreId == 0)
            {
                var agentsQuery = new GetAllAgentsQuery();
                var agentsQueryResult = await _mediator.Send(agentsQuery);

                if (!agentsQueryResult.IsSuccess)
                    return Result.Error(agentsQueryResult.Errors.First());

                var _agents = managerId != 0 ? agentsQueryResult.Value.Where(x => x.ManagerId == managerId).ToList() : agentsQueryResult.Value;
                agents = _agents
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .ToDictionary(a => a.Id, a => a);
            }
            else
            {
                var _store = stores.First(s => s.Key == request.StoreId);

                stores = new Dictionary<long, StoreResponse> { { _store.Key, _store.Value } };

                var agentsQuery = new GetAllAgentsByStoreIdQuery(request.StoreId);
                var agentsQueryResult = await _mediator.Send(agentsQuery);

                if (!agentsQueryResult.IsSuccess)
                    return Result.Error(agentsQueryResult.Errors.First());

                var _agents = managerId != 0 ? agentsQueryResult.Value.Where(x => x.ManagerId == managerId).ToList() : agentsQueryResult.Value;
                agents = _agents
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .ToDictionary(a => a.Id, a => a);
            }

            var agentsIds = agents.Select(x => x.Value.Id).ToList();
            orders = orders.Where(x => agentsIds.Contains(x.AgentId)).ToList();

            //if (request.StoreId == 0)
            //{
            //    var storesQuery = new GetAllStoresQuery(true);
            //    var storesQueryResult = await _mediator.Send(storesQuery);

            //    if (!storesQueryResult.IsSuccess)
            //        return Result.Error(storesQueryResult.Errors.First());

            //    stores = storesQueryResult.Value
            //            .GroupBy(s => s.Id)
            //            .Select(g => g.First())
            //            .ToDictionary(s => s.Id, s => s);

            //    var agentsQuery = new GetAllAgentsQuery();
            //    var agentsQueryResult = await _mediator.Send(agentsQuery);

            //    if(!agentsQueryResult.IsSuccess)
            //        return Result.Error(agentsQueryResult.Errors.First());

            //    agents = agentsQueryResult.Value
            //        .GroupBy(s => s.Id)
            //        .Select(g => g.First())
            //        .ToDictionary(a => a.Id, a => a);
            //}
            //else
            //{
            //    var storesQuery = new GetStoreByIdQuery(request.StoreId, true);
            //    var storesQueryResult = await _mediator.Send(storesQuery);

            //    if (!storesQueryResult.IsSuccess)
            //        return Result.Error(storesQueryResult.Errors.First());

            //    stores = new Dictionary<long, StoreResponse> { { storesQueryResult.Value.Id, storesQueryResult.Value } };

            //    var agentsQuery = new GetAllAgentsByStoreIdQuery(request.StoreId);
            //    var agentsQueryResult = await _mediator.Send(agentsQuery);

            //    if (!agentsQueryResult.IsSuccess)
            //        return Result.Error(agentsQueryResult.Errors.First());

            //    agents = agentsQueryResult.Value
            //        .GroupBy(s => s.Id)
            //        .Select(g => g.First())
            //        .ToDictionary(a => a.Id, a => a);
            //}

            if (request.IncludeDebts)
            {
                var dateFrom = DateTime.ParseExact(request.DateFrom!, "ddMMyyyy", CultureInfo.InvariantCulture);
                var dateTo = DateTime.ParseExact(request.DateTo!, "ddMMyyyy", CultureInfo.InvariantCulture);

                var debtsQuery = new GetAgentsDebtsQuery(dateFrom, dateTo);
                var debtsQueryResult = await _mediator.Send(debtsQuery);

                if (!debtsQueryResult.IsSuccess)
                    return Result.Error(debtsQueryResult.Errors.First());

                agents = agents.ToDictionary(a => a.Key, a =>
                {
                    var debts = debtsQueryResult.Value.FirstOrDefault(x => x.Id == a.Key) ?? new AgentDebtsResponse();
                    return new AgentResponse
                    {
                        Id = a.Value.Id,
                        Address = a.Value.Address,
                        Comment = a.Value.Comment,
                        ManagerId = a.Value.ManagerId,
                        Name = a.Value.Name,
                        Phone = a.Value.Phone,
                        ManagerName = a.Value.ManagerName,
                        ManagerPhone = a.Value.ManagerPhone,
                        StoreId = a.Value.StoreId,
                        StoreName = a.Value.StoreName,
                        Debts = debts
                    };
                });
            }

            return new OrdersResponse
            {
                Total = orders.Count(),
                TotalAmount = orders.Sum(x => x.Amount),
                Stores = stores.Values.ToList(),
                Agents = agents.Values.ToList(),
                Items = orders
                    .Skip(request.Options.Skip)
                    .Take(request.Options.PageSize)
                    .Select(a =>
                    {
                        var store = stores.Values.FirstOrDefault(x => x.Id == a.StoreId);
                        var agent = agents.Values.FirstOrDefault(m => m.Id == a.AgentId);
                        var manager = managers.FirstOrDefault(m => m.Id == (agent is null ? -1 : agent.ManagerId));

                        return new OrderResponse
                        {
                            Id = a.Id,
                            Code = a.Code,
                            Date = a.Date,
                            DocId = a.DocId,
                            StoreId = store is not null ? store.Id : 0,
                            StoreName = store is not null ? store.Name : string.Empty,
                            AgentId = agent is not null ? agent.Id : 0,
                            AgentName = agent is not null ? agent.Name : string.Empty,
                            ManagerName = manager is not null ? $"{manager.Lastname} {manager.Firstname}".Trim() : string.Empty,
                            Amount = a.Amount,
                            Comment = a.Comment,
                            Type = (int)a.Type,
                            Agents = a.Agents.Select(at =>
                            {
                                return new OperAgentResponse
                                {
                                    Id = at.AgentId,
                                    Name = at.Name,
                                    Plan = at.Debt,
                                    Fact = at.Debt,
                                    Difference = at.Difference
                                };
                            }).ToList()
                        };
                    }).ToList()
            };
        }
    }
}

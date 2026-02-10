using Ardalis.Result;
using MediatR;

namespace Warehouse.Web.Orders.UseCases.Commands
{
    internal record DeleteOrderCommand(long Id) : IRequest<Result>;
    internal class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;

        public DeleteOrderCommandHandler(IOrderRepository orderRepository, ICurrentUser currentUser)
        {
            _orderRepository = orderRepository;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id);

            if (order is null)
            {
                return Result.NotFound();
            }
            else
            {
                order.Delete(_currentUser.FullName, _currentUser.StoreName);

                await _orderRepository.DeleteAsync(order);
                await _orderRepository.SaveChangesAsync();

                return Result.Success();
            }
        }
    }
}
using Ardalis.Result;
using MediatR;

namespace Warehouse.Web.Operations.UseCases.Commands;

internal record DeleteOperationCommand(long Id) : IRequest<Result>;
internal class DeleteOperationCommandHandler : IRequestHandler<DeleteOperationCommand, Result>
{
    private readonly IOperationRepository _operationRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteOperationCommandHandler(IOperationRepository operationRepository, ICurrentUser currentUser)
    {
        _operationRepository = operationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _operationRepository.GetByIdAsync(request.Id);

        if (operation is null)
            return Result.NotFound();

        Operation? receiveToUpdate = null;

        if (operation.Type == OperationType.Send)
        {
            receiveToUpdate = await _operationRepository.GetByParentIdAsync(operation.Id);
        }

        operation.Delete(_currentUser.FullName, _currentUser.StoreName);

        if (receiveToUpdate is not null)
        {
            receiveToUpdate.Delete(_currentUser.FullName, _currentUser.StoreName);
            await _operationRepository.DeleteAsync(receiveToUpdate);
        }

        await _operationRepository.DeleteAsync(operation);
        await _operationRepository.SaveChangesAsync();

        return Result.Success();
    }
}
using Ardalis.Result;
using MediatR;

namespace Warehouse.Web.Catalog.UseCases.Commands;

internal record DeleteProductCommand(long Id) : IRequest<Result>;
internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteProductCommandHandler(IProductRepository productRepository, ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);

        if (product == null)
            return Result.NotFound();

        product.Delete(_currentUser.FullName, _currentUser.StoreName);

        await _productRepository.DeleteAsync(product);
        await _productRepository.SaveChangesAsync();

        return Result.Success();
    }
}

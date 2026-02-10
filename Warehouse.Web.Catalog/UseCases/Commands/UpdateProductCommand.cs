using Ardalis.Result;
using MediatR;

namespace Warehouse.Web.Catalog.UseCases.Commands;
internal record UpdateProductCommand(long Id, string Name, string? Manufacturer, string Unit, decimal BuyPrice, decimal SellPrice, int Limit) : IRequest<Result>;
internal class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);

        if (product == null)
            return Result.NotFound();

        product.Update(_currentUser.FullName, _currentUser.StoreName, request.Name, request.Unit, request.BuyPrice, request.SellPrice, request.Limit, request.Manufacturer);

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        return Result.Success();
    }
}

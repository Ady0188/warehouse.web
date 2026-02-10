using Ardalis.Result;
using MediatR;

namespace Warehouse.Web.Catalog.UseCases.Commands;
internal record CreateProductCommand(string Name, string? Manufacturer, string Unit, decimal BuyPrice, decimal SellPrice, int Limit) : IRequest<Result>;
internal class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUser _currentUser;

    public CreateProductCommandHandler(IProductRepository productRepository, ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        //var exists = await _productRepository.ExistsByNameAsync(request.Name);

        //if (exists)
        //    return Result.Conflict();

        var product = Product.Create(_currentUser.FullName, _currentUser.StoreName, request.Name.Trim(), request.Unit, request.BuyPrice, request.SellPrice, request.Limit, request.Manufacturer?.Trim());

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return Result.Success();
    }
}

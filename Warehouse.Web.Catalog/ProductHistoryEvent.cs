using Warehouse.Web.Contracts;
using static Warehouse.Web.Catalog.Product;

namespace Warehouse.Web.Catalog;

internal class ProductHistoryEvent : DomainEventBase
{
    public ProductHistoryEvent(Product newProduct, ProductSnapshot? oldProduct, HistoryMethod method, string? userName, string? userStoreName)
    {
        OldProduct = oldProduct;
        NewProduct = newProduct;
        Method = method;
        UserName = userName;
        UserStoreName = userStoreName;
    }
    public ProductSnapshot? OldProduct { get; }
    public Product NewProduct { get; }
    public HistoryMethod Method { get; }
    public string? UserName { get; }
    public string? UserStoreName { get; }
}
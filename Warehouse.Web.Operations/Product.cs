using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Warehouse.Web.Operations.Operation;

namespace Warehouse.Web.Operations
{
    internal class Product
    {
        public long Id { get; private set; }
        public long ProductId { get; private set; }
        public int Code { get; private set; }
        public string Name { get; private set; }
        public string Unit { get; private set; }
        public string Manufacturer { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }
        public decimal BuyPrice { get; private set; }
        public decimal SellPrice { get; private set; }

        //For Audit
        public int Difference { get; private set; }

        private Product() { }

        public sealed record ProductSnapshot(long Id, long ProductId, int Code, string Name, string Unit, string Manufacturer, int Quantity, decimal Price, decimal BuyPrice, decimal SellPrice, int Difference);
        public ProductSnapshot ToSnapshot() =>
            new(Id, ProductId, Code, Name, Unit, Manufacturer, Quantity, Price, BuyPrice, SellPrice, Difference);

        public Product(long productId, int code, string name, decimal price, decimal buyPrice, decimal sellPrice, int quantity, string manufacturer, string unit, int difference)
        {
            ProductId = productId;
            Code = code;
            Name = name;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            Quantity = quantity;
            Manufacturer = manufacturer;
            Unit = unit;
            Price = price;
            Difference = difference;
        }

        public void IncreaseQuantity(int amount)
        {
            if (amount <= 0) throw new ArgumentException(nameof(amount));
            Quantity += amount;
        }

        public void SetQuantity(int newQuantity)
        {
            if (newQuantity < 0) throw new ArgumentException(nameof(newQuantity));
            Quantity = newQuantity;
        }

        public void UpdateProduct(long productId, int code, string name, decimal price, decimal buyPrice, decimal sellPrice, int quantity, string manufacturer, string unit, int difference, OperationType type)
        {
            if (type != OperationType.Audit && quantity < 0) throw new ArgumentException(nameof(quantity));

            ProductId = productId;
            Code = code;
            Name = name;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            Quantity = quantity;
            Manufacturer = manufacturer;
            Unit = unit;
            Price = price;
            Difference = difference;
        }
    }
}

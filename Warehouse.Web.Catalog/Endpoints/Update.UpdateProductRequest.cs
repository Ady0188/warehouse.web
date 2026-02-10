using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Catalog.Endpoints;

public record UpdateProductRequest([FromRoute] long Id, string Name, string? Manufacturer, string Unit, decimal BuyPrice, decimal SellPrice, int limit);

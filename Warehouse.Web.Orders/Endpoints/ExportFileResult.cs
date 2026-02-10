namespace Warehouse.Web.Orders.Endpoints;

public sealed record ExportFileResult(
    byte[] Bytes,
    string FileName,
    string ContentType);
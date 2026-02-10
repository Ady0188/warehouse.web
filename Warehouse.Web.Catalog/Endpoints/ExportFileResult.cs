namespace Warehouse.Web.Catalog.Endpoints;

public sealed record ExportFileResult(
    byte[] Bytes,
    string FileName,
    string ContentType);

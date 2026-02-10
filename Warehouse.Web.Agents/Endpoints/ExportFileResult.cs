namespace Warehouse.Web.Agents.Endpoints;

public sealed record ExportFileResult(
    byte[] Bytes,
    string FileName,
    string ContentType);

using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Client.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public ErrorDetails Errors { get; set; }
}

public class ErrorDetails
{
    public List<string> GeneralErrors { get; set; }
}
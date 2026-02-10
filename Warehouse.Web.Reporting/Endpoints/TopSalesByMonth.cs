using FastEndpoints;
using System.Security.Claims;

namespace Warehouse.Web.Reporting.Endpoints;

internal record TopSalesByMonthRequest();
internal record TopSalesByMonthResponse();
internal class TopSalesByMonth : Endpoint<TopSalesByMonthRequest, TopSalesByMonthResponse>
{
    public override void Configure()
    {
        Get("/topsales");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TopSalesByMonthRequest req, CancellationToken ct)
    {
        //var report = _reportService.ReachInSqlQuery(req.Month, req.Year);
        //var response = new TopSalesByMonthResponse( Report = report);

        //await SendAsync(response);
    }
}
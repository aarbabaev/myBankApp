using AA.FileGenerator.Models;
using IO.Swagger.Model;
using Microsoft.AspNetCore.Mvc;

namespace AA.FileGenerator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExcelGeneratorController: ControllerBase
{
    private IDataAnalyticsProvider _analyticsProvider;
    private readonly ChartRenderService _chartRenderService;
    private readonly PdfRenderService _pdfRenderService;

    public ExcelGeneratorController(IDataAnalyticsProvider analyticsProvider, ChartRenderService chartRenderService, PdfRenderService pdfRenderService)

    {
        _analyticsProvider = analyticsProvider;
        _chartRenderService = chartRenderService;
        _pdfRenderService = pdfRenderService;

    }
    
    [HttpGet(Name = "Generate")]
    public async Task<IActionResult> Generate()
    {
        var request = new ElectricityRequestModel()
        {
            DataTypes = [DataType.Active],
            DataUnitId = 1,
            Ids = [1, 2, 3],
            FilterType = FilterType.CompanyId,
            Granularity = Granularity.Monthly,
            DateFrom = 1739491200,
            DateTo = 1771027199,
            RequestSource = "ExcelGenerator"
        };
        var result = await _analyticsProvider.GetDataAsync(request);


        return Ok(result);
    }

    private async Task<List<ConsumptionChunk>> GetData()
    {
        var requestAnalytics = new ElectricityRequestModel()
        {
            DataTypes = [DataType.Active],
            DataUnitId = 1,
            Ids = [1, 2, 3],
            FilterType = FilterType.CompanyId,
            Granularity = Granularity.Monthly,
            DateFrom = 1739491200,
            DateTo = 1771027199,
            RequestSource = "ExcelGenerator"
        };
        var resultAnalytics = await _analyticsProvider.GetDataAsync(requestAnalytics);

        var chartData = resultAnalytics.Data.First().Chunks.OrderBy(x => x.Date).ToList();
        return chartData;
    }

    [HttpPost("chart/preview")]
    public async Task<IActionResult> PreviewChart()
    {
        var chartData = await GetData();
        var request = new ChartPreviewRequest
        {
            Title = "Eleclricity Consumption",
            XLabel = "Date",
            YLabel = "kWh,",
            Values = chartData.Select(x => x.Value).ToList(),
            Labels = chartData.Select(x => x.Date.ToString("MMM")).ToList()
        };
        var pngBytes = _chartRenderService.RenderBarChartPng(request);

        return File(pngBytes, "image/png");
    }

    [HttpPost("chart/export/png")]
    public async Task<IActionResult> ExportChartPng()
    {
        var chartData = await GetData();
        var request = new ChartPreviewRequest
        {
            Title = "Eleclricity Consumption",
            XLabel = "Date",
            YLabel = "kWh,",
            Values = chartData.Select(x => x.Value).ToList(),
            Labels = chartData.Select(x => x.Date.ToString("MMM")).ToList()
        };

        var pngBytes = _chartRenderService.RenderBarChartPng(request);

        return File(pngBytes, "image/png", "chart.png");
    }

    [HttpPost("chart/export/pdf")]
    public async Task<IActionResult> ExportChartPdf()
    {
        var chartData = await GetData();
        var request = new ChartPreviewRequest
        {
            Title = "Eleclricity Consumption",
            XLabel = "Date",
            YLabel = "kWh,",
            Values = chartData.Select(x => x.Value).ToList(),
            Labels = chartData.Select(x => x.Date.ToString("MMM")).ToList()
        };
        var pdfBytes = _pdfRenderService.RenderChartPdf(request);

        return File(pdfBytes, "application/pdf", "chart.pdf");
    }

    
}


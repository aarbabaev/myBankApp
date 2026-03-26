using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using Colors = ScottPlot.Colors;
using ImageFormat = ScottPlot.ImageFormat;


namespace AA.FileGenerator.Models;

public interface IDataAnalyticsProvider
{
    Task<ConsumptionResponseModel> GetDataAsync(ElectricityRequestModel request); 
}

public class DataAnalyticsProvider(DependencyInjection.ProviderOptions options) : IDataAnalyticsProvider
{
    private Configuration Configuration { get; } = new()
    {
        BasePath = options.ApiUrl,
        DefaultHeader = new Dictionary<string, string?>()
        {
            ["x-api-key"] = options.ApiKey
        }
    };

    public async Task<ConsumptionResponseModel> GetDataAsync(ElectricityRequestModel request)
    {
        var clientApi = new ElectricityApi(Configuration);
        var response = await clientApi.ApiElectricityDataPostAsync(request);

        return response;
    }
}


public static class DependencyInjection
{
    public class ProviderOptions
    {
        public string? ApiUrl { get; set; }
        public string? ApiKey { get; set; }
    }

    public static IServiceCollection AddProvider(this IServiceCollection services, Action<ProviderOptions> setupAction)
    {
        var options = new ProviderOptions();
        setupAction?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IDataAnalyticsProvider, DataAnalyticsProvider>();
        return services;
    }
}


public sealed class ChartPreviewRequest
{
    public string Title { get; init; } = "Chart Preview";
    public string XLabel { get; init; } = "X";
    public string YLabel { get; init; } = "Y";
    public IReadOnlyList<double> Values { get; init; } = [];
    public IReadOnlyList<string>? Labels { get; init; }
}


public sealed class ChartRenderService
{
    public byte[] RenderLineChartPng(ChartPreviewRequest request)
    {
        if (request.Values.Count == 0)
        {
            throw new ArgumentException("Values collection must contain at least one item.");
        }

        var plot = new Plot();

        double[] xs = Enumerable.Range(0, request.Values.Count)
            .Select(i => (double)i)
            .ToArray();

        double[] ys = request.Values.ToArray();

        plot.Add.Scatter(xs, ys);

        plot.Title(request.Title);
        plot.XLabel(request.XLabel);
        plot.YLabel(request.YLabel);

        if (request.Labels is { Count: > 0 } labels)
        {
            var tickPositions = Enumerable.Range(0, Math.Min(labels.Count, request.Values.Count))
                .Select(i => (double)i)
                .ToArray();

            var tickLabels = labels.Take(request.Values.Count).ToArray();

            plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickPositions, tickLabels);
        }

        plot.ShowLegend();
        plot.Axes.AutoScale();

        return plot.GetImageBytes(800, 450, ImageFormat.Png);
    }
    
    public byte[] RenderBarChartPng(ChartPreviewRequest request)
    {
        if (request.Values.Count == 0)
            throw new ArgumentException("Values collection must contain at least one item.");

        var plot = new Plot();

        double[] positions = Enumerable.Range(0, request.Values.Count)
            .Select(i => (double)i)
            .ToArray();

        var bars = request.Values
            .Select((value, index) => new Bar
            {
                Position = index,
                Value = value
            })
            .ToArray();

        var barPlot = plot.Add.Bars(bars);
        barPlot.Color = Colors.DodgerBlue;

        plot.Title(request.Title);
        plot.XLabel(request.XLabel);
        plot.YLabel(request.YLabel);

        ApplyLabels(plot, request);

        plot.Axes.AutoScale();

        return plot.GetImageBytes(800, 450, ImageFormat.Png);
    }

    private static void ApplyLabels(Plot plot, ChartPreviewRequest request)
    {
        if (request.Labels is not { Count: > 0 })
            return;

        var tickPositions = Enumerable.Range(0, Math.Min(request.Labels.Count, request.Values.Count))
            .Select(i => (double)i)
            .ToArray();

        var tickLabels = request.Labels.Take(request.Values.Count).ToArray();

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickPositions, tickLabels);
    }

}


public sealed class PdfRenderService
{
    private readonly ChartRenderService _chartRenderService;

    public PdfRenderService(ChartRenderService chartRenderService)
    {
        _chartRenderService = chartRenderService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderChartPdf(ChartPreviewRequest request)
    {
        var chartBytes = _chartRenderService.RenderBarChartPng(request);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    column.Item().Text(request.Title ?? "Chart")
                        .FontSize(20)
                        .Bold();

                    column.Item().Image(chartBytes);

                    column.Item().Text($"X: {request.XLabel}");
                    column.Item().Text($"Y: {request.YLabel}");
                });
            });
        }).GeneratePdf();
    }
}

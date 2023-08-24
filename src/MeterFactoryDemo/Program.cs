using AspNetCoreEnricher;
using Microsoft.AspNetCore.Http.Features;

namespace MeterFactoryDemo;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            EnvironmentName = "Test"
        });
        builder.Services.AddEnricher<DemoEnricher1>();
        builder.Services.AddEnricher<DemoEnricher2>();
        builder.Services.AddSingleton<IHostedService, HttpServerMetricsPublisher>();
        builder.Services.Configure<HttpServerMetricsPublisherOptions>(o =>
        {
            o.Mapping.Add("status-code", "my-cool-status-code");
        });
        var app = builder.Build();

        app.MapGet("/", (HttpContext context) =>
        {
            context.Features.Get<IHttpMetricsTagsFeature>()?.Tags
                .Add(new KeyValuePair<string, object?>("custom-tag", "value!"));

            return "Hello World!";
        });
        app.MapGet("/error", (HttpContext context) =>
        {
            throw new Exception("Exception!");
        });

        app.Run();
    }
}
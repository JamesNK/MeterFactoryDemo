namespace MeterFactoryDemo;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<IHostedService, HttpServerMetricsPublisher>();
        builder.Services.Configure<HttpServerMetricsPublisherOptions>(o =>
        {
            o.Mapping.Add("status-code", "my-cool-status-code");
        });
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
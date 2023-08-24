using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Telemetry.Testing.Metering;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo.Tests
{
    public class HttpRequestDuration : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public HttpRequestDuration(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Get_RequestDuration()
        {
            // Arrange
            var meterFactory = _factory.Services.GetRequiredService<IMeterFactory>();
            var collector = new MetricCollector<double>(meterFactory, "Microsoft.AspNetCore.Hosting", "http.server.request.duration");
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.Equal("Hello World!", await response.Content.ReadAsStringAsync());

            await collector.WaitForMeasurementsAsync(minCount: 1);
            Assert.Collection(collector.GetMeasurementSnapshot(),
                measurement =>
                {
                    Assert.Equal("value!", (string)measurement.Tags["custom-tag"]!);
                });
        }
    }
}
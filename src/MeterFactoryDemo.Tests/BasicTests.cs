using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo.Tests
{
    public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public BasicTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Get_RequestCounterIncreased()
        {
            // Arrange
            var meterFactory = _factory.Services.GetRequiredService<IMeterFactory>();
            var instrumentRecorder = new InstrumentRecorder<double>(meterFactory, "Microsoft.AspNetCore.Hosting", "request-duration");
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.Equal("Hello World!", await response.Content.ReadAsStringAsync());

            Assert.Collection(instrumentRecorder.GetMeasurements(),
                measurement =>
                {
                    Assert.Equal("http", measurement.Tags.ToArray().Single(t => t.Key == "scheme").Value);
                    Assert.Equal("GET", measurement.Tags.ToArray().Single(t => t.Key == "method").Value);
                    Assert.Equal("/", measurement.Tags.ToArray().Single(t => t.Key == "route").Value);
                });
        }
    }
}
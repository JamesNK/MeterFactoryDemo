using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo.Tests
{
    public class CustomTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public CustomTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Get_RequestCounterIncreased()
        {
            // Arrange
            var meterFactory = _factory.Services.GetRequiredService<IMeterFactory>();
            var instrumentRecorder = new InstrumentRecorder<double>(meterFactory, "MeterFactoryDemo.Http", "demo-http-server-request-duration");
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.Equal("Hello World!", await response.Content.ReadAsStringAsync());

            Assert.Collection(instrumentRecorder.GetMeasurements(),
                measurement =>
                {
                    Assert.Equal(200, (int)measurement.Tags.ToArray().Single(t => t.Key == "my-cool-status-code").Value);
                });
        }
    }
}
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

            await Task.Delay(100); // TODO: Replace with MetricsCollection.WaitForMeasurements

            // This is currently flaky. Counter value is added once response is complete.
            // There is a race between the client reading the response + assert and the counter.
            // Need InstrumentRecorder.WaitForMeasurementsAsync API to make this test work reliably.
            Assert.Collection(instrumentRecorder.GetMeasurements(),
                measurement =>
                {
                    // Built-in
                    Assert.Equal("http", measurement.Tags.ToArray().Single(t => t.Key == "scheme").Value);
                    Assert.Equal("GET", measurement.Tags.ToArray().Single(t => t.Key == "method").Value);
                    Assert.Equal("/", measurement.Tags.ToArray().Single(t => t.Key == "route").Value);

                    // From enrichers
                    Assert.Equal("Value!", measurement.Tags.ToArray().Single(t => t.Key == "TagName1").Value);
                    Assert.Equal("Value!", measurement.Tags.ToArray().Single(t => t.Key == "TagName2").Value);
                });
        }

        [Fact]
        public async Task Get_MeterListener_RequestCounterIncreased()
        {
            // Arrange
            var requestDurationReceivedTcs = new TaskCompletionSource<Measurement<double>>();

            var meterFactory = _factory.Services.GetRequiredService<IMeterFactory>();
            var meterListener = new MeterListener();
            meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Scope == meterFactory &&
                    instrument.Meter.Name == "Microsoft.AspNetCore.Hosting" &&
                    instrument.Name == "request-duration")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
            {
                requestDurationReceivedTcs.TrySetResult(new Measurement<double>(measurement, tags));
            });
            meterListener.Start();

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.Equal("Hello World!", await response.Content.ReadAsStringAsync());

            var requestDuration = await requestDurationReceivedTcs.Task;

            Assert.Equal("http", requestDuration.Tags.ToArray().Single(t => t.Key == "scheme").Value);
            Assert.Equal("GET", requestDuration.Tags.ToArray().Single(t => t.Key == "method").Value);
            Assert.Equal("/", requestDuration.Tags.ToArray().Single(t => t.Key == "route").Value);
        }
    }
}
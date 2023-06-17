using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo
{
    public class HttpServerMetricsPublisher : IHostedService
    {
        private readonly IMeterFactory _meterFactory;
        private readonly HttpServerMetricsPublisherOptions _options;
        private readonly Meter _meter;
        private readonly Histogram<double> _serverRequestDuration;
        private MeterListener? _meterListener;

        public HttpServerMetricsPublisher(IMeterFactory meterFactory, IOptions<HttpServerMetricsPublisherOptions> options)
        {
            _meterFactory = meterFactory;
            _options = options.Value;

            // Create the new meter and counter.
            _meter = _meterFactory.Create("MeterFactoryDemo.Http");
            _serverRequestDuration = _meter.CreateHistogram<double>(
                "demo-http-server-request-duration",
                unit: "ms");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Listen to the built-in ASP.NET Core counter.
            _meterListener = new MeterListener();
            _meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Scope == _meterFactory &&
                    instrument.Meter.Name == "Microsoft.AspNetCore.Hosting" &&
                    instrument.Name == "request-duration")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            _meterListener.SetMeasurementEventCallback<double>(OnMeasurementRecorded);
            _meterListener.Start();

            return Task.CompletedTask;
        }

        private void OnMeasurementRecorded(Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
            // Modify the recorded built-in measurement value and republish to the new counter.
            var tagList = new TagList();
            for (int i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                if (_options.Mapping.TryGetValue(tag.Key, out var newKey))
                {
                    tagList.Add(new KeyValuePair<string, object?>(newKey, tag.Value));
                }
            }

            // Convert seconds to milliseconds.
            var milliseconds = measurement / 1000d;

            _serverRequestDuration.Record(milliseconds, tagList);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _meterListener?.Dispose();

            return Task.CompletedTask;
        }
    }
}

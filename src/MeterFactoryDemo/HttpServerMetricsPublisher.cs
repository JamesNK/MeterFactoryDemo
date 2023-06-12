using Microsoft.Extensions.Diagnostics.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo
{
    public class HttpServerMetricsPublisher : IHostedService
    {
        private readonly IMeterFactory _meterFactory;
        private readonly Meter _meter;
        private readonly Histogram<double> _serverRequestDuration;
        private MeterListener? _meterListener;

        public HttpServerMetricsPublisher(IMeterFactory meterFactory)
        {
            _meterFactory = meterFactory;

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
            foreach (var tag in tags)
            {
                switch (tag.Key)
                {
                    case "scheme":
                    case "protocol":
                        // ignore
                        break;
                    default:
                        tagList.Add(tag);
                        break;
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

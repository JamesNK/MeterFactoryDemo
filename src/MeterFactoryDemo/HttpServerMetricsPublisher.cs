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
            _meter = _meterFactory.Create("MeterFactoryDemo.Http");
            _serverRequestDuration = _meter.CreateHistogram<double>("demo-http-server-request-duration");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _meterListener = new MeterListener();
            _meterListener.InstrumentPublished = MeterPublished;
            _meterListener.SetMeasurementEventCallback<double>(OnMeasurementRecorded);
            _meterListener.Start();

            return Task.CompletedTask;
        }

        private void OnMeasurementRecorded(Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
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

            _serverRequestDuration.Record(measurement / 1000d, tagList);
        }

        private void MeterPublished(Instrument instrument, MeterListener listener)
        {
            if (instrument.Meter.Scope == _meterFactory &&
                instrument.Meter.Name == "Microsoft.AspNetCore.Hosting" &&
                instrument.Name == "request-duration")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _meterListener?.Dispose();

            return Task.CompletedTask;
        }
    }
}

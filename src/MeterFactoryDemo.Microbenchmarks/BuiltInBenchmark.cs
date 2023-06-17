using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo.Microbenchmarks
{
    public class BuiltInBenchmark
    {
        private readonly Meter _meter;
        private readonly Histogram<double> _requestDuration;
        private Measurement<double> _counterValue;
        private List<KeyValuePair<string, object?>> _tags = new List<KeyValuePair<string, object?>>();

        [Params(0, 3, 15)]
        public int TagCount { get; set; }

        public BuiltInBenchmark()
        {
            var services = new ServiceCollection();
            services.AddMetrics();
            var serviceProvider = services.BuildServiceProvider();
            var meterFactory = serviceProvider.GetRequiredService<IMeterFactory>();

            _meter = meterFactory.Create("Microsoft.AspNetCore.Hosting");
            _requestDuration = _meter.CreateHistogram<double>("request-duration", "s");

            var meterListener = new MeterListener();
            meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "Microsoft.AspNetCore.Hosting" && instrument.Name == "request-duration")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            meterListener.SetMeasurementEventCallback<double>((Instrument instrument, double measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state) =>
            {
                _counterValue = new Measurement<double>(measurement, tags);
            });
            meterListener.Start();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            for (int i = 0; i < TagCount; i++)
            {
                _tags.Add(new KeyValuePair<string, object?>($"Key{i}", $"Value{i}"));
            }
        }

        [Benchmark]
        public void RecordDuration()
        {
            var tags = new TagList();
            for (int i = 0; i < _tags.Count; i++)
            {
                tags.Add(_tags[i]);
            }

            _counterValue = default;
            _requestDuration.Record(0.5, tags);

            if (_counterValue.Value == 0)
            {
                throw new Exception("Counter value wasn't set.");
            }
        }
    }
}

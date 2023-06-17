using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MeterFactoryDemo.Microbenchmarks
{
    [MemoryDiagnoser]
    public class PublisherBenchmark
    {
        private readonly IMeterFactory _meterFactory;
        private readonly Meter _meter;
        private readonly Histogram<double> _requestDuration;
        private Measurement<double> _counterValue;
        private List<KeyValuePair<string, object?>> _tags = new List<KeyValuePair<string, object?>>();

        [Params(0, 3, 15)]
        public int TagCount { get; set; }

        public PublisherBenchmark()
        {
            var services = new ServiceCollection();
            services.AddMetrics();
            var serviceProvider = services.BuildServiceProvider();
            _meterFactory = serviceProvider.GetRequiredService<IMeterFactory>();

            _meter = _meterFactory.Create("Microsoft.AspNetCore.Hosting");
            _requestDuration = _meter.CreateHistogram<double>("request-duration", "s");

            var meterListener = new MeterListener();
            meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "MeterFactoryDemo.Http" && instrument.Name == "demo-http-server-request-duration")
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
            var options = new HttpServerMetricsPublisherOptions();

            for (int i = 0; i < TagCount; i++)
            {
                var key = $"Key{i}";
                _tags.Add(new KeyValuePair<string, object?>(key, $"Value{i}"));

                if (i % 2 == 0)
                {
                    options.Mapping.Add(key, $"mapped-{key}");
                }
            }

            var publisher = new HttpServerMetricsPublisher(_meterFactory, Options.Create(options));
            publisher.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
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

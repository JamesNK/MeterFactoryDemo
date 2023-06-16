﻿using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Threading.Tasks.Sources;

namespace MeterFactoryDemo.Microbenchmarks
{
    public class BuiltInBenchmark
    {
        private readonly Meter _meter;
        private readonly Histogram<double> _requestDuration;
        private readonly object _statusCode = 200;
        private Measurement<double> _counterValue;

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

        [Benchmark]
        public void RecordDuration()
        {
            _counterValue = default;
            _requestDuration.Record(
                0.5,
                new TagList
                {
                    new KeyValuePair<string, object?>("status-code", _statusCode),
                    new KeyValuePair<string, object?>("protocol", "HTTP/1.1"),
                    new KeyValuePair<string, object?>("route", "api/product/{id}"),
                });

            if (_counterValue.Value == 0)
            {
                throw new Exception("Counter value wasn't set.");
            }
        }
    }
}

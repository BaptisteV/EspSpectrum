using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using EspSpectrum.PerformanceTests;

var config = DefaultConfig.Instance
    .AddJob(Job
         .MediumRun
         .WithLaunchCount(1)
         .WithToolchain(InProcessEmitToolchain.Instance));

//BenchmarkRunner.Run<FftRecorderTests>(config);
//BenchmarkRunner.Run<FakeLoopbackWaveInTests>(config);
//BenchmarkRunner.Run<ThreadedChannelTests>(config);
BenchmarkRunner.Run<PartialDataReaderTests>(config);
//BenchmarkRunner.Run<FftProcessorTests>(config);
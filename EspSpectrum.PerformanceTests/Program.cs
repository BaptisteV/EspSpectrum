using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using EspSpectrum.PerformanceTests;

var config = DefaultConfig.Instance
    .AddJob(Job
         .ShortRun
         .WithLaunchCount(1)
         .WithToolchain(InProcessEmitToolchain.Instance));

//BenchmarkRunner.Run<FftRecorderTests>(config);
//BenchmarkRunner.Run<FakeLoopbackWaveInTests>(config);
//BenchmarkRunner.Run<PartialDataReaderTests>(config);
BenchmarkRunner.Run<FftProcessorTests>(config);
//BenchmarkRunner.Run<EspSpectrumRunnerTests>(config);
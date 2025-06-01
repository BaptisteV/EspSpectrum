using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using EspSpectrum.PerformanceTests;

var config = DefaultConfig.Instance
    .AddJob(Job
         .ShortRun.WithToolchain(InProcessEmitToolchain.Instance));

//BenchmarkRunner.Run<FftTests>(config);
//BenchmarkRunner.Run<ThreadedChannelTests>(config);
//BenchmarkRunner.Run<ReadChannelTests>(config);
BenchmarkRunner.Run<FftProcessorTests>(config);
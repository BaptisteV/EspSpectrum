using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using EspSpectrum.PerformanceTests;

var config = DefaultConfig.Instance
    .AddJob(Job
         .Default
         //.WithLaunchCount(1)
         .WithToolchain(InProcessEmitToolchain.Instance)
         ).AddDiagnoser(ExceptionDiagnoser.Default)
         .AddDiagnoser(MemoryDiagnoser.Default)
         .AddDiagnoser(ThreadingDiagnoser.Default);

//BenchmarkRunner.Run<FftRecorderTests>(config);
//BenchmarkRunner.Run<FakeLoopbackWaveInTests>(config);
//BenchmarkRunner.Run<PartialDataReaderTests>(config);
//BenchmarkRunner.Run<FftProcessorTests>(config);
BenchmarkRunner.Run<EspSpectrumRunnerTests>(config);
//BenchmarkRunner.Run<PreciseSleepTests>(config);
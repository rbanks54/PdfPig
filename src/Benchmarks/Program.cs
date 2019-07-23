using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args,
                DefaultConfig.Instance);
    }
}

using System.Reflection;
using BenchmarkDotNet.Running;

/*
 * Welcome to benchmarks
 */

BenchmarkSwitcher.FromAssemblies(new[]
{
    Assembly.GetAssembly(typeof(성능연구소.Disqord.Library)),
    Assembly.GetAssembly(typeof(성능연구소.Remora.Library)),
    Assembly.GetAssembly(typeof(성능연구소.DSharpPlus.Library)),
    Assembly.GetAssembly(typeof(성능연구소.Discord.Net.Library)), 
}).Run(args);
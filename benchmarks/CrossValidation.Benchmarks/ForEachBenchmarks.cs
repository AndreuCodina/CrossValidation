using System.Collections.Frozen;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using CrossValidation.Exceptions;

namespace CrossValidation.Benchmarks;

[DotTraceDiagnoser]
[MemoryDiagnoser]
[RankColumn]
public class ForEachBenchmarks
{
    private static readonly List<int> List = Enumerable.Range(1, 4).ToList();
    private static readonly FrozenSet<int> FrozenSet = Enumerable.Range(1, 4).ToFrozenSet();

    [Benchmark]
    public int IterateList()
    {
        foreach (var item in List)
        {
            return item;
        }

        return 0;
    }
    
    [Benchmark]
    public int IterateFrozenSet()
    {
        foreach (var item in FrozenSet)
        {
            return item;
        }

        return 0;
    }
}
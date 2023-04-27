namespace BlogSampleCodes.ConcurrentStructurePerformanceCheck;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

internal class ConcurrentDictionaryChecker
{
    private Stopwatch Stopwatch { get; } = new Stopwatch();

    private long GetAddTime(IDictionary<int, int> dict, int key, int value)
    {
        var now = DateTime.UtcNow;
        dict.TryAdd(key, value);
        var elapsedTick = (DateTime.UtcNow - now).Ticks;
        return elapsedTick;
    }

    private long GetTotalAddTime(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        var now = DateTime.UtcNow;
        for (var i = 0; i < count; i++)
        {
            dict.TryAdd(i, i);
        }
        var elapsedTick = (DateTime.UtcNow - now).Ticks;
        return elapsedTick;
    }

    private List<long> GetEachAddTimes(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        var result = Enumerable.Range(0, count).Select(index => GetAddTime(dict, index, index)).ToList();
        return result;
    } 

    private long GetRemoveTime(IDictionary<int, int> dict, int key)
    {
        var now = DateTime.UtcNow;
        dict.Remove(key);
        var elapsedTick = (DateTime.UtcNow - now).Ticks;
        return elapsedTick;
    }

    private long GetTotalRemoveTime(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        var iter = Enumerable.Range(0, count).Select(index => dict.TryAdd(index, index) ? index : index).ToList();
        var now = DateTime.UtcNow;
        for (var i = 0; i < count; i++) 
        {
            dict.Remove(i);
        }
        var elapsedTick = (DateTime.UtcNow - now).Ticks;
        return elapsedTick;
    }

    private List<long> GetEachRemoveTimes(IDictionary<int, int> dict, int count) 
    {
        dict.Clear();
        var result = Enumerable.Range(0, count).
            Select(index => dict.TryAdd(index, index) ? index : index).
            Select(index => GetRemoveTime(dict, index)).
            ToList();
        return result;
    }

    private long GetIterationTime(IDictionary<int, int> dict, int count)
    {
        dict.Clear();
        Enumerable.Range(0, count).Select(index => dict.TryAdd(index, index)).ToList();
        var now = DateTime.UtcNow;
        var _ = dict.Values.ToList();
        var elapsedTick = (DateTime.UtcNow - now).Ticks;
        return elapsedTick;
    }

    public void GetResult(int capacity = 1) 
    {
        var thresholdTick = TimeSpan.TicksPerMillisecond;
        var repeatCount = 1000000;
        // add statistic
        {
            var stringBuilder = new StringBuilder();
            var concurrentDict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: capacity);
            var concurrentResult = GetEachAddTimes(concurrentDict, count: repeatCount);
            var concurrentTotalTime = Enumerable.Range(0, 100).
                Select(_ => 
                {
                    var dict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: capacity);
                    return GetTotalAddTime(dict, repeatCount);
                }).
                Average() / TimeSpan.TicksPerMillisecond;

            var nonConcurrentDict = new Dictionary<int, int>(capacity: capacity);
            var nonConcurrentResult = GetEachAddTimes(nonConcurrentDict, count: repeatCount);
            var nonConcurrentTotalTime = Enumerable.Range(0, 100).
                Select(_ => 
                {
                    var dict = new Dictionary<int, int>(capacity: capacity);
                    return GetTotalAddTime(dict, repeatCount);
                }).
                Average() / TimeSpan.TicksPerMillisecond;

            stringBuilder.Append($"Capacity,{capacity}\n");
            stringBuilder.Append($"Type,{nameof(ConcurrentDictionary<int, int>)},{nameof(Dictionary<int, int>)}\n");
            stringBuilder.Append($"Total,{concurrentTotalTime},{nonConcurrentTotalTime}\n");
            stringBuilder.Append($"Max,{concurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond},{nonConcurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond}\n");
            foreach (var (index, resultPair) in concurrentResult.Zip(nonConcurrentResult).Select((pair,index) =>(index,pair))) 
            {
                if (resultPair.First > thresholdTick || resultPair.Second > thresholdTick)
                {
                    stringBuilder.Append($"{index + 1},{resultPair.First / (double)TimeSpan.TicksPerMillisecond},{resultPair.Second / (double)TimeSpan.TicksPerMillisecond}\n");
                }
            }

            Save(Directory.GetCurrentDirectory(), $"dict_capacity{capacity}_add_result", stringBuilder.ToString());
        }

        // remove statistic
        {
            var stringBuilder = new StringBuilder();
            var concurrentDict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: capacity);
            var concurrentResult = GetEachRemoveTimes(concurrentDict, count: repeatCount);
            var concurrentTotalTime = Enumerable.Range(0, 100).Select(_ => GetTotalRemoveTime(concurrentDict, repeatCount)).Average() / TimeSpan.TicksPerMillisecond;

            var nonConcurrentDict = new Dictionary<int, int>(capacity: capacity);
            var nonConcurrentResult = GetEachRemoveTimes(nonConcurrentDict, count: repeatCount);
            var nonConcurrentTotalTime = Enumerable.Range(0, 100).Select(_ => GetTotalRemoveTime(nonConcurrentDict, repeatCount)).Average() / TimeSpan.TicksPerMillisecond;

            stringBuilder.Append($"Capacity,{capacity}\n");
            stringBuilder.Append($"Type,{nameof(ConcurrentDictionary<int, int>)},{nameof(Dictionary<int, int>)}\n");
            stringBuilder.Append($"Total,{concurrentTotalTime},{nonConcurrentTotalTime}\n");
            stringBuilder.Append($"Max,{concurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond},{nonConcurrentResult.Max() / (double)TimeSpan.TicksPerMillisecond}\n");
            foreach (var (index, resultPair) in concurrentResult.Zip(nonConcurrentResult).Select((pair, index) => (index, pair)))
            {
                if (resultPair.First > thresholdTick || resultPair.Second > thresholdTick)
                {
                    stringBuilder.Append($"{index + 1},{resultPair.First / (double)TimeSpan.TicksPerMillisecond},{resultPair.Second / (double)TimeSpan.TicksPerMillisecond}\n");
                }
            }

            Save(Directory.GetCurrentDirectory(), $"dict_capacity{capacity}_remove_result", stringBuilder.ToString());
        }

        // iterate statistic
        {
            var stringBuilder = new StringBuilder();
            var concurrentDict = new ConcurrentDictionary<int, int>(concurrencyLevel: 1, capacity: capacity);
            var nonConcurrentDict = new Dictionary<int, int>(capacity: capacity);

            for(var i = 0; i < repeatCount; i++) 
            {
                concurrentDict.TryAdd(i, i);
                nonConcurrentDict.TryAdd(i, i);
            }

            var concurrentTotalTime = Enumerable.Range(0, 100).Select(_ => GetIterationTime(concurrentDict, repeatCount)).Average() / TimeSpan.TicksPerMillisecond;
            var nonConcurrentTotalTime = Enumerable.Range(0, 100).Select(_ => GetIterationTime(nonConcurrentDict, repeatCount)).Average() / TimeSpan.TicksPerMillisecond;

            stringBuilder.Append($"Capacity,{capacity}\n");
            stringBuilder.Append($"Type,{nameof(ConcurrentDictionary<int, int>)},{nameof(Dictionary<int, int>)}\n");
            stringBuilder.Append($"Total,{concurrentTotalTime},{nonConcurrentTotalTime}\n");

            Save(Directory.GetCurrentDirectory(), $"dict_capacity{capacity}_iter_result", stringBuilder.ToString());
        }
    }

    private void Save(string folder,string filename, string text) 
    {
        using (TextWriter writer = new StreamWriter($"{folder}/{filename}.csv", false, Encoding.UTF8))
        {
            writer.Write(text);
        }
    }
 
}

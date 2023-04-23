namespace LinqMemoryCheck;

using System;
using System.Linq;

public class LinqMemoryCheck
{
    private static long memoryDifference = 0;
    private static long lastPrintedMemoryUsage = 0;
    static void printMemoryUsage(string name)
    {
        long memory = GC.GetTotalMemory(true);
        long kb = 1024;
        long memoryUsage = memory / kb;
        memoryDifference = memoryUsage - lastPrintedMemoryUsage;
        lastPrintedMemoryUsage = memoryUsage;
        Console.WriteLine($"{name,23}:: difference: {memoryDifference}KB, memory: {memoryUsage}KB");
    }

    public void MemoryCheck() {
        printMemoryUsage("");
        var enumerator = Enumerable.Range(0, 20 * 1024);
        printMemoryUsage(nameof(enumerator));
        var list = enumerator.ToList();
        printMemoryUsage(nameof(list));
        var orderedEnumerator = enumerator.OrderBy(x => x);
        printMemoryUsage(nameof(orderedEnumerator));
        var select = enumerator.Select(x => x);
        printMemoryUsage(nameof(select));
        var whereAndSelect = enumerator.Where(x => true).Select(x => x);
        printMemoryUsage(nameof(whereAndSelect));
        var orderedList = enumerator.OrderBy(x => x).ToList();
        printMemoryUsage(nameof(orderedList));

        foreach (var iter_enumerator in enumerator)
        {
            printMemoryUsage(nameof(iter_enumerator));
            break;
        }
        foreach (var iter_list in list)
        {
            printMemoryUsage(nameof(iter_list));
            break;
        }
        foreach (var iter_orderedEnumerator in orderedEnumerator)
        {
            printMemoryUsage(nameof(iter_orderedEnumerator));
            break;
        }
        foreach (var iter_select in select)
        {
            printMemoryUsage(nameof(iter_select));
            break;
        }
        foreach (var iter_whereAndSelect in whereAndSelect)
        {
            printMemoryUsage(nameof(iter_whereAndSelect));
            break;
        }
        foreach (var iter_orderedList in orderedList)
        {
            printMemoryUsage(nameof(iter_orderedList));
            break;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using static System.Console;

var tcs = new TaskCompletionSource<Unit>();
var wrTcs = new WeakReference(tcs);
var task = NoFinallyAsync(tcs!.Task);
var wrTask = new WeakReference(task);

void GCCollectAndPulseCheck(string comment, int generation)
{
    WriteLine();
    WriteLine(comment);
    GC.Collect(generation);
    WriteLine($"GC.Collect({generation})");
    WriteLine(wrTask.Target != null ? "Task is alive" : "Task is dead");
    WriteLine(wrTcs.Target != null ? "TCS is alive" : "TCS is dead");
}

GCCollectAndPulseCheck("Initial state", 2);
tcs = null;
await Task.Delay(100);
GCCollectAndPulseCheck("After delay & tcs = null", 2);
var garbage = AllocateGarbage(1_000_000);
task = null;
GCCollectAndPulseCheck("After allocation & task = null", 0);
GCCollectAndPulseCheck("Gen 1", 1);
GCCollectAndPulseCheck("Gen 2", 2);

static List<object> AllocateGarbage(int garbageSize)
    => Enumerable.Range(0, garbageSize).Select(_ => new object()).ToList();

static async Task NoFinallyAsync(Task dependency)
{
    try {
        WriteLine("try");
        await dependency;
    }
    finally {
        WriteLine("finally");
    }
}

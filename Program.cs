using System;
using System.Reactive;
using System.Threading.Tasks;

var bytes = new byte[1];
var originalMemoryAddress = GetArrayAddress(ref bytes);
TaskCompletionSource<Unit>? tcs = new();
var wrTcs = new WeakReference(tcs);
var task = NoFinallyAsync(tcs!.Task);
var wrTask = new WeakReference(task);
Console.WriteLine(wrTask.Target != null ? "Task is alive" : "Task is collected");
Console.WriteLine(wrTcs.Target != null ? "TCS is alive" : "TCS is collected");

tcs = null;
task = null;
await Task.Delay(100);
AllocateGarbage(50_000_000);
GC.Collect();

var newMemoryAddress = GetArrayAddress(ref bytes);
Console.WriteLine("The byte array " +
    (originalMemoryAddress == newMemoryAddress ? "didn't move" : "moved"));

Console.WriteLine(wrTask.Target != null ? "Task is alive" : "Task is collected");
Console.WriteLine(wrTcs.Target != null ? "TCS is alive" : "TCS is collected");

static void AllocateGarbage(int garbageSize)
{
    for (var x = 0; x < garbageSize; x++)
        new object();
}

static unsafe UIntPtr GetArrayAddress(ref byte[] bytes)
{
    fixed (byte* pbytes = bytes)
        return (UIntPtr) pbytes;
}

static async Task NoFinallyAsync(Task dependency)
{
    try {
        Console.WriteLine("try");
        await dependency;
    }
    finally {
        Console.WriteLine("finally");
    }
}

using System;
using System.Threading;

namespace YqlossClientHarmony.Features.Replay;

public class Condition(Func<bool> predicate) : IDisposable
{
    private Func<bool> Predicate { get; } = predicate;

    private ThreadLocal<int> Counter { get; } = new();

    public void Dispose()
    {
        --Counter.Value;
    }

    public static implicit operator bool(Condition condition)
    {
        if (condition.Counter.Value == 0 && !condition.Predicate()) return false;
        ++condition.Counter.Value;
        return true;
    }
}
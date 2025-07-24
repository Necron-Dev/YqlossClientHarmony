using System;
using System.Threading;

namespace YqlossClientHarmony.Features.Replay;

public class ConcurrentCondition(Func<bool> predicate, Action? onTrue = null, Action? onFalse = null) : IDisposable
{
    private Func<bool> Predicate { get; } = predicate;

    private Action? OnTrue { get; } = onTrue;

    private Action? OnFalse { get; } = onFalse;

    private ThreadLocal<int> Counter { get; } = new();

    public void Dispose()
    {
        --Counter.Value;
    }

    public static implicit operator bool(ConcurrentCondition condition)
    {
        if (condition.Counter.Value == 0 && !condition.Predicate())
        {
            condition.OnFalse?.Invoke();
            return false;
        }

        ++condition.Counter.Value;
        condition.OnTrue?.Invoke();
        return true;
    }
}
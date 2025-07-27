using System;

namespace YqlossClientHarmony.Features.Replay;

public class Condition(Func<bool> predicate, Action? onTrue = null, Action? onFalse = null) : IDisposable
{
    private Func<bool> Predicate { get; } = predicate;

    private Action? OnTrue { get; } = onTrue;

    private Action? OnFalse { get; } = onFalse;

    private int Counter { get; set; }

    public void Dispose()
    {
        --Counter;
    }

    public static implicit operator bool(Condition condition)
    {
        if (condition.Counter == 0 && !condition.Predicate())
        {
            condition.OnFalse?.Invoke();
            return false;
        }

        condition.OnTrue?.Invoke();
        ++condition.Counter;
        return true;
    }
}
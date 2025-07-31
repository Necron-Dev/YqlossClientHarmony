using System;

namespace YqlossClientHarmony.Gui;

public class Trigger<TK, TV>
{
    public Trigger(Func<TK, TV>? initializer = null)
    {
        Initialized = false;
        Key = default;
        Value = default;
        Initializer = initializer;
    }

    public Trigger(TK key, TV value, Func<TK, TV>? initializer = null)
    {
        Initialized = true;
        Key = key;
        Value = value;
        Initializer = initializer;
    }

    private bool Initialized { get; set; }

    private TK? Key { get; set; }

    private TV? Value { get; set; }

    private Func<TK, TV>? Initializer { get; }

    public void Reset()
    {
        Initialized = false;
        Key = default;
        Value = default;
    }

    public TV Get(TK key, Func<TK, TV>? initializer = null)
    {
        var reinitialize = !Initialized || !Equals(key, Key);
        TV value;

        if (reinitialize)
        {
            initializer ??= Initializer;
            Initialized = false;
            Value = value = initializer!(key);
            Initialized = true;
            Key = key;
        }
        else
        {
            value = Value!;
        }

        return value;
    }
}
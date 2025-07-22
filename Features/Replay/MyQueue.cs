using System;
using System.Collections.Generic;

namespace YqlossClientHarmony.Features.Replay;

public class MyQueue<T>
{
    private int Pointer { get; set; }

    private List<IBox> List { get; } = [];

    public int Count => List.Count - Pointer;

    public void Enqueue(T item)
    {
        List.Add(new IBox.Data(item));
    }

    private static T Checked(IBox box)
    {
        return box switch
        {
            IBox.Data boxData => boxData.Value,
            _ => throw new NullReferenceException("accessing a discarded value")
        };
    }

    public T Peek()
    {
        var pointer = Pointer;
        if (pointer >= List.Count) throw new IndexOutOfRangeException();
        return Checked(List[pointer]);
    }

    public T Dequeue()
    {
        var pointer = Pointer;
        if (pointer >= List.Count) throw new IndexOutOfRangeException();
        var result = List[pointer];
        List[pointer] = new IBox.Empty();
        Pointer++;
        return Checked(result);
    }

    private interface IBox
    {
        public struct Empty : IBox;

        public struct Data(T value) : IBox
        {
            public T Value { get; } = value;
        }
    }
}
using System.Reflection;

namespace YqlossClientHarmony.Utilities;

public static class Reflections
{
    public static void CopyFields<T>(T dst, T src)
    {
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            field.SetValue(dst, field.GetValue(src));
    }
}
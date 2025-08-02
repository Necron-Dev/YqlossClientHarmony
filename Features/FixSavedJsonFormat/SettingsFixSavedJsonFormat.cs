using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.FixSavedJsonFormat;

[NoReorder]
public class SettingsFixSavedJsonFormat
{
    public static SettingsFixSavedJsonFormat Instance { get; } = new();

    public bool Enabled => Main.Enabled && Main.Settings.EnableFixSavedJsonFormat;
}
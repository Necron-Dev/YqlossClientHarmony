using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.FixSetInputEventCrash;

[NoReorder]
public class SettingsFixSetInputEventCrash
{
    public static SettingsFixSetInputEventCrash Instance { get; } = new();

    public bool Enabled => Main.Enabled && Main.Settings.EnableFixSetInputEventCrash;
}
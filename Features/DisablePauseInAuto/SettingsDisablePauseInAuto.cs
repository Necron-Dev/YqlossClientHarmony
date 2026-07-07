using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.DisablePauseInAuto;

[NoReorder]
public class SettingsDisablePauseInAuto
{
    public static SettingsDisablePauseInAuto Instance { get; } = new();

    public bool Enabled => Main.Enabled && Main.Settings.EnableDisablePauseInAuto;
}
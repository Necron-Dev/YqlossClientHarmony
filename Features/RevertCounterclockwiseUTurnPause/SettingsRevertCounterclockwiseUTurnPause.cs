using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.RevertCounterclockwiseUTurnPause;

[NoReorder]
public class SettingsRevertCounterclockwiseUTurnPause
{
    public static SettingsRevertCounterclockwiseUTurnPause Instance { get; } = new();

    public bool Enabled => Main.Enabled && Main.Settings.EnableRevertCounterclockwiseUTurnPause;
}
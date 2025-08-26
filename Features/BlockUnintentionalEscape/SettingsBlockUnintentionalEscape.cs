using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.BlockUnintentionalEscape;

[NoReorder]
public class SettingsBlockUnintentionalEscape
{
    public static SettingsBlockUnintentionalEscape Instance => Main.Settings.BlockUnintentionalEscapeSettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnableBlockUnintentionalEscape;

    public int EscapesRequired = 2;

    public double InSeconds = 0.5;

    public bool OnlyInGame = true;
}
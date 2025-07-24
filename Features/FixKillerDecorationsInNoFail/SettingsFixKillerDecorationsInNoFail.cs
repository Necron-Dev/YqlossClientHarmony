using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.FixKillerDecorationsInNoFail;

[NoReorder]
public class SettingsFixKillerDecorationsInNoFail
{
    public static SettingsFixKillerDecorationsInNoFail Instance { get; } = new();

    public bool Enabled => Main.Enabled && Main.Settings.EnableFixKillerDecorationsInNoFail;
}
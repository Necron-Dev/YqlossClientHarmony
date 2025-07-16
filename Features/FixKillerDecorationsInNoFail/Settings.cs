using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.FixKillerDecorationsInNoFail;

[NoReorder]
public class Settings
{
    public static Settings Instance { get; } = new();

    public bool Enabled => Main.Enabled && Main.Settings.EnableFixKillerDecorationsInNoFail;
}
using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.LimitRecolorTrack;

[NoReorder]
public class SettingsLimitRecolorTrack
{
    public static SettingsLimitRecolorTrack Instance => Main.Settings.LimitRecolorTrackSettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnableLimitRecolorTrack;

    public int AllowedBefore = 200;

    public int AllowedAfter = 500;
}
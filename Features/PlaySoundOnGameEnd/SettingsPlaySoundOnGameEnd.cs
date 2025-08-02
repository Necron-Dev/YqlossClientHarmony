using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.PlaySoundOnGameEnd;

[NoReorder]
public class SettingsPlaySoundOnGameEnd
{
    public static SettingsPlaySoundOnGameEnd Instance => Main.Settings.PlaySoundOnGameEndSettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnablePlaySoundOnGameEnd;

    public string OnWin = "";

    public string OnDeath = "";
}
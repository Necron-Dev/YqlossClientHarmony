using System;
using JetBrains.Annotations;
using UnityModManagerNet;
using YqlossClientHarmony.Features.BlockUnintentionalEscape;
using YqlossClientHarmony.Features.ModifyLoadingLevel;
using YqlossClientHarmony.Features.PlaySoundOnGameEnd;
using YqlossClientHarmony.Features.Replay;

namespace YqlossClientHarmony;

[NoReorder]
public class Settings : UnityModManager.ModSettings
{
    public string Language = I18N.DefaultLanguage;

    public bool EnableFixKillerDecorationsInNoFail = false;

    public bool EnableFixSetInputEventCrash = false;

    public bool EnableRevertCounterclockwiseUTurnPause = false;

    public bool EnableModifyLoadingLevel = false;

    public SettingsModifyLoadingLevel ModifyLoadingLevelSettings = new();

    public bool EnableReplay = false;

    public SettingsReplay ReplaySettings = new();

    public bool EnableFixSavedJsonFormat = false;

    public bool EnableBlockUnintentionalEscape = false;

    public SettingsBlockUnintentionalEscape BlockUnintentionalEscapeSettings = new();

    public bool EnablePlaySoundOnGameEnd = false;

    public SettingsPlaySoundOnGameEnd PlaySoundOnGameEndSettings = new();

    public void OnChange()
    {
        OnSettingChange?.Invoke();
    }

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public event Action? OnSettingChange;
}
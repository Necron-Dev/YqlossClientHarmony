using System;
using JetBrains.Annotations;
using UnityModManagerNet;
using YqlossClientHarmony.Features.ModifyLoadingLevel;
using YqlossClientHarmony.Features.Replay;

namespace YqlossClientHarmony;

[NoReorder]
public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Fix Killer Decorations Failing The Game In No Fail Mode")]
    public bool EnableFixKillerDecorationsInNoFail = false;

    [Draw("Fix Set Input Event Crashing Levels (Making Them Unplayable)")]
    public bool EnableFixSetInputEventCrash = false;

    [Draw("Revert Changes To Pause Events On Counterclockwise U-Turns In 2.9.4")]
    public bool EnableRevertCounterclockwiseUTurnPause = false;

    [Draw("Modify Loading Level")] public bool EnableModifyLoadingLevel = false;

    [Draw("Modify Loading Level Settings", Collapsible = true)]
    public SettingsModifyLoadingLevel ModifyLoadingLevelSettings = new();

    [Draw("Replay")] public bool EnableReplay = false;

    [Draw("Replay Settings", Collapsible = true)]
    public SettingsReplay ReplaySettings = new();

    public void OnChange()
    {
        OnSettingChange?.Invoke();
    }

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void DrawGUI(UnityModManager.ModEntry modEntry)
    {
        ReplayGUI.Draw(modEntry);
        this.Draw(modEntry);
    }

    public event Action? OnSettingChange;
}
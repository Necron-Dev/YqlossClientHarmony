using System;
using UnityModManagerNet;

namespace YqlossClientHarmony;

public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Fix Killer Decorations Failing The Game In No Fail Mode")]
    public bool EnableFixKillerDecorationsInNoFail = false;

    [Draw("Fix Set Input Event Crashing Levels (Making Them Unplayable)")]
    public bool EnableFixSetInputEventCrash = false;

    [Draw("Revert Changes To Pause Events On Counterclockwise U-Turns In 2.9.4")]
    public bool EnableRevertCounterclockwiseUTurnPause = false;

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
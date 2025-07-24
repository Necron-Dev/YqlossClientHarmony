using System;
using JetBrains.Annotations;
using UnityModManagerNet;
using YqlossClientHarmony.Features.ModifyLoadingLevel;
using YqlossClientHarmony.Features.Replay;

namespace YqlossClientHarmony;

[NoReorder]
public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Fix Killer Decorations Failing The Game In No Fail Mode 修复死亡装饰物在不死模式中导致玩家死亡")]
    public bool EnableFixKillerDecorationsInNoFail = false;

    [Draw("Fix Set Input Event Crashing Levels (Making Them Unplayable) 修复设置输入事件使谱面无法播放")]
    public bool EnableFixSetInputEventCrash = false;

    [Draw("Revert Changes To Pause Events On Counterclockwise U-Turns In 2.9.4 撤销 2.9.4 中对逆时针发卡弯上的暂停事件的修改")]
    public bool EnableRevertCounterclockwiseUTurnPause = false;

    [Draw("Modify Loading Level (Effect Remover) 修改加载关卡（去特效）")]
    public bool EnableModifyLoadingLevel = false;

    [Draw("Modify Loading Level Settings 修改加载关卡 设置", Collapsible = true)]
    public SettingsModifyLoadingLevel ModifyLoadingLevelSettings = new();

    [Draw("Replay 回放")] public bool EnableReplay = false;

    [Draw("Replay Settings 回放 设置", Collapsible = true)]
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
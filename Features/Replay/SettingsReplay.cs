using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace YqlossClientHarmony.Features.Replay;

[NoReorder]
public class SettingsReplay
{
    public static SettingsReplay Instance => Main.Settings.ReplaySettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnableReplay;

    [Header("Record replays in asynchronous input mode! 请在异步输入模式下录制回放！")]
    [Header("Currently only keyboard is supported. 目前只支持键盘输入。")]
    [Header("Currently DLC contents are partially supported. 目前只支持部分 DLC 功能。")]
    [Header("Use Normal hold tile mode when recording. 在录制时请使用“标准”长按方块行为。")]
    [Draw("Replay Storage Location 回放存储位置")]
    public string ReplayStorageLocation =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YCH ADOFAI Replays");

    [Header("The option below should be 0. 以下的选项值应该为 0。")]
    [Draw("Synchronous Input Recording Offset (ms) 同步输入模式录制偏移（毫秒）")]
    public double SyncRecordingOffset = 0.0;

    [Header("The option below should be 0. 以下的选项值应该为 0。")]
    [Draw("Asynchronous Input Recording Offset (ms) 异步输入模式录制偏移（毫秒）")]
    public double AsyncRecordingOffset = 0.0;

    [Draw("Store Synchronous Key Code In Asynchronous Input Mode 在使用异步输入模式时存储同步键码")]
    public bool StoreSyncKeyCode = false;

    [Draw("Only Record The Last Key Release When A Key Is Released Multiple Times 当按键被松开多次时只记录最后一次")]
    public bool OnlyStoreLastInMultiReleases = false;

    [Header("The option below should be 0. 以下的选项值应该为 0。")] [Draw("Playing Offset (ms) 播放偏移（毫秒）")]
    public double PlayingOffset = 0.0;

    [Draw("Trailing Length For Keys Held At The End Of Playing (ms) 播放结束时按住的按键拖尾长度（毫秒）")]
    public double TrailLength = -1.0;

    [Header("Debug Options 调试选项")] [Draw("Verbose")]
    public bool Verbose = false;
}
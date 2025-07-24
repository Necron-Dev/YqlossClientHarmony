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

    [Header("Recording replays in asynchronous input mode is recommended!")]
    [Header("Currently only keyboard is supported.")]
    [Header("Currently DLC contents are partially supported. Use Normal hold tile mode when recording.")]
    [Draw("Replay Storage Location")]
    public string ReplayStorageLocation =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YCH ADOFAI Replays");

    [Header("The option below should be 0.")] [Draw("Synchronous Input Recording Offset (ms)")]
    public double SyncRecordingOffset = 0.0;

    [Header("The option below should be 0.")] [Draw("Asynchronous Input Recording Offset (ms)")]
    public double AsyncRecordingOffset = 0.0;

    [Draw("Store Synchronous Key Code In Asynchronous Input Mode")]
    public bool StoreSyncKeyCode = false;

    [Header("The option below should be 0.")] [Draw("Playing Offset (ms)")]
    public double PlayingOffset = 0.0;

    [Header("Debug Options")] [Draw("Verbose")]
    public bool Verbose = false;
}
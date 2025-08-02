using System;
using System.IO;
using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.Replay;

[NoReorder]
public class SettingsReplay
{
    public static SettingsReplay Instance => Main.Settings.ReplaySettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnableReplay;

    public string ReplayStorageLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YCH ADOFAI Replays");

    public double SyncRecordingOffset = 0.0;

    public double AsyncRecordingOffset = 0.0;

    public bool StoreSyncKeyCode = false;

    public bool OnlyStoreLastInMultiReleases = true;

    public double PlayingOffset = 0.0;

    public double TrailLength = 100.0;

    public bool Verbose = false;
}
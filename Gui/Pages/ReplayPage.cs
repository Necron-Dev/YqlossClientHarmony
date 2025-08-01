using System;
using System.Collections.Generic;
using System.Linq;
using SFB;
using YqlossClientHarmony.Features.Replay;
using static YqlossClientHarmony.Gui.YCHLayout;
using static YqlossClientHarmony.Gui.YCHLayoutPreset;
using static YqlossClientHarmony.Gui.SettingUtil;

namespace YqlossClientHarmony.Gui.Pages;

public static class ReplayPage
{
    private static IReadOnlyList<string>? LastError { get; set; }

    private static string[] ErrorLoadReplay { get; } = ["Gui.Replay.Error.LoadReplay"];

    private static string[] ErrorRequireInLevel { get; } = ["Gui.Replay.Error.RequireInLevel"];

    private static string[] ErrorOfficialLevel { get; } = ["Gui.Replay.Error.OfficialLevel"];

    private static string[] ErrorLoadInGame { get; } = ["Gui.Replay.Error.LoadInGame"];

    private static string[] ErrorUnloadInGame { get; } = ["Gui.Replay.Error.UnloadInGame"];

    private static string[] ImportantTexts { get; } =
    [
        "Gui.Replay.Important.RecordInAsync",
        "Gui.Replay.Important.OnlySupportKeyboard",
        "Gui.Replay.Important.DLCCompatibility",
        "Gui.Replay.Important.KeyboardChatterBlockerCompatibility",
        "Gui.Replay.Important.OverlayerCompatibility"
    ];

    private static string LoadedReplayFileName { get; set; } = "";

    private static Trigger<ReplayInformationCacheKey, string[]> CachedReplayInformation { get; } = new();

    private static SizesGroup.Holder Group { get; } = new();

    private static void LoadReplay()
    {
        if (!Adofai.Controller.gameworld)
        {
            LastError = ErrorRequireInLevel;
            return;
        }

        if (ADOBase.isOfficialLevel)
        {
            LastError = ErrorOfficialLevel;
            return;
        }

        if (!Adofai.Controller.paused)
        {
            LastError = ErrorLoadInGame;
            return;
        }

        string[] levelPaths = StandaloneFileBrowser.OpenFilePanel(
            I18N.Translate("Dialog.Replay.SelectReplay.Title"),
            SettingsReplay.Instance.ReplayStorageLocation,
            [new ExtensionFilter(I18N.Translate("Dialog.Replay.SelectReplay.FileTypeName"), "ychreplay.gz")],
            false
        );

        if (levelPaths.Length == 0) return;

        var replayFileName = levelPaths[0];
        LoadedReplayFileName = replayFileName;

        LastError = ReplayPlayer.LoadReplay(replayFileName) ? null : ErrorLoadReplay;
    }

    private static void SelectReplayStorageLocation()
    {
        string[] levelPaths = StandaloneFileBrowser.OpenFolderPanel(
            I18N.Translate("Dialog.Replay.SelectReplayStorageLocation.Title"),
            SettingsReplay.Instance.ReplayStorageLocation,
            false
        );

        if (levelPaths.Length == 0) return;

        SettingsReplay.Instance.ReplayStorageLocation = levelPaths[0];
        Save |= true;
    }


    private static void UnloadReplay()
    {
        if (Adofai.Controller.gameworld && !ADOBase.isOfficialLevel && !Adofai.Controller.paused)
        {
            LastError = ErrorUnloadInGame;
            return;
        }

        ReplayPlayer.UnloadReplay();

        LastError = null;
    }

    private static void TryJumpToFloor(int floorId)
    {
        try
        {
            Adofai.Editor.SelectFloor(Adofai.Editor.floors[floorId]);
        }
        catch
        {
            // ignored
        }
    }

    private static string[] ReplayInformationKeys()
    {
        return
        [
            I18N.Translate("Gui.Replay.ReplayInformation.ReplayFile.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.LevelPath.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.Artist.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.Song.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.Author.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.XAccuracy.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.Progress.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.Judgements.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.Difficulty.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.AsyncInput.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.NoFail.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.HoldTileBehavior.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.LimitJudgements.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.KeyCount.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.KeyPressCounts.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.YchVersion.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.StartTime.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.EndTime.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.RecordingOffset.Name"),
            I18N.Translate("Gui.Replay.ReplayInformation.ModList.Name")
        ];
    }

    private static (int, List<int>) GetKeyCountInfo(Replay replay)
    {
        Dictionary<int, int> keyCount = [];

        foreach (var keyEvent in replay.KeyEvents.Where(keyEvent => !keyEvent.IsKeyUp))
            keyCount[keyEvent.KeyCode] = keyCount.GetValueOrDefault(keyEvent.KeyCode, 0) + 1;

        var values = keyCount.Values.ToList();
        values.Sort((x, y) => y.CompareTo(x));
        return (keyCount.Count, values);
    }

    private static string[] ReplayInformationValues(string replayFileName, Replay replay)
    {
        var filteredArtist = ReplayUtils.FilterInvalidCharacters(replay.Metadata.Artist).Trim();
        var filteredSong = ReplayUtils.FilterInvalidCharacters(replay.Metadata.Song).Trim();
        var filteredAuthor = ReplayUtils.FilterInvalidCharacters(replay.Metadata.Author).Trim();
        var xAccuracy = ReplayUtils.GetXAccuracy(replay) * 100;
        var startFloor = replay.Metadata.StartingFloorId;
        var endFloor = ReplayUtils.GetEndingFloorId(replay) + 1;
        var startProgress = startFloor * 100 / replay.Metadata.TotalFloorCount;
        if (replay.Metadata.StartingFloorId != 0 && startProgress == 0) startProgress = 1;
        var endProgress = endFloor * 100 / replay.Metadata.TotalFloorCount;
        var te = ReplayUtils.GetHitMarginCount(replay, HitMargin.TooEarly);
        var e = ReplayUtils.GetHitMarginCount(replay, HitMargin.VeryEarly);
        var ep = ReplayUtils.GetHitMarginCount(replay, HitMargin.EarlyPerfect);
        var pp = ReplayUtils.GetHitMarginCount(replay, HitMargin.Perfect);
        var lp = ReplayUtils.GetHitMarginCount(replay, HitMargin.LatePerfect);
        var l = ReplayUtils.GetHitMarginCount(replay, HitMargin.VeryLate);
        var tl = ReplayUtils.GetHitMarginCount(replay, HitMargin.TooLate);
        var miss = ReplayUtils.GetHitMarginCount(replay, HitMargin.FailMiss);
        var overload = ReplayUtils.GetHitMarginCount(replay, HitMargin.FailOverload);
        var auto = ReplayUtils.GetHitMarginCount(replay, HitMargin.Auto);
        var difficulty = replay.Metadata.Difficulty switch
        {
            Difficulty.Lenient => "Lenient",
            Difficulty.Normal => "Normal",
            Difficulty.Strict => "Strict",
            _ => $"{replay.Metadata.Difficulty}"
        };
        var asyncInput = replay.Metadata.UseAsyncInput ? "True" : "False";
        var noFail = replay.Metadata.NoFailMode ? "True" : "False";
        var holdBehavior = replay.Metadata.HoldBehavior switch
        {
            HoldBehavior.Normal => "Normal",
            HoldBehavior.CanHitEnd => "CanHitEnd",
            HoldBehavior.NoHoldNeeded => "NoHoldNeeded",
            _ => $"{replay.Metadata.HoldBehavior}"
        };
        var hitMarginLimit = replay.Metadata.HitMarginLimit switch
        {
            HitMarginLimit.None => "None",
            HitMarginLimit.PerfectsOnly => "PerfectsOnly",
            HitMarginLimit.PurePerfectOnly => "PurePerfectOnly",
            _ => $"{replay.Metadata.HitMarginLimit}"
        };
        var levelPathKey = replay.Metadata.LevelPath is null ? "Unknown" : "Value";
        var ychVersionKey = replay.Metadata.YchVersion is null ? "Unknown" : "Value";
        var startTimeKey = replay.Metadata.StartTime is null ? "Unknown" : "Value";
        var endTimeKey = replay.EndTime is null ? "Unknown" : "Value";
        var recordingOffsetKey = replay.Metadata.RecordingOffset is null ? "Unknown" : "Value";
        var modListKey = replay.Metadata.ModList is null ? "Unknown" : "Value";
        var (uniqueKeys, keyCounts) = GetKeyCountInfo(replay);
        return
        [
            I18N.Translate("Gui.Replay.ReplayInformation.ReplayFile.Value", replayFileName),
            I18N.Translate($"Gui.Replay.ReplayInformation.LevelPath.{levelPathKey}", replay.Metadata.LevelPath),
            I18N.Translate("Gui.Replay.ReplayInformation.Artist.Value", filteredArtist),
            I18N.Translate("Gui.Replay.ReplayInformation.Song.Value", filteredSong),
            I18N.Translate("Gui.Replay.ReplayInformation.Author.Value", filteredAuthor),
            I18N.Translate("Gui.Replay.ReplayInformation.XAccuracy.Value", xAccuracy),
            I18N.Translate("Gui.Replay.ReplayInformation.Progress.Value", startProgress, startFloor, endProgress, endFloor, replay.Metadata.TotalFloorCount),
            I18N.Translate("Gui.Replay.ReplayInformation.Judgements.Value", overload, te, e, ep, pp, auto, lp, l, tl, miss),
            I18N.Translate($"Gui.Replay.ReplayInformation.Difficulty.{difficulty}"),
            I18N.Translate($"Gui.Replay.ReplayInformation.AsyncInput.{asyncInput}"),
            I18N.Translate($"Gui.Replay.ReplayInformation.NoFail.{noFail}"),
            I18N.Translate($"Gui.Replay.ReplayInformation.HoldTileBehavior.{holdBehavior}"),
            I18N.Translate($"Gui.Replay.ReplayInformation.LimitJudgements.{hitMarginLimit}"),
            I18N.Translate("Gui.Replay.ReplayInformation.KeyCount.Value", uniqueKeys),
            I18N.Translate("Gui.Replay.ReplayInformation.KeyPressCounts.Value", string.Join(", ", keyCounts)),
            I18N.Translate($"Gui.Replay.ReplayInformation.YchVersion.{ychVersionKey}", replay.Metadata.YchVersion),
            I18N.Translate($"Gui.Replay.ReplayInformation.StartTime.{startTimeKey}", replay.Metadata.StartTime?.ToLocalTime()),
            I18N.Translate($"Gui.Replay.ReplayInformation.EndTime.{endTimeKey}", replay.EndTime?.ToLocalTime()),
            I18N.Translate($"Gui.Replay.ReplayInformation.RecordingOffset.{recordingOffsetKey}", replay.Metadata.RecordingOffset),
            I18N.Translate($"Gui.Replay.ReplayInformation.ModList.{modListKey}", replay.Metadata.ModList)
        ];
    }

    public static void Draw()
    {
        var settings = SettingsReplay.Instance;
        var group = Group.Begin();

        Begin(ContainerDirection.Vertical);
        {
            Text(I18N.Translate("Page.Replay.Name"), TextStyle.Title);
            Separator();
            SwitchOption(group, ref Main.Settings.EnableReplay, "Setting.Replay.Enabled", true);
            Separator();
            AddMargin(8);

            Begin(ContainerDirection.Horizontal, options: WidthMax);
            {
                if (Button(I18N.Translate("Gui.Replay.LoadReplay"), options: WidthMax))
                    LoadReplay();

                if (ReplayPlayer.Replay is not null)
                {
                    if (Button(I18N.Translate("Gui.Replay.UnloadReplay"), options: WidthMax))
                        UnloadReplay();
                    if (Button(I18N.Translate("Gui.Replay.JumpToStartingFloor"), options: WidthMax))
                        TryJumpToFloor(ReplayPlayer.Replay.Metadata.StartingFloorId);
                    if (Button(I18N.Translate("Gui.Replay.JumpToEndingFloor"), options: WidthMax))
                        TryJumpToFloor(ReplayUtils.GetEndingFloorId(ReplayPlayer.Replay) + 1);
                }
                else
                {
                    Fill();
                }
            }
            End();

            AddMargin(8);

            var errorGroup = group.Group;
            if (LastError is not null)
            {
                var clearError = false;
                foreach (var line in LastError) clearError |= IconText(errorGroup, IconStyle.Error, line);
                if (clearError) LastError = null;
            }

            var replay = ReplayPlayer.Replay;
            if (replay is not null)
            {
                Text(I18N.Translate("Gui.Replay.ReplayInformation"), TextStyle.Subtitle);

                var keys = ReplayInformationKeys();
                var values = CachedReplayInformation.Get(
                    new ReplayInformationCacheKey(I18N.SelectedLanguage.Code, replay),
                    _ => ReplayInformationValues(LoadedReplayFileName, replay)
                );

                Begin(ContainerDirection.Vertical, ContainerStyle.Background, options: WidthMax);
                {
                    for (var i = 0; i < keys.Length; i++)
                    {
                        Begin(ContainerDirection.Horizontal, options: WidthMax);
                        {
                            Text(I18N.Translate(keys[i]), options: Width(120));
                            Text(I18N.Translate(values[i]), options: WidthMax);
                        }
                        End();
                    }
                }
                End();
            }

            Text(I18N.Translate("Gui.Replay.Important"), TextStyle.Subtitle);

            var importantGroup = group.Group;
            Begin(ContainerDirection.Vertical, ContainerStyle.Background, options: WidthMax);
            {
                var first = true;
                foreach (var important in ImportantTexts)
                {
                    if (first) first = false;
                    else Separator();
                    IconText(importantGroup, IconStyle.Warning, important);
                }
            }
            End();

            Text(I18N.Translate("Gui.Replay.Settings"), TextStyle.Subtitle);

            var settingsGroup = group.Group;
            Begin(ContainerDirection.Vertical, ContainerStyle.Background, options: WidthMax);
            {
                Begin(ContainerDirection.Horizontal, sizes: settingsGroup, options: [WidthMax]);
                PushAlign(0.5);
                {
                    Text(I18N.Translate("Setting.Replay.ReplayStorageLocation"), options: WidthMin);
                    Fill();
                    Save |= TextField(ref settings.ReplayStorageLocation, options: WidthMin);
                    if (Button(I18N.Translate("Setting.Replay.ReplayStorageLocation.Select"), options: WidthMin))
                        SelectReplayStorageLocation();
                }
                PopAlign();
                End();

                Separator();
                SwitchOption(settingsGroup, ref settings.StoreSyncKeyCode, "Setting.Replay.StoreSyncKeyCode");
                Separator();
                SwitchOption(settingsGroup, ref settings.OnlyStoreLastInMultiReleases, "Setting.Replay.OnlyStoreLastInMultiReleases", true);
                Separator();
                DoubleOption(settingsGroup, ref settings.TrailLength, "Setting.Replay.TrailLength", description: true);
                Separator();
                SwitchOption(settingsGroup, ref settings.NoTickToDspCache, "Setting.Replay.NoTickToDspCache", true);
                Separator();
                SwitchOption(settingsGroup, ref settings.DecoderSortKeyEvents, "Setting.Replay.DecoderSortKeyEvents", true);
                Separator();
                IconText(settingsGroup, IconStyle.Warning, "Gui.Replay.Settings.OffsetZeroWarning");
                Separator();
                DoubleOption(settingsGroup, ref settings.SyncRecordingOffset, "Setting.Replay.SyncRecordingOffset");
                Separator();
                DoubleOption(settingsGroup, ref settings.AsyncRecordingOffset, "Setting.Replay.AsyncRecordingOffset");
                Separator();
                DoubleOption(settingsGroup, ref settings.PlayingOffset, "Setting.Replay.PlayingOffset");
            }
            End();

            Text(I18N.Translate("Gui.Replay.DebugOptions"), TextStyle.Subtitle);

            var debugGroup = group.Group;
            Begin(ContainerDirection.Vertical, ContainerStyle.Background, options: WidthMax);
            {
                IconText(debugGroup, IconStyle.Warning, "Gui.Replay.DebugOptions.Warning");
                Separator();
                SwitchOption(debugGroup, ref settings.Verbose, "Setting.Replay.Verbose");
            }
            End();
        }
        End();
    }

    public class ReplayInformationCacheKey(string language, Replay replay)
    {
        private string Language { get; } = language;

        private Replay Replay { get; } = replay;

        public override bool Equals(object? obj)
        {
            if (obj is not ReplayInformationCacheKey key) return false;
            return key.Language == Language && ReferenceEquals(key.Replay, Replay);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Language, Replay);
        }
    }
}
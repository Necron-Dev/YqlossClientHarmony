using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayUtils
{
    private static readonly Regex RegexStyle = new("<.*?>");

    private static readonly List<char> InvalidCharacters = Path.GetInvalidFileNameChars().ToList();

    public static string FilterInvalidCharacters(string path)
    {
        path = RegexStyle.Replace(path, "");
        var builder = new StringBuilder();
        foreach (var c in path)
            if (!InvalidCharacters.Contains(c))
                builder.Append(c);
        return builder.ToString().Trim();
    }

    public static int GetEndingFloorId(Replay replay)
    {
        var floorId = replay.Metadata.StartingFloorId;

        foreach (var judgement in replay.Judgements) floorId += judgement.FloorIdIncrement;

        return floorId;
    }

    public static double GetXAccuracy(Replay replay)
    {
        if (replay.Judgements.Count == 0) return 0;

        var xAccuracy = 0.0;

        foreach (var judgement in replay.Judgements)
            xAccuracy += judgement.HitMargin switch
            {
                HitMargin.TooEarly => 0.2,
                HitMargin.VeryEarly => 0.4,
                HitMargin.EarlyPerfect => 0.75,
                HitMargin.Perfect => 1.0,
                HitMargin.LatePerfect => 0.75,
                HitMargin.VeryLate => 0.4,
                HitMargin.TooLate => 0.2,
                HitMargin.Auto => 1.0,
                _ => 0.0
            };

        return xAccuracy / replay.Judgements.Count;
    }

    public static int GetHitMarginCount(Replay replay, HitMargin hitMargin)
    {
        var count = 0;

        foreach (var judgement in replay.Judgements)
            if (judgement.HitMargin == hitMargin)
                ++count;

        return count;
    }

    public static string ReplayFileName(Replay replay)
    {
        var time = DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss.fff");
        // var filteredArtist = FilterInvalidCharacters(replay.Metadata.Artist).Trim();
        // var filteredSong = FilterInvalidCharacters(replay.Metadata.Song).Trim();
        // var filteredAuthor = FilterInvalidCharacters(replay.Metadata.Author).Trim();
        // var folderName = $"{filteredArtist} - {filteredSong} - {filteredAuthor}".Trim();
        var xAccuracy = GetXAccuracy(replay) * 100;
        var startingProgress = replay.Metadata.StartingFloorId * 100 / replay.Metadata.TotalFloorCount;
        if (replay.Metadata.StartingFloorId != 0 && startingProgress == 0) startingProgress = 1;
        var endingProgress = (GetEndingFloorId(replay) + 1) * 100 / replay.Metadata.TotalFloorCount;
        var fileName = $"{time}-{xAccuracy:0.00}-{startingProgress}-{endingProgress}.ychreplay.gz";
        // return Path.Combine(Settings.Instance.ReplayStorageLocation, folderName, fileName);
        return Path.Combine(SettingsReplay.Instance.ReplayStorageLocation, fileName);
    }

    public static string[] ReplayMetadataString(Replay replay)
    {
        var filteredArtist = FilterInvalidCharacters(replay.Metadata.Artist).Trim();
        var filteredSong = FilterInvalidCharacters(replay.Metadata.Song).Trim();
        var filteredAuthor = FilterInvalidCharacters(replay.Metadata.Author).Trim();
        var xAccuracy = GetXAccuracy(replay) * 100;
        var startFloor = replay.Metadata.StartingFloorId;
        var endFloor = GetEndingFloorId(replay) + 1;
        var startProgress = startFloor * 100 / replay.Metadata.TotalFloorCount;
        if (replay.Metadata.StartingFloorId != 0 && startProgress == 0) startProgress = 1;
        var endProgress = endFloor * 100 / replay.Metadata.TotalFloorCount;
        var te = GetHitMarginCount(replay, HitMargin.TooEarly);
        var e = GetHitMarginCount(replay, HitMargin.VeryEarly);
        var ep = GetHitMarginCount(replay, HitMargin.EarlyPerfect);
        var pp = GetHitMarginCount(replay, HitMargin.Perfect);
        var lp = GetHitMarginCount(replay, HitMargin.LatePerfect);
        var l = GetHitMarginCount(replay, HitMargin.VeryLate);
        var tl = GetHitMarginCount(replay, HitMargin.TooLate);
        var miss = GetHitMarginCount(replay, HitMargin.FailMiss);
        var overload = GetHitMarginCount(replay, HitMargin.FailOverload);
        var auto = GetHitMarginCount(replay, HitMargin.Auto);
        var ppText = auto != 0 ? $"{pp} ({auto})" : $"{pp}";
        var difficulty = replay.Metadata.Difficulty switch
        {
            Difficulty.Lenient => "Lenient",
            Difficulty.Normal => "Normal",
            Difficulty.Strict => "Strict",
            _ => $"{replay.Metadata.Difficulty}"
        };
        var noFail = replay.Metadata.NoFailMode ? "On" : "Off";
        var asyncInput = replay.Metadata.UseAsyncInput ? "On" : "Off";
        var holdBehavior = replay.Metadata.HoldBehavior switch
        {
            HoldBehavior.Normal => "Normal",
            HoldBehavior.CanHitEnd => "Allow Tapping End Tile",
            HoldBehavior.NoHoldNeeded => "No Holding Required",
            _ => $"{replay.Metadata.HoldBehavior}"
        };
        var hitMarginLimit = replay.Metadata.HitMarginLimit switch
        {
            HitMarginLimit.None => "None",
            HitMarginLimit.PerfectsOnly => "Perfects Only",
            HitMarginLimit.PurePerfectOnly => "Pure Perfects Only",
            _ => $"{replay.Metadata.HitMarginLimit}"
        };
        return
        [
            $"Artist: {filteredArtist} | " +
            $"Song: {filteredSong} | " +
            $"Author: {filteredAuthor}",
            $"X-Accuracy: {xAccuracy:0.00}% | " +
            $"Progress: {startProgress}% ({startFloor}) - {endProgress}% ({endFloor}) | " +
            $"Judgements: {overload} {te} [{e} {ep} [{ppText}] {lp} {l}] {tl} {miss}",
            $"Difficulty: {difficulty} | " +
            $"No Fail: {noFail} | " +
            $"Async Input: {asyncInput} | " +
            $"Hold Tile Behavior: {holdBehavior} | " +
            $"Limit Judgements: {hitMarginLimit}"
        ];
    }
}
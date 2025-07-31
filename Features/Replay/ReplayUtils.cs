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
}
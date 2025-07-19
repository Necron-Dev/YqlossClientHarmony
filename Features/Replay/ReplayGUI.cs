using System.Collections.Generic;
using SFB;
using UnityEngine;
using UnityModManagerNet;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayGUI
{
    private static int LastLoadingFailure { get; set; }
    private static string LoadedReplayFileName { get; set; } = "";

    private static string[] GetTexts()
    {
        var replayToPlay = ReplayPlayer.Replay;

        if (replayToPlay is null)
            return LastLoadingFailure switch
            {
                0 => ["There is no replay loaded."],
                1 => [$"Failed to load replay {LoadedReplayFileName}", "See logs for more information."],
                2 => ["You must load in a level to load a replay!"],
                3 => ["Official levels are not supported!"],
                4 => ["You cannot load a replay mid-game!"],
                5 => ["You cannot unload a replay mid-game!"],
                6 => ["You must be using synchronous input mode to play replays!"],
                _ => [$"Unknown Error {LastLoadingFailure}", "See logs for more information."]
            };

        List<string> texts = [$"Replay File: {LoadedReplayFileName}"];

        texts.AddRange(ReplayUtils.ReplayMetadataString(replayToPlay));

        return texts.ToArray();
    }

    private static void LoadReplay()
    {
        if (!Adofai.Controller.gameworld)
        {
            LastLoadingFailure = 2;
            return;
        }

        if (ADOBase.isOfficialLevel)
        {
            LastLoadingFailure = 3;
            return;
        }

        if (!Adofai.Controller.paused)
        {
            LastLoadingFailure = 4;
            return;
        }

        if (Persistence.GetChosenAsynchronousInput())
        {
            LastLoadingFailure = 6;
            return;
        }

        string[] levelPaths = StandaloneFileBrowser.OpenFilePanel(
            "Load Replay",
            Settings.Instance.ReplayStorageLocation,
            [new ExtensionFilter("Compressed YCH Replay", "ychreplay.gz")],
            false
        );

        if (levelPaths.Length == 0) return;

        var replayFileName = levelPaths[0];
        LoadedReplayFileName = replayFileName;

        LastLoadingFailure = ReplayPlayer.LoadReplay(replayFileName) ? 0 : 1;
    }

    private static void UnloadReplay()
    {
        if (Adofai.Controller.gameworld && !ADOBase.isOfficialLevel && !Adofai.Controller.paused)
        {
            LastLoadingFailure = 5;
            return;
        }

        ReplayPlayer.UnloadReplay();
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

    public static void Draw(UnityModManager.ModEntry modEntry)
    {
        foreach (var text in GetTexts()) GUILayout.Label(text);

        if (ReplayPlayer.Replay is not null)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Jump To Start");

                if (GUILayout.Button("Jump"))
                    TryJumpToFloor(ReplayPlayer.Replay.Metadata.StartingFloorId);

                GUILayout.Label("Jump To End");

                if (GUILayout.Button("Jump"))
                    TryJumpToFloor(ReplayUtils.GetEndingFloorId(ReplayPlayer.Replay) + 1);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Load Replay File");

            if (GUILayout.Button("Select")) LoadReplay();

            GUILayout.Label("Unload Replay");

            if (GUILayout.Button("Unload")) UnloadReplay();
        }
        GUILayout.EndHorizontal();
    }
}
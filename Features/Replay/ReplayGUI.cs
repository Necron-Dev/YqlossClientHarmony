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
                0 => ["There is no replay loaded. 未加载回放。"],
                1 =>
                [
                    $"Failed to load replay 加载回放失败 {LoadedReplayFileName}",
                    "See logs for more information. 详细信息请查看日志。"
                ],
                2 => ["You must load in a level to load a replay! 必须加载关卡才能加载回放！"],
                3 => ["Official levels are not supported! 不支持官方关卡！"],
                4 => ["You cannot load a replay mid-game! 无法在游戏中加载回放！"],
                5 => ["You cannot unload a replay mid-game! 无法在游戏中卸载回放！"],
                6 => ["Ciallo～(∠・ω< )⌒★ 那我问你为什么会出现这个"],
                _ =>
                [
                    $"Unknown Error 未知错误 {LastLoadingFailure}",
                    "See logs for more information. 详细信息请查看日志。"
                ]
            };

        List<string> texts = [$"Replay File 回放文件: {LoadedReplayFileName}"];

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

        string[] levelPaths = StandaloneFileBrowser.OpenFilePanel(
            "Load Replay",
            SettingsReplay.Instance.ReplayStorageLocation,
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
                GUILayout.Label("Jump To Start 跳转到起点");

                if (GUILayout.Button("Jump 跳转"))
                    TryJumpToFloor(ReplayPlayer.Replay.Metadata.StartingFloorId);

                GUILayout.Label("Jump To End 跳转到终点");

                if (GUILayout.Button("Jump 跳转"))
                    TryJumpToFloor(ReplayUtils.GetEndingFloorId(ReplayPlayer.Replay) + 1);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Load Replay File 加载回放文件");

            if (GUILayout.Button("Select 选择")) LoadReplay();

            GUILayout.Label("Unload Replay 卸载回放文件");

            if (GUILayout.Button("Unload 卸载")) UnloadReplay();
        }
        GUILayout.EndHorizontal();
    }
}
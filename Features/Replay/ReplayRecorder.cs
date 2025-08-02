using System;
using System.IO;
using System.Text;
using UnityModManagerNet;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayRecorder
{
    private const int KeyCodeEsc = 27;
    private const int KeyCodeEscAsync = 4123;

    public static Replay? Replay { get; set; }

    private static double? ErrorMeterValue { get; set; }

    private static int? LastFloorIdJudgement { get; set; }

    private static int? LastFloorIdKeyEvent { get; set; }

    private static int KeysWithoutAngleCorrection { get; set; }

    private static int CurrentIterationFirstKeyIndex { get; set; }

    public static void SaveAndResetReplay()
    {
        var replay = Replay;
        if (replay is null) return;

        if (replay.KeyEvents.Count == 0 && replay.Judgements.Count == 0)
        {
            Main.Mod.Logger.Log("discarding replay because it's empty");
        }
        else
        {
            FillAngleCorrections();
            replay.EndTime = DateTimeOffset.Now;
            var fileName = ReplayUtils.ReplayFileName(replay);
            Main.Mod.Logger.Log($"saving replay as {fileName}");
            Directory.CreateDirectory(SettingsReplay.Instance.ReplayStorageLocation);
            ReplayEncoder.LaunchCompressAndSaveAs(replay, fileName);
        }

        Replay = null;
    }

    private static string? GetModList()
    {
        try
        {
            var builder = new StringBuilder();

            foreach (var modEntry in UnityModManager.modEntries)
            {
                builder.Append("[");
                builder.Append(modEntry.Active ? "O" : "X");
                builder.Append("] ");
                builder.Append(modEntry.Info.Id);
                builder.Append(" v");
                builder.Append(modEntry.Info.Version);
                builder.Append(", ");
            }

            builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }
        catch (Exception exception)
        {
            Main.Mod.Logger.Warning($"failed to fetch mod list: {exception}");
            return null;
        }
    }

    private static void NewReplay(int floorId)
    {
        Replay = new Replay(new Replay.MetadataType(
            floorId,
            Adofai.TotalFloorCount,
            Adofai.Game.levelData.artist,
            Adofai.Game.levelData.song,
            Adofai.Game.levelData.author,
            GCS.difficulty,
            Adofai.Controller.noFail,
            Persistence.GetChosenAsynchronousInput(),
            Persistence.holdBehavior,
            Persistence.hitMarginLimit,
            DateTimeOffset.Now,
            Persistence.GetChosenAsynchronousInput() ? SettingsReplay.Instance.AsyncRecordingOffset : SettingsReplay.Instance.SyncRecordingOffset,
            string.IsNullOrEmpty(ADOBase.levelPath) ? null : ADOBase.levelPath,
            Main.Mod.Info.Version,
            GetModList(),
            (double)scrConductor.calibration_i * 1000,
            Persistence.audioBufferSize
        ));

        ErrorMeterValue = null;
        LastFloorIdJudgement = floorId;
        LastFloorIdKeyEvent = floorId;
        KeysWithoutAngleCorrection = 0;

        Main.Mod.Logger.Log("starting to record replay");
    }

    public static void StartRecording(int floorId)
    {
        SaveAndResetReplay();
        if (ReplayPlayer.Replay is not null) return;
        NewReplay(floorId);
    }

    public static void EndRecording()
    {
        SaveAndResetReplay();
    }

    public static void OnHitMargin(HitMargin hitMargin)
    {
        var replay = Replay;
        if (replay is null) return;

        var floorId = Adofai.CurrentFloorId;

        if (Adofai.Controller.midspinInfiniteMargin)
        {
            if (ErrorMeterValue is not null)
                Main.Mod.Logger.Warning($"[Floor {floorId}] error meter already has a value on a midspin, overwriting to NaN");

            ErrorMeterValue = double.NaN;
        }

        var errorMeterNullable = ErrorMeterValue;
        double errorMeter;
        ErrorMeterValue = null;

        if (errorMeterNullable is null)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] error meter is null, recording as NaN");
            errorMeter = double.NaN;
        }
        else
        {
            errorMeter = errorMeterNullable.Value;
        }

        var lastFloorIdNullable = LastFloorIdJudgement;
        LastFloorIdJudgement = floorId;
        int lastFloorId;

        if (lastFloorIdNullable is null)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] judgement last floor id is null, defaulting to {floorId}");
            lastFloorId = floorId;
        }
        else
        {
            lastFloorId = lastFloorIdNullable.Value;
        }

        replay.Judgements.Add(new Replay.JudgementType(
            errorMeter,
            hitMargin,
            floorId - lastFloorId
        ));

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"meter: {errorMeter} margin: {hitMargin} dseq: {floorId - lastFloorId}");
    }

    public static void OnErrorMeter(double errorMeter)
    {
        var replay = Replay;
        if (replay is null) return;

        var floorId = Adofai.CurrentFloorId;
        if (ErrorMeterValue is not null)
            Main.Mod.Logger.Warning($"[Floor {floorId}] error meter already has a value, overwriting");
        ErrorMeterValue = errorMeter;
    }

    public static void OnKeyEvent(int keyCode, bool isKeyUp, double songSeconds)
    {
        var replay = Replay;
        if (replay is null) return;

        FillAngleCorrections();

        if (keyCode is KeyCodeEsc or KeyCodeEscAsync) return;

        var floorId = Adofai.CurrentFloorId;

        var lastFloorIdNullable = LastFloorIdKeyEvent;
        LastFloorIdKeyEvent = floorId;
        int lastFloorId;

        if (lastFloorIdNullable is null)
        {
            Main.Mod.Logger.Warning($"[Floor {floorId}] key event last floor id is null, defaulting to {floorId}");
            lastFloorId = floorId;
        }
        else
        {
            lastFloorId = lastFloorIdNullable.Value;
        }

        if (SettingsReplay.Instance.StoreSyncKeyCode)
        {
            keyCode = KeyCodeMapping.GetSyncKeyCode(keyCode);

            if (keyCode >= 0x1000)
                Main.Mod.Logger.Warning($"[Floor {floorId}] unidentified async key code {keyCode}");
        }

        var nextFloor = Adofai.Controller.currFloor.nextfloor;
        var autoFloor = nextFloor != null && nextFloor.auto;
        var inputLocked = !Adofai.Controller.responsive;

        replay.KeyEvents.Add(new Replay.KeyEventType(
            songSeconds,
            keyCode,
            isKeyUp,
            floorId - lastFloorId,
            autoFloor,
            inputLocked
        ));

        ++KeysWithoutAngleCorrection;

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"key: {keyCode} up: {isKeyUp} dseq: {floorId - lastFloorId} pos: {songSeconds} auto: {autoFloor} locked: {inputLocked}");
    }

    public static void OnAngleCorrection(double angle)
    {
        var replay = Replay;
        if (replay is null) return;

        if (KeysWithoutAngleCorrection <= 0) return;
        --KeysWithoutAngleCorrection;
        replay.AngleCorrections.Add(angle);

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"angle: {angle}");
    }

    private static void FillAngleCorrections()
    {
        var replay = Replay;
        if (replay is null) return;

        var angleCorrections = replay.AngleCorrections;
        var last = angleCorrections.Count == 0 ? double.NaN : angleCorrections[^1];

        while (KeysWithoutAngleCorrection > 0)
        {
            --KeysWithoutAngleCorrection;
            angleCorrections.Add(last);
        }
    }

    public static void OnIterationStart(int stacked)
    {
        var replay = Replay;
        if (replay is null) return;

        CurrentIterationFirstKeyIndex = Math.Max(0, replay.KeyEvents.Count - stacked);
    }

    public static void OnKeysProcessed(int count)
    {
        var replay = Replay;
        if (replay is null) return;

        CurrentIterationFirstKeyIndex = Math.Min(CurrentIterationFirstKeyIndex + count, replay.KeyEvents.Count);

        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"iteration advance: +{count} {CurrentIterationFirstKeyIndex} total: {replay.KeyEvents.Count}");
    }

    public static void OnMarkKeyEvent(bool auto, bool responsive)
    {
        var replay = Replay;
        if (replay is null) return;

        var i = CurrentIterationFirstKeyIndex;
        if (i >= replay.KeyEvents.Count) return;
        replay.KeyEvents[i] = MarkKeyEvent(replay.KeyEvents[i], auto, !responsive);
        if (SettingsReplay.Instance.Verbose)
            Main.Mod.Logger.Log($"mark input locked: index: {i} {auto} {!responsive}");
    }

    private static Replay.KeyEventType MarkKeyEvent(Replay.KeyEventType keyEvent, bool auto, bool responsive)
    {
        return new Replay.KeyEventType(
            keyEvent.SongSeconds,
            keyEvent.KeyCode,
            keyEvent.IsKeyUp,
            keyEvent.FloorIdIncrement,
            auto,
            responsive,
            keyEvent.Version1
        );
    }
}
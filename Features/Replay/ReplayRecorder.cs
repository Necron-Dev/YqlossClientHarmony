using System;
using System.IO;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayRecorder
{
    private const int KeyCodeEsc = 27;
    private const int KeyCodeEscAsync = 4123;

    private static object RecorderLock { get; } = new();

    public static Replay? Replay { get; set; }

    private static double? ErrorMeterValue { get; set; }

    private static int? LastFloorIdJudgement { get; set; }

    private static int? LastFloorIdKeyEvent { get; set; }

    public static int KeysWithoutAngleCorrection { get; set; }

    public static int LastIterationFirstKeyIndex { get; set; }

    public static int CurrentIterationFirstKeyIndex { get; set; }

    public static void SaveAndResetReplay()
    {
        WithReplay(replay =>
        {
            if (replay.KeyEvents.Count == 0 && replay.Judgements.Count == 0)
            {
                Main.Mod.Logger.Log("discarding replay because it's empty");
            }
            else
            {
                var fileName = ReplayUtils.ReplayFileName(replay);
                Main.Mod.Logger.Log($"saving replay as {fileName}");
                Directory.CreateDirectory(Settings.Instance.ReplayStorageLocation);
                ReplayEncoder.LaunchCompressAndSaveAs(replay, fileName);
            }
        });
        Replay = null;
    }

    private static void NewReplay(int floorId)
    {
        lock (RecorderLock)
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
                Persistence.hitMarginLimit
            ));

            ErrorMeterValue = null;
            LastFloorIdJudgement = floorId;
            LastFloorIdKeyEvent = floorId;
            KeysWithoutAngleCorrection = 0;

            Main.Mod.Logger.Log("starting to record replay");
        }
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

    private static void WithReplay(Action<Replay> receiver)
    {
        lock (RecorderLock)
        {
            var replay = Replay;

            if (replay is null)
            {
                ErrorMeterValue = null;
                LastFloorIdJudgement = null;
                LastFloorIdKeyEvent = null;
                KeysWithoutAngleCorrection = 0;
            }
            else
            {
                receiver(replay);
            }
        }
    }

    public static void OnHitMargin(HitMargin hitMargin)
    {
        WithReplay(replay =>
        {
            var floorId = Adofai.CurrentFloorId;

            if (Adofai.Controller.midspinInfiniteMargin)
            {
                if (ErrorMeterValue is not null)
                    Main.Mod.Logger.Warning(
                        $"[Floor {floorId}] error meter already has a value on a midspin, overwriting to NaN"
                    );

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

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"meter: {errorMeter} margin: {hitMargin} dseq: {floorId - lastFloorId}");
        });
    }

    public static void OnErrorMeter(double errorMeter)
    {
        WithReplay(_ =>
        {
            var floorId = Adofai.CurrentFloorId;
            if (ErrorMeterValue is not null)
                Main.Mod.Logger.Warning($"[Floor {floorId}] error meter already has a value, overwriting");
            ErrorMeterValue = errorMeter;
        });
    }

    public static void OnKeyEvent(int keyCode, bool isKeyUp, double songSeconds)
    {
        WithReplay(replay =>
        {
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

            if (Settings.Instance.StoreSyncKeyCode)
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

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log(
                    $"key: {keyCode} up: {isKeyUp} dseq: {floorId - lastFloorId} pos: {songSeconds} auto: {autoFloor} locked: {inputLocked}");
        });
    }

    public static void OnAngleCorrection(double angle)
    {
        WithReplay(replay =>
        {
            for (; KeysWithoutAngleCorrection > 0; KeysWithoutAngleCorrection--)
            {
                replay.AngleCorrections.Add(angle);

                if (Settings.Instance.Verbose) Main.Mod.Logger.Log($"angle: {angle}");
            }
        });
    }

    public static void OnIterationStart(int stacked)
    {
        WithReplay(replay =>
        {
            LastIterationFirstKeyIndex = CurrentIterationFirstKeyIndex =
                Math.Max(0, replay.KeyEvents.Count - stacked);
        });
    }

    public static void OnKeysProcessed(int count)
    {
        WithReplay(replay =>
        {
            LastIterationFirstKeyIndex = CurrentIterationFirstKeyIndex;
            CurrentIterationFirstKeyIndex = Math.Min(CurrentIterationFirstKeyIndex + count, replay.KeyEvents.Count);

            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log(
                    $"iteration advance: +{count} {CurrentIterationFirstKeyIndex} total: {replay.KeyEvents.Count}");
        });
    }

    public static void OnMarkKeyEvent(bool auto, bool responsive)
    {
        WithReplay(replay =>
        {
            var i = CurrentIterationFirstKeyIndex;
            if (i >= replay.KeyEvents.Count) return;
            replay.KeyEvents[i] = MarkKeyEvent(replay.KeyEvents[i], auto, !responsive);
            if (Settings.Instance.Verbose)
                Main.Mod.Logger.Log($"mark input locked: index: {i} {auto} {!responsive}");
        });
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
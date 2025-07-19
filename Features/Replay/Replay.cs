using System.Collections.Generic;

namespace YqlossClientHarmony.Features.Replay;

public class Replay(Replay.MetadataType metadata)
{
    public MetadataType Metadata { get; } = metadata;
    public List<KeyEventType> KeyEvents { get; } = [];
    public List<JudgementType> Judgements { get; } = [];
    public List<double> AngleCorrections { get; } = [];

    public readonly struct MetadataType(
        int startingFloorId,
        // data below don't affect playing
        int totalFloorCount,
        string artist,
        string song,
        string author,
        Difficulty difficulty,
        bool noFailMode,
        bool useAsyncInput,
        HoldBehavior holdBehavior,
        HitMarginLimit hitMarginLimit
    )
    {
        public readonly int StartingFloorId = startingFloorId;
        public readonly int TotalFloorCount = totalFloorCount;
        public readonly string Artist = artist;
        public readonly string Song = song;
        public readonly string Author = author;
        public readonly Difficulty Difficulty = difficulty;
        public readonly bool NoFailMode = noFailMode;
        public readonly bool UseAsyncInput = useAsyncInput;
        public readonly HoldBehavior HoldBehavior = holdBehavior;
        public readonly HitMarginLimit HitMarginLimit = hitMarginLimit;
    }

    public readonly struct KeyEventType(
        double songSeconds,
        int keyCode,
        bool isKeyUp,
        int floorIdIncrement
    )
    {
        public readonly double SongSeconds = songSeconds;
        public readonly int KeyCode = keyCode;
        public readonly bool IsKeyUp = isKeyUp;
        public readonly int FloorIdIncrement = floorIdIncrement;
    }

    public readonly struct JudgementType(
        double errorMeter,
        HitMargin hitMargin,
        int floorIdIncrement
    )
    {
        public readonly double ErrorMeter = errorMeter;
        public readonly HitMargin HitMargin = hitMargin;
        public readonly int FloorIdIncrement = floorIdIncrement;
    }
}
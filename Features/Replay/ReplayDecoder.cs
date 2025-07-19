using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace YqlossClientHarmony.Features.Replay;

public static class ReplayDecoder
{
    private static void CheckMagicNumber(ulong magicNumber)
    {
        if (magicNumber != ReplayConstants.MagicNumber) throw new InvalidDataException("magic number mismatch");
    }

    private static int CheckVersion(int version, int currentVersion)
    {
        if (version <= 0) throw new InvalidDataException("version must be positive");
        if (version > currentVersion)
            throw new InvalidDataException(
                "the replay is created by newer versions of YCH. please update your mod!"
            );
        return version;
    }

    private static bool ProcessMetadata(
        BinaryReader reader,
        ref Replay.MetadataType? metadata
    )
    {
        var version = CheckVersion(reader.ReadInt32(), ReplayConstants.MetadataFormatVersion);

        // if (version < Replay.MetadataFormatVersion) ;

        if (metadata is not null) throw new InvalidDataException("multiple metadata blocks");

        metadata = new Replay.MetadataType(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadString(),
            reader.ReadString(),
            reader.ReadString(),
            (Difficulty)reader.ReadByte(),
            reader.ReadBoolean(),
            reader.ReadBoolean(),
            (HoldBehavior)reader.ReadByte(),
            (HitMarginLimit)reader.ReadByte()
        );

        return false;
    }

    private static bool ProcessKeyEvents(
        BinaryReader reader,
        ref List<Replay.KeyEventType>? keyEvents
    )
    {
        var version = CheckVersion(reader.ReadInt32(), ReplayConstants.KeyEventFormatVersion);

        // if (version < Replay.KeyEventFormatVersion) ;

        if (keyEvents is not null) throw new InvalidDataException("multiple key event blocks");

        var count = reader.ReadInt32();
        keyEvents = [];

        for (var i = 0; i < count; i++)
            keyEvents.Add(new Replay.KeyEventType(
                reader.ReadDouble(),
                reader.ReadInt32(),
                reader.ReadBoolean(),
                reader.ReadByte()
            ));

        return false;
    }

    private static bool ProcessJudgements(
        BinaryReader reader,
        ref List<Replay.JudgementType>? judgements
    )
    {
        var version = CheckVersion(reader.ReadInt32(), ReplayConstants.JudgementFormatVersion);

        // if (version < Replay.JudgementFormatVersion) ;

        if (judgements is not null) throw new InvalidDataException("multiple judgement blocks");

        var count = reader.ReadInt32();
        judgements = [];

        for (var i = 0; i < count; i++)
            judgements.Add(new Replay.JudgementType(
                reader.ReadDouble(),
                (HitMargin)reader.ReadByte(),
                reader.ReadByte()
            ));

        return false;
    }

    private static bool ProcessAngleCorrections(
        BinaryReader reader,
        ref List<double>? angleCorrections
    )
    {
        var version = CheckVersion(reader.ReadInt32(), ReplayConstants.JudgementFormatVersion);

        // if (version < Replay.JudgementFormatVersion) ;

        if (angleCorrections is not null) throw new InvalidDataException("multiple angle correction blocks");

        var count = reader.ReadInt32();
        angleCorrections = [];

        for (var i = 0; i < count; i++) angleCorrections.Add(reader.ReadDouble());

        return false;
    }

    private static void ProcessBlock(
        byte[] block,
        ref Replay.MetadataType? metadata,
        ref List<Replay.KeyEventType>? keyEvents,
        ref List<Replay.JudgementType>? judgements,
        ref List<double>? angleCorrections
    )
    {
        using var stream = new MemoryStream(block);
        var reader = new BinaryReader(stream);

        var magicNumber = reader.ReadUInt64();
        _ = magicNumber switch
        {
            ReplayConstants.MetadataMagicNumber => ProcessMetadata(reader, ref metadata),
            ReplayConstants.KeyEventMagicNumber => ProcessKeyEvents(reader, ref keyEvents),
            ReplayConstants.JudgementMagicNumber => ProcessJudgements(reader, ref judgements),
            ReplayConstants.AngleCorrectionMagicNumber => ProcessAngleCorrections(reader, ref angleCorrections),
            _ => throw new InvalidDataException($"invalid data block magic number: {magicNumber}")
        };
    }

    public static Replay ParseReplay(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var reader = new BinaryReader(stream);

        CheckMagicNumber(reader.ReadUInt64());
        var version = CheckVersion(reader.ReadInt32(), ReplayConstants.FormatVersion);

        // if (version < Replay.FormatVersion) ;

        var blockCount = reader.ReadUInt32();
        Replay.MetadataType? metadata = null;
        List<Replay.KeyEventType>? keyEvents = null;
        List<Replay.JudgementType>? judgements = null;
        List<double>? angleCorrections = null;

        for (var i = 0u; i < blockCount; i++)
        {
            var blockSize = reader.ReadUInt64();
            ProcessBlock(
                reader.ReadBytes((int)blockSize),
                ref metadata,
                ref keyEvents,
                ref judgements,
                ref angleCorrections
            );
        }

        if (metadata is null) throw new InvalidDataException("missing metadata");
        if (keyEvents is null) throw new InvalidDataException("missing key events");

        var replay = new Replay(metadata.Value);
        replay.KeyEvents.AddRange(keyEvents);
        replay.Judgements.AddRange(judgements ?? []);
        replay.AngleCorrections.AddRange(angleCorrections ?? []);
        return replay;
    }

    public static Replay? ReadCompressedReplay(string path)
    {
        try
        {
            byte[] decompressedBytes;

            {
                using var fileStream = new FileStream(path, FileMode.Open);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var memoryStream = new MemoryStream();
                gzipStream.CopyTo(memoryStream);
                decompressedBytes = memoryStream.ToArray();
            }

            return ParseReplay(decompressedBytes);
        }
        catch (Exception exception)
        {
            Main.Mod.Logger.Error($"failed to read replay: {exception}");
            return null;
        }
    }
}
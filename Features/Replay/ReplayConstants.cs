namespace YqlossClientHarmony.Features.Replay;

public static class ReplayConstants
{
    public const ulong MagicNumber = 0x73736F6C71592107;
    public const ulong MetadataMagicNumber = 0x617461646174654D;
    public const ulong KeyEventMagicNumber = 0x746E65764579654B;
    public const ulong JudgementMagicNumber = 0x6E656D656764754A;
    public const ulong AngleCorrectionMagicNumber = 0x726F43656C676E41;
    public const int FormatVersion = 1;
    public const int MetadataFormatVersion = 1;
    public const int KeyEventFormatVersion = 1;
    public const int JudgementFormatVersion = 1;
    public const int AngleCorrectionFormatVersion = 1;
}
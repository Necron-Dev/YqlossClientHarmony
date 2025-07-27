namespace YqlossClientHarmony.Features.Replay;

public static class ReplayUnityModManagerEventHandlers
{
    private static ConcurrentCondition ConditionPlayingCustom { get; } = new(() =>
        Adofai.Controller.gameworld && !ADOBase.isOfficialLevel && !Adofai.Controller.paused
    );

    public static void OnUpdate()
    {
        try
        {
            if (!ConditionPlayingCustom) return;
            using var _ = ConditionPlayingCustom;

            ReplayPlayer.HandleTrail();
        }
        catch
        {
            // ignored
        }
    }
}
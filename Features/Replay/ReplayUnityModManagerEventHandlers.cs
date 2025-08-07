namespace YqlossClientHarmony.Features.Replay;

public static class ReplayUnityModManagerEventHandlers
{
    public static void OnUpdate()
    {
        try
        {
            if (Adofai.Controller.paused) return;

            ReplayPlayer.HandleTrail();
        }
        catch
        {
            // ignored
        }
    }
}
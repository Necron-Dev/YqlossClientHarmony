using System;
using System.Collections.Generic;
using HarmonyLib;

namespace YqlossClientHarmony.Features.BlockUnintentionalEscape;

public static class Injections
{
    private static List<long> EscapePresses { get; } = [];

    [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SwitchToEditMode))]
    public static class Inject_scnEditor_SwitchToEditMode
    {
        public static bool Prefix()
        {
            if (!SettingsBlockUnintentionalEscape.Instance.Enabled) return true;

            if (SettingsBlockUnintentionalEscape.Instance.OnlyInGame && Adofai.Controller.state != States.PlayerControl) return true;

            var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var escapesRequired = SettingsBlockUnintentionalEscape.Instance.EscapesRequired;
            var inMilliseconds = (long)(SettingsBlockUnintentionalEscape.Instance.InSeconds * 1000);

            while (EscapePresses.Count > 0 && time - EscapePresses[0] > inMilliseconds)
                EscapePresses.RemoveAt(0);

            EscapePresses.Add(time);

            return EscapePresses.Count >= escapesRequired;
        }
    }
}
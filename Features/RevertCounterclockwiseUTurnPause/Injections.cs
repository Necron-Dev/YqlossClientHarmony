using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using HarmonyLib;

namespace YqlossClientHarmony.Features.RevertCounterclockwiseUTurnPause;

public static class Injections
{
    public static bool IsInPlayMethod { get; set; }

    [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Play))]
    public static class Inject_scnEditor_Play
    {
        public static void Prefix()
        {
            if (!Settings.Instance.Enabled) return;

            IsInPlayMethod = true;
        }

        public static Exception? Finalizer(
            Exception? __exception
        )
        {
            IsInPlayMethod = false;
            return __exception;
        }
    }

    [HarmonyPatch]
    public static class Inject_scnGame_ApplyCoreEventsToFloors
    {
        public static MethodBase TargetMethod()
        {
            return typeof(scnGame).GetMethod(
                "ApplyCoreEventsToFloors",
                [
                    typeof(List<scrFloor>),
                    typeof(LevelData),
                    typeof(scrLevelMaker),
                    typeof(List<LevelEvent>),
                    typeof(List<LevelEvent>[])
                ]
            )!;
        }

        public static void Prefix(
            List<scrFloor> floors,
            List<LevelEvent> events,
            List<LevelEvent>[]? floorEvents
        )
        {
            if (!Settings.Instance.Enabled) return;
            if (!IsInPlayMethod) return;

            List<LevelEvent>[] notNullFloorEvents;

            if (floorEvents == null)
            {
                notNullFloorEvents = new List<LevelEvent>[floors.Count];
                for (var i = 0; i < notNullFloorEvents.Length; ++i) notNullFloorEvents[i] = [];
                foreach (var floorEvent in events) notNullFloorEvents[floorEvent.floor].Add(floorEvent);
            }
            else
            {
                notNullFloorEvents = floorEvents;
            }

            var isCounterclockwise = false;

            foreach (var floor in floors)
            {
                notNullFloorEvents[floor.seqID]
                    .Where(floorEvent => floorEvent.eventType == LevelEventType.Twirl)
                    .ForEach(_ => isCounterclockwise = !isCounterclockwise);

                floor.isCCW = isCounterclockwise;
            }
        }
    }
}
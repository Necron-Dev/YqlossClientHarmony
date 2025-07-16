using System;
using System.Collections.Generic;

namespace YqlossClientHarmony.Features.ModifyLoadingLevel;

public class LevelSettingType(
    string? name,
    Func<Settings, bool> enabledSelector,
    Func<Settings, object?> overrideSelector,
    Action<Dictionary<string, object?>>? handler = null
)
{
    public static LevelSettingType[] Types { get; } =
    [
        new("hitsound", s => s.EnableHitsound, s => s.Hitsound),
        new("trackTexture", s => s.DisableTrackTexture, _ => ""),
        new("trackColorType", s => s.EnableTrackColorType, s => s.TrackColorType),
        new("trackStyle", s => s.EnableTrackStyle, s => s.TrackStyle),
        new("trackColor", s => s.EnableTrackColor, s => s.TrackColor),
        new("secondaryTrackColor", s => s.EnableSecondaryTrackColor, s => s.SecondaryTrackColor),
        new("trackColorAnimDuration", s => s.EnableColorAnimDuration, s => s.ColorAnimDuration),
        new("trackGlowIntensity", s => s.EnableTrackGlowIntensity, s => s.TrackGlowIntensity),
        new("trackAnimation", s => s.EnableTrackAnimation, s => s.TrackAnimation),
        new("beatsAhead", s => s.EnableBeatsAhead, s => s.BeatsAhead),
        new("trackDisappearAnimation", s => s.EnableTrackDisappearAnimation, s => s.TrackDisappearAnimation),
        new("beatsBehind", s => s.EnableBeatsBehind, s => s.BeatsBehind),
        new("bgImage", s => s.DisableBgImage, _ => ""),
        new("backgroundColor", s => s.EnableBackgroundColor, s => s.BackgroundColor),
        new("showDefaultBGIfNoImage", s => s.EnableShowDefaultBgTile, s => s.ShowDefaultBgTile),
        new("showDefaultBGTile", s => s.EnableShowDefaultBgTile, s => s.ShowDefaultBgTile),
        new("defaultBGTileColor", s => s.EnableDefaultBgTileColor, s => s.DefaultBgTileColor),
        new("defaultBGShapeType", s => s.EnableDefaultBgShapeType, s => s.DefaultBgShapeType),
        new("defaultBGShapeColor", s => s.EnableDefaultBgShapeColor, s => s.DefaultBgShapeColor),
        new("relativeTo", s => s.EnableRelativeTo, s => s.RelativeTo),
        new(
            "position",
            s => s.EnablePosition,
            _ => new object[] { Settings.Instance.PositionX, Settings.Instance.PositionY }
        ),
        new("rotation", s => s.EnableRotation, s => s.Rotation),
        new("zoom", s => s.EnableZoom, s => s.Zoom),
        new("pulseOnFloor", s => s.EnablePulseOnFloor, s => s.PulseOnFloor),
        new("bgVideo", s => s.DisableBgVideo, _ => ""),
        new("floorIconOutlines", s => s.EnableFloorIconOutlines, s => s.FloorIconOutlines),
        new("stickToFloors", s => s.EnableStickToFloors, s => s.StickToFloors),
        new("planetEase", s => s.EnablePlanetEase, s => s.PlanetEase),
        new("planetEaseParts", s => s.EnablePlanetEaseParts, s => s.PlanetEaseParts),
        new("planetEasePartBehavior", s => s.EnablePlanetEasePartBehavior, s => s.PlanetEasePartBehavior),
        new("defaultTextColor", s => s.EnableDefaultTextColor, s => s.DefaultTextColor),
        new("defaultTextShadowColor", s => s.EnableDefaultTextShadowColor, s => s.DefaultTextShadowColor),
        new("congratsText", s => s.EnableCongratsText, s => s.CongratsText),
        new("perfectText", s => s.EnablePerfectText, s => s.PerfectText)
    ];

    public void Modify(Dictionary<string, object?> settings)
    {
        if (!enabledSelector(Settings.Instance)) return;

        if (name is null) handler?.Invoke(settings);
        else settings[name] = overrideSelector(Settings.Instance);
    }
}
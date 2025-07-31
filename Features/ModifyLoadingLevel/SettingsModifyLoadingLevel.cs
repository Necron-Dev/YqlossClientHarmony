using JetBrains.Annotations;

namespace YqlossClientHarmony.Features.ModifyLoadingLevel;

[NoReorder]
public class SettingsModifyLoadingLevel
{
    public static SettingsModifyLoadingLevel Instance => Main.Settings.ModifyLoadingLevelSettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnableModifyLoadingLevel;

    public bool EnableHitsound = false;

    public string Hitsound = "Kick";

    public bool DisableTrackTexture = false;

    public bool EnableTrackColorType = false;

    public string TrackColorType = "Single";

    public bool EnableTrackStyle = false;

    public string TrackStyle = "Standard";

    public bool EnableTrackColor = false;

    public string TrackColor = "debb7b";

    public bool EnableSecondaryTrackColor = false;

    public string SecondaryTrackColor = "ffffff";

    public bool EnableColorAnimDuration = false;

    public double ColorAnimDuration = 2;

    public bool EnableTrackGlowIntensity = false;

    public double TrackGlowIntensity = 100;

    public bool EnableTrackAnimation = false;

    public string TrackAnimation = "None";

    public bool EnableBeatsAhead = false;

    public double BeatsAhead = 3;

    public bool EnableTrackDisappearAnimation = false;

    public string TrackDisappearAnimation = "None";

    public bool EnableBeatsBehind = false;

    public double BeatsBehind = 4;

    public bool DisableBgImage = false;

    public bool EnableBackgroundColor = false;

    public string BackgroundColor = "000000";

    public bool EnableShowDefaultBgTile = false;

    public bool ShowDefaultBgTile = true;

    public bool EnableDefaultBgTileColor = false;

    public string DefaultBgTileColor = "101121";

    public bool EnableDefaultBgShapeType = false;

    public string DefaultBgShapeType = "Default";

    public bool EnableDefaultBgShapeColor = false;

    public string DefaultBgShapeColor = "ffffff";

    public bool EnableRelativeTo = false;

    public string RelativeTo = "Player";

    public bool EnablePosition = false;

    public double PositionX = 0.0;

    public double PositionY = 0.0;

    public bool EnableRotation = false;

    public double Rotation = 0.0;

    public bool EnableZoom = false;

    public double Zoom = 100.0;

    public bool EnablePulseOnFloor = false;

    public bool PulseOnFloor = true;

    public bool DisableBgVideo = false;

    public bool EnableFloorIconOutlines = false;

    public bool FloorIconOutlines = false;

    public bool EnableStickToFloors = false;

    public bool StickToFloors = true;

    public bool EnablePlanetEase = false;

    public string PlanetEase = "Linear";

    public bool EnablePlanetEaseParts = false;

    public int PlanetEaseParts = 1;

    public bool EnablePlanetEasePartBehavior = false;

    public string PlanetEasePartBehavior = "Mirror";

    public bool EnableDefaultTextColor = false;

    public string DefaultTextColor = "ffffff";

    public bool EnableDefaultTextShadowColor = false;

    public string DefaultTextShadowColor = "00000050";

    public bool EnableCongratsText = false;

    public string CongratsText = "";

    public bool EnablePerfectText = false;

    public string PerfectText = "";

    public bool DisableAddDecoration = false;

    public bool DisableAddText = false;

    public bool DisableAddObject = false;

    public bool DisableAddParticle = false;

    public bool DisableKillerDecorations = false;

    public bool DisableSetHitsound = false;

    public bool DisablePlaySound = false;

    public bool DisableSetHoldSound = false;

    public bool DisableSetPlanetRotation = false;

    public bool DisableScalePlanets = false;

    public bool DisableScaleRadius = false;

    public bool DisableMoveCamera = false;

    public bool DisableCustomBackground = false;

    public bool DisableHide = false;

    public bool DisableMoveTrack = false;

    public bool DisablePositionTrack = false;

    public bool DisableColorTrack = false;

    public bool DisableAnimateTrack = false;

    public bool DisableRecolorTrack = false;

    public bool DisableMoveDecorations = false;

    public bool DisableSetText = false;

    public bool DisableEmitParticle = false;

    public bool DisableSetParticle = false;

    public bool DisableSetObject = false;

    public bool DisableSetDefaultText = false;

    public bool DisableFlash = false;

    public bool DisableSetFilter = false;

    public bool DisableSetFilterAdvanced = false;

    public bool DisableHallOfMirrors = false;

    public bool DisableShakeScreen = false;

    public bool DisableBloom = false;

    public bool DisableScreenTile = false;

    public bool DisableScreenScroll = false;

    public bool DisableSetFrameRate = false;

    public bool DisableEditorComment = false;

    public bool DisableBookmark = false;
}
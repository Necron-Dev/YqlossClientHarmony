using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace YqlossClientHarmony.Features.ModifyLoadingLevel;

[NoReorder]
public class Settings
{
    public static Settings Instance => Main.Settings.ModifyLoadingLevelSettings;

    public bool Enabled => Main.Enabled && Main.Settings.ModifyLoadingLevelEvents;

    [Header("Level Song Settings")] [Draw("Enable Hitsound Override")]
    public bool EnableHitsound = false;

    [Draw("Value")] public string Hitsound = "Kick";

    [Header("Level Track Settings")] [Draw("Disable Track Texture")]
    public bool DisableTrackTexture = false;

    [Draw("Enable Track Color Type Override")]
    public bool EnableTrackColorType = false;

    [Draw("Value")] public string TrackColorType = "Single";

    [Draw("Enable Track Style Override")] public bool EnableTrackStyle = false;

    [Draw("Value")] public string TrackStyle = "Standard";

    [Draw("Enable Track Color Override")] public bool EnableTrackColor = false;

    [Draw("Value")] public string TrackColor = "debb7b";

    [Draw("Enable Secondary Track Color Override")]
    public bool EnableSecondaryTrackColor = false;

    [Draw("Value")] public string SecondaryTrackColor = "ffffff";

    [Draw("Enable Color Animation Interval Override")]
    public bool EnableColorAnimDuration = false;

    [Draw("Value")] public double ColorAnimDuration = 2;

    [Draw("Enable Glow Intensity Override")]
    public bool EnableTrackGlowIntensity = false;

    [Draw("Value")] public double TrackGlowIntensity = 100;

    [Draw("Enable Track Appear Animation Override")]
    public bool EnableTrackAnimation = false;

    [Draw("Value")] public string TrackAnimation = "None";

    [Draw("Enable Beats Before For Animation Override")]
    public bool EnableBeatsAhead = false;

    [Draw("Value")] public double BeatsAhead = 3;

    [Draw("Enable Track Disappear Animation Override")]
    public bool EnableTrackDisappearAnimation = false;

    [Draw("Value")] public string TrackDisappearAnimation = "None";

    [Draw("Enable Beats After For Animation Override")]
    public bool EnableBeatsBehind = false;

    [Draw("Value")] public double BeatsBehind = 4;

    [Header("Level Background Settings")] [Draw("Disable Background Image")]
    public bool DisableBgImage = false;

    [Draw("Enable Background Color Override")]
    public bool EnableBackgroundColor = false;

    [Draw("Value")] public string BackgroundColor = "000000";

    [Draw("Enable Show Tutorial Background Pattern Override")]
    public bool EnableShowDefaultBgTile = false;

    [Draw("Value")] public bool ShowDefaultBgTile = true;

    [Draw("Enable Tutorial Background Pattern Color Override")]
    public bool EnableDefaultBgTileColor = false;

    [Draw("Value")] public string DefaultBgTileColor = "101121";

    [Draw("Enable Tutorial Background Shape Override")]
    public bool EnableDefaultBgShapeType = false;

    [Draw("Value")] public string DefaultBgShapeType = "Default";

    [Draw("Enable Tutorial Background Shape Color Override")]
    public bool EnableDefaultBgShapeColor = false;

    [Draw("Value")] public string DefaultBgShapeColor = "ffffff";

    [Header("Level Camera Settings")] [Draw("Enable Relative To Override")]
    public bool EnableRelativeTo = false;

    [Draw("Value")] public string RelativeTo = "Player";

    [Draw("Enable Position Override")] public bool EnablePosition = false;

    [Draw("Value")] public double PositionX = 0.0;

    [Draw("Value")] public double PositionY = 0.0;

    [Draw("Enable Rotation Override")] public bool EnableRotation = false;

    [Draw("Value")] public double Rotation = 0.0;

    [Draw("Enable Zoom Override")] public bool EnableZoom = false;

    [Draw("Value")] public double Zoom = 100.0;

    [Draw("Enable Pulse Camera At Tiles Override")]
    public bool EnablePulseOnFloor = false;

    [Draw("Value")] public bool PulseOnFloor = true;

    [Header("Level Misc Settings")] [Draw("Disable Video Background")]
    public bool DisableBgVideo = false;

    [Draw("Enable Tile Icon Outlines Override")]
    public bool EnableFloorIconOutlines = false;

    [Draw("Value")] public bool FloorIconOutlines = false;

    [Draw("Enable Sticky Tiles Override")] public bool EnableStickToFloors = false;

    [Draw("Value")] public bool StickToFloors = true;

    [Draw("Enable Planet Orbit Ease Override")]
    public bool EnablePlanetEase = false;

    [Draw("Value")] public string PlanetEase = "Linear";

    [Draw("Enable Orbit Ease Parts Override")]
    public bool EnablePlanetEaseParts = false;

    [Draw("Value")] public int PlanetEaseParts = 1;

    [Draw("Enable Ease Part Behavior Override")]
    public bool EnablePlanetEasePartBehavior = false;

    [Draw("Value")] public string PlanetEasePartBehavior = "Mirror";

    [Draw("Enable Default Text Color Override")]
    public bool EnableDefaultTextColor = false;

    [Draw("Value")] public string DefaultTextColor = "ffffff";

    [Draw("Enable Default Text Shadow Color Override")]
    public bool EnableDefaultTextShadowColor = false;

    [Draw("Value")] public string DefaultTextShadowColor = "00000050";

    [Draw("Enable Congratulations Text Override")]
    public bool EnableCongratsText = false;

    [Draw("Value")] public string CongratsText = "";

    [Draw("Enable Pure Perfect Text Override")]
    public bool EnablePerfectText = false;

    [Draw("Value")] public string PerfectText = "";

    [Header("Decorations")] [Draw("Disable Image")]
    public bool DisableAddDecoration = false;

    [Draw("Disable Text")] public bool DisableAddText = false;

    [Draw("Disable Object")] public bool DisableAddObject = false;

    [Draw("Disable Particle Emitter")] public bool DisableAddParticle = false;

    [Draw("Disable Killer Decorations")] public bool DisableKillerDecorations = false;

    [Header("Functional Actions")] [Draw("Disable Checkpoint")]
    public bool DisableCheckpoint = false;

    [Draw("Disable AutoPlay Tiles")] public bool DisableAutoPlayTiles = false;

    [Draw("Disable Repeat Events")] public bool DisableRepeatEvents = false;

    [Draw("Disable Set Conditional Events")]
    public bool DisableSetConditionalEvents = false;

    [Draw("Disable Set Input Event")] public bool DisableSetInputEvent = false;

    [Draw("Disable Timing Window Scale")] public bool DisableScaleMargin = false;

    [Header("Auditory Actions")] [Draw("Disable Set Hitsound")]
    public bool DisableSetHitsound = false;

    [Draw("Disable Play Sound")] public bool DisablePlaySound = false;

    [Draw("Disable Set Hold Sound")] public bool DisableSetHoldSound = false;

    [Header("Planet-Related Visual Actions")] [Draw("Disable Set Planet Orbit")]
    public bool DisableSetPlanetRotation = false;

    [Draw("Disable Scale Planets")] public bool DisableScalePlanets = false;

    [Draw("Disable Planet Radius Scale")] public bool DisableScaleRadius = false;

    [Header("Misc Visual Actions")] [Draw("Disable Move Camera")]
    public bool DisableMoveCamera = false;

    [Draw("Disable Set Background")] public bool DisableCustomBackground = false;

    [Header("Track-Related Visual Actions")] [Draw("Disable Hide Judgement/Floor Icons")]
    public bool DisableHide = false;

    [Draw("Disable Move Track")] public bool DisableMoveTrack = false;

    [Draw("Disable Position Track")] public bool DisablePositionTrack = false;

    [Draw("Disable Set Track Color")] public bool DisableColorTrack = false;

    [Draw("Disable Set Track Animation")] public bool DisableAnimateTrack = false;

    [Draw("Disable Recolor Track")] public bool DisableRecolorTrack = false;

    [Header("Decoration-Related Visual Actions")] [Draw("Disable Move Decorations")]
    public bool DisableMoveDecorations = false;

    [Draw("Disable Set Text")] public bool DisableSetText = false;

    [Draw("Disable Emit Particle")] public bool DisableEmitParticle = false;

    [Draw("Disable Set Particle")] public bool DisableSetParticle = false;

    [Draw("Disable Set Object")] public bool DisableSetObject = false;

    [Draw("Disable Set Default Text")] public bool DisableSetDefaultText = false;

    [Header("Visual Effect Actions")] [Draw("Disable Flash")]
    public bool DisableFlash = false;

    [Draw("Disable Set Filter")] public bool DisableSetFilter = false;

    [Draw("Disable Set Filter Advanced")] public bool DisableSetFilterAdvanced = false;

    [Draw("Disable Hall of Mirrors")] public bool DisableHallOfMirrors = false;

    [Draw("Disable Shake Screen")] public bool DisableShakeScreen = false;

    [Draw("Disable Bloom")] public bool DisableBloom = false;

    [Draw("Disable Tile Screen")] public bool DisableScreenTile = false;

    [Draw("Disable Scroll Screen")] public bool DisableScreenScroll = false;

    [Draw("Disable Set Frame Rate")] public bool DisableSetFrameRate = false;

    [Header("Misc Actions")] [Draw("Disable Editor Comment")]
    public bool DisableEditorComment = false;

    [Draw("Disable Bookmark")] public bool DisableBookmark = false;
}
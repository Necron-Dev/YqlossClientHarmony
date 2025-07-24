using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace YqlossClientHarmony.Features.ModifyLoadingLevel;

[NoReorder]
public class SettingsModifyLoadingLevel
{
    public static SettingsModifyLoadingLevel Instance => Main.Settings.ModifyLoadingLevelSettings;

    public bool Enabled => Main.Enabled && Main.Settings.EnableModifyLoadingLevel;

    [Header("以下所有“值”文本框均只能填写英文，也就是 .adofai 文件里的值！")]
    [Header("Level Song Settings 关卡歌曲设置")]
    [Draw("Enable Hitsound Override 开启覆盖 打拍声")]
    public bool EnableHitsound = false;

    [Draw("Value 值")] public string Hitsound = "Kick";

    [Header("Level Track Settings 关卡轨道设置")] [Draw("Disable Track Texture 禁用轨道纹理")]
    public bool DisableTrackTexture = false;

    [Draw("Enable Track Color Type Override 开启覆盖 轨道颜色类型")]
    public bool EnableTrackColorType = false;

    [Draw("Value 值")] public string TrackColorType = "Single";

    [Draw("Enable Track Style Override 开启覆盖 轨道风格")]
    public bool EnableTrackStyle = false;

    [Draw("Value 值")] public string TrackStyle = "Standard";

    [Draw("Enable Track Color Override 开启覆盖 轨道主色调")]
    public bool EnableTrackColor = false;

    [Draw("Value 值")] public string TrackColor = "debb7b";

    [Draw("Enable Secondary Track Color Override 开启覆盖 轨道副色调")]
    public bool EnableSecondaryTrackColor = false;

    [Draw("Value 值")] public string SecondaryTrackColor = "ffffff";

    [Draw("Enable Color Animation Interval Override 开启覆盖 色彩动画间隔")]
    public bool EnableColorAnimDuration = false;

    [Draw("Value 值")] public double ColorAnimDuration = 2;

    [Draw("Enable Glow Intensity Override 开启覆盖 辉度")]
    public bool EnableTrackGlowIntensity = false;

    [Draw("Value 值")] public double TrackGlowIntensity = 100;

    [Draw("Enable Track Appear Animation Override 开启覆盖 轨道出现动画")]
    public bool EnableTrackAnimation = false;

    [Draw("Value 值")] public string TrackAnimation = "None";

    [Draw("Enable Beats Before For Animation Override 开启覆盖 动画前节拍")]
    public bool EnableBeatsAhead = false;

    [Draw("Value 值")] public double BeatsAhead = 3;

    [Draw("Enable Track Disappear Animation Override 开启覆盖 轨道消失动画")]
    public bool EnableTrackDisappearAnimation = false;

    [Draw("Value 值")] public string TrackDisappearAnimation = "None";

    [Draw("Enable Beats After For Animation Override 开启覆盖 动画后节拍")]
    public bool EnableBeatsBehind = false;

    [Draw("Value 值")] public double BeatsBehind = 4;

    [Header("Level Background Settings 关卡背景设置")] [Draw("Disable Background Image 禁用背景图片")]
    public bool DisableBgImage = false;

    [Draw("Enable Background Color Override 开启覆盖 背景颜色")]
    public bool EnableBackgroundColor = false;

    [Draw("Value 值")] public string BackgroundColor = "000000";

    [Draw("Enable Show Tutorial Background Pattern Override 开启覆盖 显示教程背景图案")]
    public bool EnableShowDefaultBgTile = false;

    [Draw("Value 值")] public bool ShowDefaultBgTile = true;

    [Draw("Enable Tutorial Background Pattern Color Override 开启覆盖 教程背景图案颜色")]
    public bool EnableDefaultBgTileColor = false;

    [Draw("Value 值")] public string DefaultBgTileColor = "101121";

    [Draw("Enable Tutorial Background Shape Override 开启覆盖 教程背景图形")]
    public bool EnableDefaultBgShapeType = false;

    [Draw("Value 值")] public string DefaultBgShapeType = "Default";

    [Draw("Enable Tutorial Background Shape Color Override 开启覆盖 教程背景图形颜色")]
    public bool EnableDefaultBgShapeColor = false;

    [Draw("Value 值")] public string DefaultBgShapeColor = "ffffff";

    [Header("Level Camera Settings 关卡摄像头设置")] [Draw("Enable Relative To Override 开启覆盖 关联到")]
    public bool EnableRelativeTo = false;

    [Draw("Value 值")] public string RelativeTo = "Player";

    [Draw("Enable Position Override 开启覆盖 位置")]
    public bool EnablePosition = false;

    [Draw("X Value X 位置")] public double PositionX = 0.0;

    [Draw("Y Value Y 位置")] public double PositionY = 0.0;

    [Draw("Enable Rotation Override 开启覆盖 角度")]
    public bool EnableRotation = false;

    [Draw("Value 值")] public double Rotation = 0.0;

    [Draw("Enable Zoom Override 开启覆盖 缩放")] public bool EnableZoom = false;

    [Draw("Value 值")] public double Zoom = 100.0;

    [Draw("Enable Pulse Camera At Tiles Override 开启覆盖 触发方块时摄像头脉动")]
    public bool EnablePulseOnFloor = false;

    [Draw("Value 值")] public bool PulseOnFloor = true;

    [Header("Level Misc Settings 关卡杂项设置")] [Draw("Disable Video Background 禁用视频背景")]
    public bool DisableBgVideo = false;

    [Draw("Enable Tile Icon Outlines Override 开启覆盖 方块图标描边")]
    public bool EnableFloorIconOutlines = false;

    [Draw("Value 值")] public bool FloorIconOutlines = false;

    [Draw("Enable Sticky Tiles Override 开启覆盖 黏性方块")]
    public bool EnableStickToFloors = false;

    [Draw("Value 值")] public bool StickToFloors = true;

    [Draw("Enable Planet Orbit Ease Override 开启覆盖 星球轨道缓速")]
    public bool EnablePlanetEase = false;

    [Draw("Value 值")] public string PlanetEase = "Linear";

    [Draw("Enable Orbit Ease Parts Override 开启覆盖 轨道缓速部分")]
    public bool EnablePlanetEaseParts = false;

    [Draw("Value 值")] public int PlanetEaseParts = 1;

    [Draw("Enable Ease Part Behavior Override 开启覆盖 缓速部分行为")]
    public bool EnablePlanetEasePartBehavior = false;

    [Draw("Value 值")] public string PlanetEasePartBehavior = "Mirror";

    [Draw("Enable Default Text Color Override 开启覆盖 默认文本颜色")]
    public bool EnableDefaultTextColor = false;

    [Draw("Value 值")] public string DefaultTextColor = "ffffff";

    [Draw("Enable Default Text Shadow Color Override 开启覆盖 默认文本阴影颜色")]
    public bool EnableDefaultTextShadowColor = false;

    [Draw("Value 值")] public string DefaultTextShadowColor = "00000050";

    [Draw("Enable Congratulations Text Override 开启覆盖 “庆贺！”文本")]
    public bool EnableCongratsText = false;

    [Draw("Value 值")] public string CongratsText = "";

    [Draw("Enable Pure Perfect Text Override 开启覆盖 “完美无瑕！”文本")]
    public bool EnablePerfectText = false;

    [Draw("Value 值")] public string PerfectText = "";

    [Header("Decorations 装饰")] [Draw("Disable Image 禁用装饰（图像装饰）")]
    public bool DisableAddDecoration = false;

    [Draw("Disable Text 禁用文本装饰")] public bool DisableAddText = false;

    [Draw("Disable Object 禁用对象装饰")] public bool DisableAddObject = false;

    [Draw("Disable Particle Emitter 禁用粒子发射器")]
    public bool DisableAddParticle = false;

    [Draw("Disable Killer Decorations 禁用死亡装饰物")]
    public bool DisableKillerDecorations = false;

    [Header("Functional Actions 功能型事件")] [Draw("Disable Checkpoint 禁用检查点")]
    public bool DisableCheckpoint = false;

    [Draw("Disable AutoPlay Tiles 禁用自动播放格子")]
    public bool DisableAutoPlayTiles = false;

    [Draw("Disable Repeat Events 禁用重复事件")] public bool DisableRepeatEvents = false;

    [Draw("Disable Set Conditional Events 禁用设置条件事件")]
    public bool DisableSetConditionalEvents = false;

    [Draw("Disable Set Input Event 禁用设置输入事件")]
    public bool DisableSetInputEvent = false;

    [Draw("Disable Timing Window Scale 禁用定时窗口大小")]
    public bool DisableScaleMargin = false;

    [Header("Auditory Actions 音效事件")] [Draw("Disable Set Hitsound 禁用设置打拍音")]
    public bool DisableSetHitsound = false;

    [Draw("Disable Play Sound 禁用播放音效")] public bool DisablePlaySound = false;

    [Draw("Disable Set Hold Sound 禁用设置长按音效")]
    public bool DisableSetHoldSound = false;

    [Header("Planet-Related Visual Actions 星球相关的视觉事件")] [Draw("Disable Set Planet Orbit 禁用设置星球轨道")]
    public bool DisableSetPlanetRotation = false;

    [Draw("Disable Scale Planets 禁用缩放行星")] public bool DisableScalePlanets = false;

    [Draw("Disable Planet Radius Scale 禁用星球半径大小")]
    public bool DisableScaleRadius = false;

    [Header("Misc Visual Actions 其他视觉事件")] [Draw("Disable Move Camera 禁用移动摄像头")]
    public bool DisableMoveCamera = false;

    [Draw("Disable Set Background 禁用设置背景")]
    public bool DisableCustomBackground = false;

    [Header("Track-Related Visual Actions 轨道相关的视觉事件")] [Draw("Disable Hide Judgement/Floor Icons 禁用隐藏判定/地板图标")]
    public bool DisableHide = false;

    [Draw("Disable Move Track 禁用移动轨道")] public bool DisableMoveTrack = false;

    [Draw("Disable Position Track 禁用位置轨道")]
    public bool DisablePositionTrack = false;

    [Draw("Disable Set Track Color 禁用设置轨道颜色")]
    public bool DisableColorTrack = false;

    [Draw("Disable Set Track Animation 设置轨道动画")]
    public bool DisableAnimateTrack = false;

    [Draw("Disable Recolor Track 禁用重新设置轨道颜色")]
    public bool DisableRecolorTrack = false;

    [Header("Decoration-Related Visual Actions 装饰相关的视觉事件")] [Draw("Disable Move Decorations 禁用移动装饰")]
    public bool DisableMoveDecorations = false;

    [Draw("Disable Set Text 禁用设置文本")] public bool DisableSetText = false;

    [Draw("Disable Emit Particle 禁用发射粒子")] public bool DisableEmitParticle = false;

    [Draw("Disable Set Particle 禁用设置粒子")] public bool DisableSetParticle = false;

    [Draw("Disable Set Object 禁用设置对象")] public bool DisableSetObject = false;

    [Draw("Disable Set Default Text 禁用设置默认文本")]
    public bool DisableSetDefaultText = false;

    [Header("Visual Effect Actions 视觉效果事件")] [Draw("Disable Flash 禁用闪光")]
    public bool DisableFlash = false;

    [Draw("Disable Set Filter 禁用预设滤镜")] public bool DisableSetFilter = false;

    [Draw("Disable Set Filter Advanced 禁用预设高级滤镜")]
    public bool DisableSetFilterAdvanced = false;

    [Draw("Disable Hall of Mirrors 禁用镜厅")] public bool DisableHallOfMirrors = false;

    [Draw("Disable Shake Screen 禁用振屏")] public bool DisableShakeScreen = false;

    [Draw("Disable Bloom 禁用绽放")] public bool DisableBloom = false;

    [Draw("Disable Tile Screen 禁用平铺")] public bool DisableScreenTile = false;

    [Draw("Disable Scroll Screen 禁用卷屏")] public bool DisableScreenScroll = false;

    [Draw("Disable Set Frame Rate 禁用设置帧率")]
    public bool DisableSetFrameRate = false;

    [Header("Misc Actions 其他事件")] [Draw("Disable Editor Comment 禁用编辑器附注")]
    public bool DisableEditorComment = false;

    [Draw("Disable Bookmark 禁用书签")] public bool DisableBookmark = false;
}
using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using YqlossClientHarmony.Features.Replay;

namespace YqlossClientHarmony;

public static class Main
{
    private static UnityModManager.ModEntry? _mod;

    private static Settings? _settings;

    private static bool _enabled;

    public static UnityModManager.ModEntry Mod
    {
        get => _mod ?? throw new NullReferenceException("use Main.Mod before the mod is initialized");
        set
        {
            if (ReferenceEquals(_mod, value)) return;
            _mod = value;

            _mod.OnToggle += (_, enabled) =>
            {
                Enabled = enabled;
                return true;
            };

            _mod.OnGUI += mod => Settings.DrawGUI(mod);

            _mod.OnSaveGUI += mod => Settings.Save(mod);

            _mod.OnUpdate += OnUpdate;
        }
    }

    public static Settings Settings
    {
        get => _settings ?? throw new NullReferenceException("use Main.Settings before the mod is initialized");
        set
        {
            if (ReferenceEquals(_settings, value)) return;
            _settings = value;

            _settings.OnSettingChange += OnSettingChange;
        }
    }

    public static bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            OnToggle(value);
        }
    }

    public static Harmony Harmony { get; } = new("YCH");

    public static bool Load(UnityModManager.ModEntry mod)
    {
        Mod = mod;
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        Settings = UnityModManager.ModSettings.Load<Settings>(Mod);
        Enabled = true;
        return true;
    }

    private static void OnToggle(bool enabled)
    {
    }

    private static void OnSettingChange()
    {
    }

    private static void OnUpdate(UnityModManager.ModEntry mod, float _)
    {
        try
        {
            ReplayUnityModManagerEventHandlers.OnUpdate();
        }
        catch
        {
            // ignored
        }
    }
}
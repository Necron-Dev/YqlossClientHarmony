using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YqlossClientHarmony.Features.Replay;

public static class KeyCodeMapping
{
    private static Dictionary<int, KeyCode> AsyncToSync { get; } = new()
    {
        { 4288, KeyCode.BackQuote }, // Grave
        { 4123, KeyCode.Escape }, // Escape
        { 4128, KeyCode.Space }, // Space
        { 4177, KeyCode.Q }, // Q
        { 4183, KeyCode.W }, // W
        { 4165, KeyCode.E }, // E
        { 4178, KeyCode.R }, // R
        { 4180, KeyCode.T }, // T
        { 4185, KeyCode.Y }, // Y
        { 4181, KeyCode.U }, // U
        { 4169, KeyCode.I }, // I
        { 4175, KeyCode.O }, // O
        { 4176, KeyCode.P }, // P
        { 4258, KeyCode.LeftControl }, // LControl
        { 4217, KeyCode.F10 }, // F10
        { 4115, KeyCode.Pause }, // PauseBreak
        { 4208, KeyCode.F1 }, // F1
        { 4209, KeyCode.F2 }, // F2
        { 4210, KeyCode.F3 }, // F3
        { 4211, KeyCode.F4 }, // F4
        { 4212, KeyCode.F5 }, // F5
        { 4213, KeyCode.F6 }, // F6
        { 4214, KeyCode.F7 }, // F7
        { 4215, KeyCode.F8 }, // F8
        { 4216, KeyCode.F9 }, // F9
        { 4218, KeyCode.F11 }, // F11
        { 4219, KeyCode.F12 }, // F12
        { 4145, KeyCode.Alpha1 }, // Alpha1
        { 4146, KeyCode.Alpha2 }, // Alpha2
        { 4147, KeyCode.Alpha3 }, // Alpha3
        { 4148, KeyCode.Alpha4 }, // Alpha4
        { 4149, KeyCode.Alpha5 }, // Alpha5
        { 4150, KeyCode.Alpha6 }, // Alpha6
        { 4151, KeyCode.Alpha7 }, // Alpha7
        { 4152, KeyCode.Alpha8 }, // Alpha8
        { 4153, KeyCode.Alpha9 }, // Alpha9
        { 4144, KeyCode.Alpha0 }, // Alpha0
        { 4285, KeyCode.Minus }, // Minus
        { 4283, KeyCode.Equals }, // Equal
        { 4104, KeyCode.Backspace }, // Backspace
        { 4105, KeyCode.Tab }, // Tab
        { 4315, KeyCode.LeftBracket }, // LeftBrace
        { 4317, KeyCode.RightBracket }, // RightBrace
        { 4316, KeyCode.Backslash }, // BackSlash
        { 4116, KeyCode.CapsLock }, // CapsLock
        { 4161, KeyCode.A }, // A
        { 4179, KeyCode.S }, // S
        { 4164, KeyCode.D }, // D
        { 4166, KeyCode.F }, // F
        { 4167, KeyCode.G }, // G
        { 4168, KeyCode.H }, // H
        { 4170, KeyCode.J }, // J
        { 4171, KeyCode.K }, // K
        { 4172, KeyCode.L }, // L
        { 4282, KeyCode.Semicolon }, // Semicolon
        { 4318, KeyCode.Quote }, // Apostrophe
        { 4109, KeyCode.Return }, // Enter
        { 4256, KeyCode.LeftShift }, // LShift
        { 4186, KeyCode.Z }, // Z
        { 4184, KeyCode.X }, // X
        { 4163, KeyCode.C }, // C
        { 4182, KeyCode.V }, // V
        { 4162, KeyCode.B }, // B
        { 4174, KeyCode.N }, // N
        { 4173, KeyCode.M }, // M
        { 4284, KeyCode.Comma }, // Comma
        { 4286, KeyCode.Period }, // Dot
        { 4287, KeyCode.Slash }, // Slash
        { 4257, KeyCode.RightShift }, // RShift
        { 4187, KeyCode.LeftWindows }, // Super
        { 4260, KeyCode.LeftAlt }, // LAlt
        { 4261, KeyCode.RightAlt }, // RAlt
        { 4189, KeyCode.Menu }, // (Unknown)
        { 4259, KeyCode.RightControl }, // RControl
        { 4134, KeyCode.UpArrow }, // ArrowUp
        { 4136, KeyCode.DownArrow }, // ArrowDown
        { 4133, KeyCode.LeftArrow }, // ArrowLeft
        { 4135, KeyCode.RightArrow }, // ArrowRight
        { 4140, KeyCode.Print }, // PrintScreen
        { 4241, KeyCode.ScrollLock }, // ScrollLock
        { 4141, KeyCode.Insert }, // Insert
        { 4132, KeyCode.Home }, // Home
        { 4129, KeyCode.PageUp }, // PageUp
        { 4142, KeyCode.Delete }, // Delete
        { 4131, KeyCode.End }, // End
        { 4130, KeyCode.PageDown }, // PageDown
        { 4240, KeyCode.Numlock }, // NumLock
        { 4207, KeyCode.KeypadDivide }, // KeypadSlash
        { 4202, KeyCode.KeypadMultiply }, // KeypadAsterisk
        { 4205, KeyCode.KeypadMinus }, // KeypadMinus
        { 4199, KeyCode.Keypad7 }, // Keypad7
        { 4200, KeyCode.Keypad8 }, // Keypad8
        { 4201, KeyCode.Keypad9 }, // Keypad9
        { 4203, KeyCode.KeypadPlus }, // KeypadPlus
        { 4196, KeyCode.Keypad4 }, // Keypad4
        { 4197, KeyCode.Keypad5 }, // Keypad5
        { 4198, KeyCode.Keypad6 }, // Keypad6
        { 4193, KeyCode.Keypad1 }, // Keypad1
        { 4194, KeyCode.Keypad2 }, // Keypad2
        { 4195, KeyCode.Keypad3 }, // Keypad3
        { 4192, KeyCode.Keypad0 }, // Keypad0
        { 4206, KeyCode.KeypadPeriod } // KeypadDot
    };

    private static Dictionary<KeyCode, int> SyncToAsync { get; } =
        AsyncToSync
            .Keys
            .ToDictionary(
                it => AsyncToSync[it],
                it => it
            );

    public static int GetSyncKeyCode(int keyCode)
    {
        return (int)AsyncToSync.GetValueOrDefault(keyCode, (KeyCode)keyCode);
    }

    public static int GetAsyncKeyCode(KeyCode keyCode)
    {
        return SyncToAsync.GetValueOrDefault(keyCode, (int)keyCode);
    }
}
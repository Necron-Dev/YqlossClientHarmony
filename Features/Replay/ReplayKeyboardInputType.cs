using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YqlossClientHarmony.Features.Replay;

public class ReplayKeyboardInputType : RDInputType
{
    private static readonly KeyCode[] LeftKeys =
    [
        KeyCode.Q,
        KeyCode.W,
        KeyCode.E,
        KeyCode.R,
        KeyCode.T,
        KeyCode.CapsLock,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F,
        KeyCode.G,
        KeyCode.LeftShift,
        KeyCode.Z,
        KeyCode.X,
        KeyCode.C,
        KeyCode.V,
        KeyCode.B
    ];

    private static readonly KeyCode[] RightKeys =
    [
        KeyCode.Y,
        KeyCode.U,
        KeyCode.I,
        KeyCode.O,
        KeyCode.P,
        KeyCode.LeftBracket,
        KeyCode.RightBracket,
        KeyCode.H,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
        KeyCode.Semicolon,
        KeyCode.Quote,
        KeyCode.N,
        KeyCode.M,
        KeyCode.Comma,
        KeyCode.Period,
        KeyCode.Slash,
        KeyCode.RightShift,
        KeyCode.Return
    ];

    private static readonly KeyCode[] ConfirmKeys =
    [
        KeyCode.Space,
        KeyCode.Return,
        KeyCode.KeypadEnter
    ];

    private ReplayKeyboardInputType()
    {
        schemeIndex = 0;
        isActive = true;
    }

    public static ReplayKeyboardInputType Instance { get; } = new();

    public static List<RDInputType> SingletonList { get; } = [Instance];

    private static bool CheckKeyState(KeyCode key, ButtonState state = ButtonState.WentDown)
    {
        return state switch
        {
            ButtonState.WentDown => Input.GetKeyDown(key),
            ButtonState.WentUp => Input.GetKeyUp(key),
            ButtonState.IsDown => Input.GetKey(key),
            ButtonState.IsUp => !Input.GetKey(key),
            _ => false
        };
    }

    private static bool CheckAnyKeyState(KeyCode[] keys, ButtonState state = ButtonState.WentDown)
    {
        return keys.Any(key => CheckKeyState(key, state));
    }

    public override int Main(ButtonState state)
    {
        var stateCount = GetStateCount(state);
        stateCount.lastFrameUpdated = Time.frameCount;
        stateCount.keys = [];
        var keys = new List<KeyCode>();

        for (var keyCode = 0; keyCode < 320; ++keyCode)
            if (CheckKeyState((KeyCode)keyCode, state))
                keys.Add((KeyCode)keyCode);

        if (state == ButtonState.WentDown && Cancel()) keys.Remove(KeyCode.Escape);

        foreach (var keyCode in keys)
            stateCount.keys.Add(new AnyKeyCode(keyCode));

        return stateCount.keys.Count;
    }

    public override bool Restart()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    public override bool Cancel()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    public override bool Quit()
    {
        return Input.GetKeyDown(KeyCode.Q);
    }

    public override bool Left(ButtonState state)
    {
        return CheckKeyState(KeyCode.LeftArrow, state);
    }

    public override bool Right(ButtonState state)
    {
        return CheckKeyState(KeyCode.RightArrow, state);
    }

    public override bool Up(ButtonState state)
    {
        return CheckKeyState(KeyCode.UpArrow, state);
    }

    public override bool Down(ButtonState state)
    {
        return CheckKeyState(KeyCode.DownArrow, state);
    }

    public override bool LeftAlt(ButtonState state)
    {
        return CheckKeyState(KeyCode.LeftShift, state);
    }

    public override bool RightAlt(ButtonState state)
    {
        return CheckKeyState(KeyCode.RightShift, state);
    }

    public override bool UpAlt(ButtonState state)
    {
        return false;
    }

    public override bool DownAlt(ButtonState state)
    {
        return false;
    }

    public override bool Action1(ButtonState state)
    {
        return CheckAnyKeyState(LeftKeys, state);
    }

    public override bool Action2(ButtonState state)
    {
        return CheckAnyKeyState(RightKeys, state);
    }

    public override bool Confirm(ButtonState state)
    {
        return CheckAnyKeyState(ConfirmKeys, state);
    }
}
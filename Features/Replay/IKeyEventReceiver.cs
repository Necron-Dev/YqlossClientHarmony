using UnityEngine;

namespace YqlossClientHarmony.Features.Replay;

public interface IKeyEventReceiver
{
    void Begin();

    void End();

    void OnKey(KeyCode code, bool isKeyDown);
}
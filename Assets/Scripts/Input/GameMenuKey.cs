using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class GameMenuKey : KeyBinding
{
    public override Key? DefaultKey => Key.Escape;

    public override GamepadButton? DefaultButton => GamepadButton.Start;

    public override MouseButton? DefaultMouseButton => null;

    public override int? InputType => KeyInput.PlayerInputType;

}

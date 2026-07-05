using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class JumpKey : KeyBinding
{
    public override Key? DefaultKey => Key.Space;

    public override GamepadButton? DefaultButton => GamepadButton.South;

    public override MouseButton? DefaultMouseButton => null;

    public override int? InputType => KeyInput.PlayerInputType;
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class CameraStick : StickBinding
{
    public override Stick? DefaultStick => Stick.Right;

    public override IReadOnlyList<Key?> DefaultKeys => new Key?[]{Key.RightArrow, Key.LeftArrow, null, null};

    public override IReadOnlyList<GamepadButton?> DefaultButtons => new GamepadButton?[]{null, null, null, null};

    public override IReadOnlyList<MouseButton?> DefaultMouseButtons => new MouseButton?[]{null, null, null, null};

    public override int? InputType => KeyInput.PlayerInputType;

    public override bool NormalisedAbove1 => false;

    public override bool HasAxisDeadZone => true;

    public override bool HasMagnitudeDeadZone => false;

    public override bool UsesYAxis => false;

    public override bool UsesXAxis => true;
}

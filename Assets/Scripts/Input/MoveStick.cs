using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class MoveStick : StickBinding
{
    public override Stick? DefaultStick => Stick.Left;

    public override IReadOnlyList<Key?> DefaultKeys => new Key?[]{Key.D, Key.A, Key.W, Key.S};

    public override IReadOnlyList<GamepadButton?> DefaultButtons => new GamepadButton?[]{null, null, null, null};

    public override IReadOnlyList<MouseButton?> DefaultMouseButtons => new MouseButton?[]{null, null, null, null};

    public override int? InputType => KeyInput.PlayerInputType;

    public override bool NormalisedAbove1 => true;

    public override bool HasAxisDeadZone => false;

    public override bool HasMagnitudeDeadZone => true;

    public override bool UsesYAxis => true;

    public override bool UsesXAxis => true;
}

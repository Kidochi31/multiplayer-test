using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public abstract class KeyBinding
{
    public abstract Key? DefaultKey {get; }
    public abstract GamepadButton? DefaultButton {get; }
    public abstract MouseButton? DefaultMouseButton {get; }
    public abstract int? InputType {get;}

    public Key? CurrentKey;
    public GamepadButton? CurrentButton;
    public MouseButton? CurrentMouseButton;

    public bool DownThisFrame = false;
    public bool Held = false;
    public bool ReleasedThisFrame = false;
}

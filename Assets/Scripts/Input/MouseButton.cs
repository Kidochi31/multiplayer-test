using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using static MouseButton;

public enum MouseButton
{
    Back,
    Forward,
    Left,
    Right,
    Middle
}

public static class MouseExtensions
{
    static MouseExtensions()
    {
        MouseButtonFunctions[Back] = (mouse) => mouse.backButton;
        MouseButtonFunctions[Forward] = (mouse) => mouse.forwardButton;
        MouseButtonFunctions[Left] = (mouse) => mouse.leftButton;
        MouseButtonFunctions[Right] = (mouse) => mouse.rightButton;
        MouseButtonFunctions[Middle] = (mouse) => mouse.middleButton;
    }
    public static Dictionary<MouseButton, Func<Mouse, ButtonControl>> MouseButtonFunctions = new();

    public static ButtonControl GetButton(this Mouse mouse, MouseButton button)
    {
        return MouseButtonFunctions[button](mouse);
    }
}

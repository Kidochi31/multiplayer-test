using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using static Stick;

public abstract class StickBinding
{
    public const int RIGHT_INDEX = 0;
    public const int LEFT_INDEX = 1;
    public const int UP_INDEX = 2;
    public const int DOWN_INDEX = 3;

    public static Vector2 GetIndexVector(int index)
    {
        switch (index)
        {
            case 0:
                return Vector2.right;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            case 3:
                return Vector2.down;
            default:
                return Vector2.zero;
        }
    }


    public abstract Stick? DefaultStick {get;}
    public abstract IReadOnlyList<Key?> DefaultKeys {get; }
    public abstract IReadOnlyList<GamepadButton?> DefaultButtons {get; }
    public abstract IReadOnlyList<MouseButton?> DefaultMouseButtons {get; }
    public abstract int? InputType {get;}
    public abstract bool NormalisedAbove1 {get;}
    public abstract bool HasAxisDeadZone {get;}
    public abstract bool HasMagnitudeDeadZone {get;}
    public abstract bool UsesYAxis {get;}
    public abstract bool UsesXAxis {get;}

    public Stick? CurrentStick;
    public IReadOnlyList<Key?> CurrentKeys;
    public IReadOnlyList<GamepadButton?> CurrentButtons;
    public IReadOnlyList<MouseButton?> CurrentMouseButtons;
    
    
    public float XDeadZone = 0.2f;
    public float YDeadZone = 0.2f;
    public float MagnitudeDeadZone = 0.2f;
    public Vector2 Value = new Vector2(0,0);


    public Vector2 ProcessRawVector(Vector2 input)
    {
        if (!UsesXAxis)
        {
            input.x = 0;
        }
        if (!UsesYAxis)
        {
            input.y = 0;
        }

        if(HasAxisDeadZone)
        {
            if(math.abs(input.x) < XDeadZone)
            {
                input.x = 0;
            }
            else
            {
                // rescale so that it covers the entire portion from 0 to 1
                // input.x - XDeadZone gives portion of x above deadzone
                // the total portion of x above dead zone is 1 - XDeadZone
                // so ratio is the actual input
                input.x = (math.abs(input.x) - XDeadZone) / (1 - XDeadZone) * math.sign(input.x);
            }
        }

        if(HasAxisDeadZone)
        {
            if(math.abs(input.y) < YDeadZone)
            {
                input.y = 0;
            }
            else
            {
                input.y = (math.abs(input.x) - YDeadZone) / (1 - YDeadZone) * math.sign(input.x);
            }
        }

        if (NormalisedAbove1)
        {
            if(input.magnitude > 1)
            {
                input = input.normalized;
            }
        }

        if(HasMagnitudeDeadZone)
        {
            if(input.magnitude < MagnitudeDeadZone)
            {
                input.x = 0;
                input.y = 0;
            }
            else
            {
                // rescale so that it covers the entire portion from 0 to 1
                float targetMagnitude = (input.magnitude - MagnitudeDeadZone) / (1 - MagnitudeDeadZone);
                input = input.normalized * targetMagnitude;
            }
        }
        return input;
    }
}

public enum Stick
{
    Right,
    Left
}

public static class StickExtensions
{
    static StickExtensions()
    {
        StickButtonFunctions[Right] = (gamepad) => gamepad.rightStick;
        StickButtonFunctions[Left] = (gamepad) => gamepad.leftStick;
    }
    public static Dictionary<Stick, Func<Gamepad, StickControl>> StickButtonFunctions = new();

    public static StickControl GetStick(this Gamepad gamepad, Stick stick)
    {
        return StickButtonFunctions[stick](gamepad);
    }
}


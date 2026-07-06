using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class KeyInput
{
    public static Dictionary<Type, KeyBinding> KeyBindings = new();
    public static Dictionary<int, List<KeyBinding>> InputTypeToKeyBindings = new();

    public static Dictionary<Type, StickBinding> StickBindings = new();
    public static Dictionary<int, List<StickBinding>> InputTypeToStickBindings = new();
    public static Dictionary<int, bool> InputTypesBlocked = new();

    public const int PlayerInputType = 0;
    public const int MenuInputType = 1;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitialiseMessage()
    {
        {
            // Get the base type
            Type baseType = typeof(KeyBinding);

            // Find all non-abstract types in the project assembly that derive from BaseSystem
            var derivedTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in derivedTypes)
            {
                // Dynamically instantiate the class (requires a parameterless constructor)
                KeyBinding instance = (KeyBinding)Activator.CreateInstance(type, true);
                instance.CurrentButton = instance.DefaultButton;
                instance.CurrentKey = instance.DefaultKey;
                instance.CurrentMouseButton = instance.DefaultMouseButton ;
                int? inputType = instance.InputType;

                // Add it to keybindings
                KeyBindings[type] = instance;
                // Add its type to the input types list
                if(inputType is not null)
                {
                    if (InputTypeToKeyBindings.ContainsKey(inputType.Value))
                    {
                        InputTypeToKeyBindings[inputType.Value].Add(instance);
                    }
                    else
                    {
                        InputTypeToKeyBindings.Add(inputType.Value, new(){instance});
                        if (!InputTypesBlocked.ContainsKey(inputType.Value))
                        {
                            InputTypesBlocked.Add(inputType.Value, false);
                        }
                    }
                }
            }
        }


        {
            // Get the base type
            Type baseType = typeof(StickBinding);

            // Find all non-abstract types in the project assembly that derive from BaseSystem
            var derivedTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in derivedTypes)
            {
                // Dynamically instantiate the class (requires a parameterless constructor)
                StickBinding instance = (StickBinding)Activator.CreateInstance(type, true);
                instance.CurrentStick = instance.DefaultStick;
                instance.CurrentKeys = instance.DefaultKeys;
                instance.CurrentButtons = instance.DefaultButtons;
                instance.CurrentMouseButtons = instance.DefaultMouseButtons;
                int? inputType = instance.InputType;

                // Add it to keybindings
                StickBindings[type] = instance;
                // Add its type to the input types list
                if(inputType is not null)
                {
                    if (InputTypeToStickBindings.ContainsKey(inputType.Value))
                    {
                        InputTypeToStickBindings[inputType.Value].Add(instance);
                    }
                    else
                    {
                        InputTypeToStickBindings.Add(inputType.Value, new(){instance});
                        if (!InputTypesBlocked.ContainsKey(inputType.Value))
                        {
                            InputTypesBlocked.Add(inputType.Value, false);
                        }
                    }
                }
            }
        }
    }

    public static void Update()
    {
        foreach(KeyBinding binding in KeyBindings.Values)
        {
            bool blocked = binding.InputType is null ? false : InputTypesBlocked[binding.InputType.Value];
            bool pressed = false;
            Key? key = binding.CurrentKey;
            GamepadButton? button = binding.CurrentButton;
            MouseButton? mouseButton = binding.CurrentMouseButton;

            Keyboard? keyboard = Keyboard.current;
            if(keyboard is not null && key is not null && !blocked)
            {
                pressed = keyboard[key.Value].isPressed ? true : pressed;
            }
            Gamepad? gamepad = Gamepad.current;
            if(gamepad is not null && button is not null && !blocked)
            {
                pressed = gamepad[button.Value].isPressed ? true : pressed;
            }
            Mouse? mouse = Mouse.current;
            if(mouse is not null && mouseButton is not null && !blocked)
            {
                pressed = mouse.GetButton(mouseButton.Value).isPressed ? true : pressed;
            }
            binding.DownThisFrame = pressed && !binding.Held;
            binding.ReleasedThisFrame = !pressed && binding.Held;
            binding.Held = pressed;
        }

        foreach(StickBinding binding in StickBindings.Values)
        {
            bool blocked = binding.InputType is null ? false : InputTypesBlocked[binding.InputType.Value];
            Vector2? stickValue = null;
            Vector2? mouseValue = null;
            bool[] inputs = new bool[4];

            Stick? stick = binding.CurrentStick;
            Gamepad? gamepad = Gamepad.current;
            if(gamepad is not null && stick is not null && !blocked)
            {
                Vector2 rawValue = gamepad.GetStick(stick.Value).value;
                stickValue = binding.ProcessRawVector(rawValue);
            }

            Keyboard? keyboard = Keyboard.current;
            if(keyboard is not null && !blocked)
            {
                for(int i = 0; i < 4; i++)
                {
                    Key? key = binding.CurrentKeys[i];
                    if(key is not null && keyboard[key.Value].isPressed)
                    {
                        inputs[i] = true;
                    }
                }
            }

            if(gamepad is not null && !blocked)
            {
                for(int i = 0; i < 4; i++)
                {
                    GamepadButton? button = binding.CurrentButtons[i];
                    if(button is not null && gamepad[button.Value].isPressed)
                    {
                        inputs[i] = true;
                    }
                }
            }

            Mouse? mouse = Mouse.current;
            if(mouse is not null && !blocked)
            {
                for(int i = 0; i < 4; i++)
                {
                    MouseButton? button = binding.CurrentMouseButtons[i];
                    if(button is not null && mouse.GetButton(button.Value).isPressed)
                    {
                        inputs[i] = true;
                    }
                }

                if (binding.UsesMouse)
                {
                    mouseValue = mouse.delta.value / Time.deltaTime * binding.MouseSensitivity * StickBinding.BASE_MOUSE_SENSITIVITY;
                }
            }
            
            Vector2 value = new(0,0);
            // now need to interpret button results
            for(int i = 0; i < 4; i++)
            {
                if (inputs[i])
                {
                    value += StickBinding.GetIndexVector(i);
                }
            }
            

            // now need to interpret stick results
            if(stickValue is not null)
            {
                float xComponent = stickValue.Value.x;
                if(value.x == 0)
                {
                    value.x = xComponent;
                }
                float yComponent = stickValue.Value.y;
                if(value.y == 0)
                {
                    value.y = yComponent;
                }
            }

            // now need to interpret mouse results
            if(mouseValue is not null)
            {
                float xComponent = mouseValue.Value.x;
                if(value.x == 0)
                {
                    value.x = xComponent;
                }
                float yComponent = mouseValue.Value.y;
                if(value.y == 0)
                {
                    value.y = yComponent;
                }
            }

            binding.Value = binding.ProcessRawVector(value);
        }
    }

    public static void DisableInputs(int inputType)
    {
        if (InputTypesBlocked.ContainsKey(inputType))
        {
            InputTypesBlocked[inputType] = true;
        }
    }

    public static void EnableInputs(int inputType)
    {
        if (InputTypesBlocked.ContainsKey(inputType))
        {
            InputTypesBlocked[inputType] = false;
        }
    }


    public static bool AttemptUpdateKey(KeyBinding binding)
    {
        Keyboard? keyboard = Keyboard.current;
        if(keyboard is null)
        {
            return false;
        }
        foreach(KeyControl key in keyboard.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                binding.CurrentKey = key.keyCode;
                return true;
            }
        }
        return false;
    }

    public static bool AttemptUpdateStickKey(StickBinding binding, int direction)
    {
        Keyboard? keyboard = Keyboard.current;
        if(keyboard is null)
        {
            return false;
        }
        foreach(KeyControl key in keyboard.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                Key?[] keys = binding.CurrentKeys.ToArray();
                keys[direction] = key.keyCode;
                binding.CurrentKeys = keys;
                return true;
            }
        }
        return false;
    }

    public static bool AttemptUpdateButton(KeyBinding binding)
    {
        Gamepad? gamepad = Gamepad.current;
        if(gamepad is null)
        {
            return false;
        }
        foreach(GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
        {
            if (gamepad[button].wasPressedThisFrame)
            {
                binding.CurrentButton = button;
                return true;
            }
        }
        return false;
    }

    public static bool AttemptUpdateStickButton(StickBinding binding, int direction)
    {
        Gamepad? gamepad = Gamepad.current;
        if(gamepad is null)
        {
            return false;
        }
        foreach(GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
        {
            if (gamepad[button].wasPressedThisFrame)
            {
                GamepadButton?[] buttons = binding.CurrentButtons.ToArray();
                buttons[direction] = button;
                binding.CurrentButtons = buttons;
                return true;
            }
        }
        return false;
    }

    public static bool AttemptUpdateMouseButton(KeyBinding binding)
    {
        Mouse? mouse = Mouse.current;
        if(mouse is null)
        {
            return false;
        }
        foreach(MouseButton button in Enum.GetValues(typeof(MouseButton)))
        {
            if (mouse.GetButton(button).wasPressedThisFrame)
            {
                binding.CurrentMouseButton = button;
                return true;
            }
        }
        return false;
    }

    public static bool AttemptUpdateStickMouseButton(StickBinding binding, int direction)
    {
        Mouse? mouse = Mouse.current;
        if(mouse is null)
        {
            return false;
        }
        foreach(MouseButton button in Enum.GetValues(typeof(MouseButton)))
        {
            if (mouse.GetButton(button).wasPressedThisFrame)
            {
                MouseButton?[] buttons = binding.CurrentMouseButtons.ToArray();
                buttons[direction] = button;
                binding.CurrentMouseButtons = buttons;
                return true;
            }
        }
        return false;
    }

    public static KeyBinding GetKeyBinding<T>() where T : KeyBinding, new()
    {
        return KeyBindings[typeof(T)];
    }

    public static StickBinding GetStickBinding<T>() where T : StickBinding, new()
    {
        return StickBindings[typeof(T)];
    }
}

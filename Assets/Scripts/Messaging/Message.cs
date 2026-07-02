using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class Message
{
    // messages are used to send information between the client and server (and in extension, other clients)

    private delegate Message? InterpretMessagePayloadDelegate(ReadOnlySpan<byte> payload);
    static Dictionary<byte, InterpretMessagePayloadDelegate> Interpreters = new();
    protected abstract Message? InterpretMessagePayload(ReadOnlySpan<byte> payload); 
    protected abstract byte MessageType {get;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitialiseMessage()
    {
        // Get the base type
        Type baseType = typeof(Message);

        // Find all non-abstract types in the project assembly that derive from BaseSystem
        var derivedTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in derivedTypes)
        {
            // Dynamically instantiate the class (requires a parameterless constructor)
            Message instance = (Message)Activator.CreateInstance(type, true);
            
            // Load the interpreter
            Interpreters[instance.MessageType] = instance.InterpretMessagePayload;
        }
    }

    public static Message? InterpretPacket(ReadOnlySpan<byte> Packet)
    {
        if(Packet.Length == 0)
        {
            return null;
        }
        byte messageType = Packet[0];
        if (!Interpreters.ContainsKey(messageType))
        {
            return null;
        }
        InterpretMessagePayloadDelegate interpreter = Interpreters[messageType];
        return interpreter(Packet[1..]);
    }
}

public abstract class ReliableMessage : Message
{
}

public abstract class UnreliableMessage : Message
{
}

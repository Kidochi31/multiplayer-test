using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Relunrel.Connections;
using UnityEngine;

public abstract class Message
{
    // messages are used to send information between the client and server (and in extension, other clients)

    private delegate Message? InterpretMessagePayloadDelegate(ReadOnlySpan<byte> payload);
    static Dictionary<byte, InterpretMessagePayloadDelegate> Interpreters = new();
    protected abstract Message? InterpretMessagePayload(ReadOnlySpan<byte> payload); 
    protected abstract bool CreateMessagePayload(ref Span<byte> data);
    protected abstract byte MessageType {get;}
    protected abstract int GetMessagePayloadLength();

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
            if (Interpreters.ContainsKey(instance.MessageType))
            {
                Debug.LogError($"DUPLICATE MESSAGE TYPES: {instance.MessageType}");
            }
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

    public static byte[]? CreateMessage(Message message)
    {
        byte[] messageBytes = new byte[1 + message.GetMessagePayloadLength()];
        messageBytes[0] = message.MessageType;
        Span<byte> payload = messageBytes.AsSpan(1);
        if(!message.CreateMessagePayload(ref payload)) return null;
        return messageBytes;
    }

    public static bool SendMessage(Connection connection, Message message, DateTime time)
    {
        if(message is ReliableMessage)
        {
            byte[]? messageBytes = Message.CreateMessage(message);
            if(messageBytes is null)
            {
                return false;
            }
            connection.SendReliableOrdered(messageBytes, time);
            return true;
        }
        else if(message is UnreliableMessage)
        {
            byte[]? messageBytes = Message.CreateMessage(message);
            if(messageBytes is null)
            {
                return false;
            }
            connection.SendUnreliableOrdered(messageBytes);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static ReliableMessage? InterpretReliableOrderedPacket(Connection connection)
    {
        byte[]? data = connection.DequeueReliableOrderedMessage();
        if(data is null)
        {
            return null;
        }
        Message? message = InterpretPacket(data);
        return message is ReliableMessage ? (ReliableMessage)message : null;
    }

    public static UnreliableMessage? InterpretUnreliableOrderedPacket(Connection connection)
    {
        byte[]? data = connection.DequeueUnreliableOrderedMessage();
        if(data is null)
        {
            return null;
        }
        Message? message = InterpretPacket(data);
        return message is UnreliableMessage ? (UnreliableMessage)message : null;
    }
}

public abstract class ReliableMessage : Message
{
}

public abstract class UnreliableMessage : Message
{
}

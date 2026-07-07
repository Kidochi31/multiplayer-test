using System;
using UnityEngine;

public class SendChatMessage : ReliableMessage
{
    // Client -> Server
    // Sends the chat message to the server
    public string Message;
    public SendChatMessage(string message)
    {
        Message = message;
    }
    private SendChatMessage(){}
    protected override byte MessageType => 20;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteString(ref data, Message)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthString(Message);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        string? message = MessageFields.ReadString(ref payload);
        if(message is null) return null;
        return new SendChatMessage(message);
    }
}

public class ChatMessage : ReliableMessage
{
    // Server -> Client
    // Sends a chat message that was sent to the server
    public ushort ClientId;
    public string Message;
    public ChatMessage(ushort clientId, string message)
    {
        ClientId = clientId;
        Message = message;
    }
    private ChatMessage(){}
    protected override byte MessageType => 21;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        if(!MessageFields.WriteString(ref data, Message)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthUInt16() + MessageFields.LengthString(Message);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        string? message = MessageFields.ReadString(ref payload);
        if(message is null) return null;
        return new ChatMessage(clientId.Value, message);
    }
}
public class ServerMessage : ReliableMessage
{
    // Server -> Client
    // Sends a server message to the client
    public string Message;
    public ServerMessage(string message)
    {
        Message = message;
    }
    private ServerMessage(){}
    protected override byte MessageType => 24;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteString(ref data, Message)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthString(Message);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        string? message = MessageFields.ReadString(ref payload);
        if(message is null) return null;
        return new ServerMessage(message);
    }
}

public class SendAudioMessage : UnreliableMessage
{
    // Client -> Server
    // Sends the audio message to the server
    public byte[] Message;
    public int SequenceNumber;
    public SendAudioMessage(byte[] message, int seq)
    {
        Message = message;
        SequenceNumber = seq;
    }
    private SendAudioMessage(){}
    protected override byte MessageType => 22;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteByteArray(ref data, Message)) return false;
        if(!MessageFields.WriteInt32(ref data, SequenceNumber)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthByteArray(Message) + MessageFields.LengthInt32(SequenceNumber);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        byte[]? message = MessageFields.ReadByteArray(ref payload);
        if(message is null) return null;
        int? seq = MessageFields.ReadInt32(ref payload);
        if(message is null) return null;
        return new SendAudioMessage(message, seq.Value);
    }
}

public class AudioMessage : UnreliableMessage
{
    // Server -> Client
    // Sends an audio message that was sent to the server
    public ushort ClientId;
    public byte[] Message;
    public int Seq;
    public AudioMessage(ushort clientId, byte[] message, int seq)
    {
        ClientId = clientId;
        Message = message;
        Seq = seq;
    }
    private AudioMessage(){}
    protected override byte MessageType => 23;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        if(!MessageFields.WriteByteArray(ref data, Message)) return false;
        if(!MessageFields.WriteInt32(ref data, Seq)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthUInt16() + MessageFields.LengthByteArray(Message) + MessageFields.LengthInt32();
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        byte[]? message = MessageFields.ReadByteArray(ref payload);
        if(message is null) return null;
        int? seq = MessageFields.ReadInt32(ref payload);
        if(message is null) return null;
        return new AudioMessage(clientId.Value, message, seq.Value);
    }
}
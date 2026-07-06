using System;
using UnityEngine;

public class SendPositionMessage : UnreliableMessage
{
    // Client -> Server
    public Vector3 Position;
    private SendPositionMessage(){}
    public SendPositionMessage(Vector3 position)
    {
        Position = position;
    }
    protected override byte MessageType => 30;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteVector3(ref data, Position)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthVector3(Position);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        Vector3? position = MessageFields.ReadVector3(ref payload);
        if(position is null) return null;
        return new SendPositionMessage(position.Value);
    }
}

public class PositionMessage : UnreliableMessage
{
    // Server -> Client
    public ushort ClientId;
    public Vector3 Position;

    private PositionMessage(){}

    public PositionMessage(ushort clientId, Vector3 position)
    {
        ClientId = clientId;
        Position = position;
    }
    protected override byte MessageType => 31;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        if(!MessageFields.WriteVector3(ref data, Position)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthVector3(Position) + MessageFields.LengthUInt16(ClientId);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        Vector3? position = MessageFields.ReadVector3(ref payload);
        if(position is null) return null;
        return new PositionMessage(clientId.Value, position.Value);
    }
}
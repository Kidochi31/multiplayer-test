using System;
using UnityEngine;

public class JoinRequest : ReliableMessage
{
    // Client -> Server
    // Sent to request to join the game
    public string Version;
    public string Name;
    public Guid PreviousGuid;
    public JoinRequest(string version, string name, Guid previousGuid)
    {
        Version = version;
        Name = name;
        PreviousGuid = previousGuid;
    }
    private JoinRequest(){}
    protected override byte MessageType => 0;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteString(ref data, Version)) return false;
        if(!MessageFields.WriteString(ref data, Name)) return false;
        if(!MessageFields.WriteGuid(ref data, PreviousGuid)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthString(Version) + MessageFields.LengthString(Name) + MessageFields.LengthGuid();
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        string? version = MessageFields.ReadString(ref payload);
        if(version is null) return null;
        string? name = MessageFields.ReadString(ref payload);
        if(name is null) return null;
        Guid? previousGuid = MessageFields.ReadGuid(ref payload);
        if(previousGuid is null) return null;
        return new JoinRequest(version, name, previousGuid.Value);
    }
}

public class JoinResponse : ReliableMessage
{
    // Server -> Client
    // Sent to accept client joining game (disconnect may be sent instead of setting accept to false)
    public bool Accepted;
    public Guid Guid;
    public ushort ClientId;
    public JoinResponse(bool accepted, Guid guid, ushort clientId)
    {
        Accepted = accepted;
        Guid = guid;
        ClientId = clientId;
    }
    private JoinResponse(){}
    protected override byte MessageType => 1;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteBool(ref data, Accepted)) return false;
        if(!MessageFields.WriteGuid(ref data, Guid)) return false;
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        return true;
    }
    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthBool() + MessageFields.LengthGuid() + MessageFields.LengthUInt16();
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        bool? accepted = MessageFields.ReadBool(ref payload);
        if(accepted is null) return null;
        Guid? guid = MessageFields.ReadGuid(ref payload);
        if(guid is null) return null;
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        return new JoinResponse(accepted.Value, guid.Value, clientId.Value);
    }
}

public class DisconnectMessage : ReliableMessage
{
    // Server -> Client
    // Sent to inform client that the server will imminently disconnect them
    public string Message;
    public DisconnectMessage(string message)
    {
        Message = message;
    }
    private DisconnectMessage(){}
    protected override byte MessageType => 2;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteString(ref data, Message)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthString(Message) ;
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        string? message = MessageFields.ReadString(ref payload);
        if(message is null) return null;
        return new DisconnectMessage(message);
    }
}

public class ClientInfoMessage : ReliableMessage
{
    // Server -> Client
    // Sent to inform client of a player's info
    public ushort ClientId;
    public string Name;
    public ClientInfoMessage(ushort clientId, string name)
    {
        ClientId = clientId;
        Name = name;
    }
    private ClientInfoMessage(){}
    protected override byte MessageType => 10;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        if(!MessageFields.WriteString(ref data, Name)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthUInt16(ClientId) + MessageFields.LengthString(Name);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        string? name = MessageFields.ReadString(ref payload);
        if(name is null) return null;
        return new ClientInfoMessage(clientId.Value, name);
    }
}

public class ClientJoinMessage : ReliableMessage
{
    // Server -> Client
    // Sent to inform client that a player has just joined
    public ushort ClientId;
    public string Name;
    public ClientJoinMessage(ushort clientId, string name)
    {
        ClientId = clientId;
        Name = name;
    }
    private ClientJoinMessage(){}
    protected override byte MessageType => 11;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        if(!MessageFields.WriteString(ref data, Name)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthUInt16(ClientId) + MessageFields.LengthString(Name);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        string? name = MessageFields.ReadString(ref payload);
        if(name is null) return null;
        return new ClientJoinMessage(clientId.Value, name);
    }
}

public class ClientLeaveMessage : ReliableMessage
{
    // Server -> Client
    // Sent to inform client that a player has just left
    public ushort ClientId;
    public ClientLeaveMessage(ushort clientId)
    {
        ClientId = clientId;
    }
    private ClientLeaveMessage(){}
    protected override byte MessageType => 12;

    protected override bool CreateMessagePayload(ref Span<byte> data)
    {
        if(!MessageFields.WriteUInt16(ref data, ClientId)) return false;
        return true;
    }

    protected override int GetMessagePayloadLength()
    {
        return MessageFields.LengthUInt16(ClientId);
    }

    protected override Message? InterpretMessagePayload(ReadOnlySpan<byte> payload)
    {
        ushort? clientId = MessageFields.ReadUInt16(ref payload);
        if(clientId is null) return null;
        return new ClientLeaveMessage(clientId.Value);
    }
}
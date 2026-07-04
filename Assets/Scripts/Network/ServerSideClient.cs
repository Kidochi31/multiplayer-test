using System;
using System.Collections.Generic;
using System.Net;
using Relunrel.Connections;
using UnityEngine;

public class ServerSideClient
{
    public Connection Connection;
    public IPEndPoint IPEndPoint;
    public Guid Guid;
    public ushort ClientId;
    public string Name;

    public List<ReliableMessage> RecentReliableMessages = new();

    public List<UnreliableMessage> RecentUnreliableMessages = new();

    public ServerSideClient(Connection connection, IPEndPoint iPEndPoint, Guid guid, ushort clientId, string name)
    {
        Connection = connection;
        IPEndPoint = iPEndPoint;
        Guid = guid;
        ClientId = clientId;
        Name = name;
    }
}

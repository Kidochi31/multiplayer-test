using UnityEngine;
using Relunrel.Network;
using System;
using System.Collections.Generic;
using Relunrel.Connections;
using System.Net;
using System.Linq;

// This script will execute before default scripts
[DefaultExecutionOrder(-100)]
public class ServerNetwork : MonoBehaviour
{
    public RSocket Socket;
    public bool IsOpenToLan {get; private set;} = false;
    public Relanrel.Server? LanServer {get; private set;} = null;

    public Dictionary<ushort, ServerSideClient> IdToClient = new();
    public Dictionary<IPEndPoint, ServerSideClient> EndPointToClient = new();
    public List<ServerSideClient> CurrentClients = new();

    public List<ServerSideClient> NewClients = new();
    public List<ServerSideClient> DeadClients = new();

    public int ResponseWaitSeconds = 5;


    private Dictionary<IPEndPoint, DateTime> PendingConnections = new(); 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Socket = new RSocket();
        Socket.BeginStun(NetworkHelper.StunHosts);
        Socket.EnableListening();
    }

    // Update is called once per frame
    void Update()
    {
        TickSocket(DateTime.UtcNow);
        TickPendingConnections(DateTime.UtcNow);
        TickMessages(DateTime.UtcNow);
        TickLan();



        


        // (var newC, var deadC) = Socket.Tick(DateTime.UtcNow);
        // NewConnections.Clear();
        // DeadConnections.Clear();
        // NewConnections.AddRange(newC);
        // DeadConnections.AddRange(deadC);
        // CurrentConnections.AddRange(newC);
        // foreach(var c in deadC)
        // {
        //     CurrentConnections.Remove(c);
        // }
    }

    private void TickSocket(DateTime time)
    {
        NewClients.Clear();
        DeadClients.Clear();
        (var newC, var deadC) = Socket.Tick(time);
        foreach(var c in newC)
        {
            PendingConnections[c] = time + TimeSpan.FromSeconds(ResponseWaitSeconds);
        }
        foreach(var c in deadC)
        {
            PendingConnections.Remove(c);
            if (EndPointToClient.ContainsKey(c))
            {
                ServerSideClient client = EndPointToClient[c];
                ushort id = client.ClientId;
                CurrentClients.Remove(client);
                IdToClient.Remove(id);
                EndPointToClient.Remove(c);
                SendDeadClient(client, time);
                DeadClients.Add(client);
            }
        }
    }

    private void TickMessages(DateTime time)
    {
        // go through all connections and get the reliable and unreliable unordered messages
        foreach(ServerSideClient client in CurrentClients)
        {
            client.RecentReliableMessages.Clear();
            client.RecentUnreliableMessages.Clear();
            Connection connection = client.Connection;
            while (connection.ReliableOrderedMessagesAvailable)
            {
                // go through all messages
                ReliableMessage? message = Message.InterpretReliableOrderedPacket(connection);
                if(message is null)
                {
                    continue;
                }
                // manage message
                client.RecentReliableMessages.Add(message);
            }

            while (connection.UnreliableOrderedMessagesAvailable)
            {
                // go through all messages
                UnreliableMessage? message = Message.InterpretUnreliableOrderedPacket(connection);
                if(message is null)
                {
                    continue;
                }
                // manage message
                client.RecentUnreliableMessages.Add(message);
            }
        }
    }

    private void TickPendingConnections(DateTime time)
    {
        foreach((IPEndPoint ep, DateTime timeout) in PendingConnections.ToList())
        {
            Connection connection = Socket.Connections[ep];
            while (connection.ReliableOrderedMessagesAvailable)
            {
                // go through all messages
                Message? message = Message.InterpretReliableOrderedPacket(connection);
                if(message is null)
                {
                    continue;
                }
                if(message is JoinRequest)
                {
                    // manage the message
                    AcceptJoinRequest((JoinRequest)message, ep, connection, time);
                }
            }

            if(timeout <= time)
            {
                // timeout -> disconnect
                DisconnectMessage message = new DisconnectMessage("Connection Timeout");
                Message.SendMessage(connection, message, time);
                // remove from pending connections
                PendingConnections.Remove(ep);
            }
        }
    }

    private void AcceptJoinRequest(JoinRequest request, IPEndPoint ep, Connection connection, DateTime time)
    {
        ushort ClientId = GetNextClientId();
        JoinResponse response = new JoinResponse(true, Guid.NewGuid(), ClientId);
        // if the request has a guid -> reuse it
        Guid guid = request.PreviousGuid;
        if(guid != Guid.Empty)
        {
            response.Guid = guid;
        }
        // send join response
        Message.SendMessage(connection, response, time);
        // send client info
        SendClientInfo(connection, time);

        // add client to current clients
        ServerSideClient client = new ServerSideClient(connection, ep, response.Guid, ClientId, request.Name);
        CurrentClients.Add(client);
        IdToClient[ClientId] = client;
        EndPointToClient[ep] = client;
        NewClients.Add(client);
        SendNewClient(client, time);
        PendingConnections.Remove(ep);
    }

    private void SendClientInfo(Connection connection, DateTime time)
    {
        foreach(ServerSideClient client in CurrentClients)
        {
            ClientInfoMessage message = new ClientInfoMessage(client.ClientId, client.Name);
            Message.SendMessage(connection, message, time);
        }
    }

    private void SendNewClient(ServerSideClient client, DateTime time)
    {
        ClientJoinMessage message = new ClientJoinMessage(client.ClientId, client.Name);
        foreach(ServerSideClient c in CurrentClients)
        {
            if (!c.Equals(client))
            {
                Message.SendMessage(c.Connection, message, time);
            }
        }
    }

    private void SendDeadClient(ServerSideClient client, DateTime time)
    {
        ClientLeaveMessage message = new ClientLeaveMessage(client.ClientId);
        foreach(ServerSideClient c in CurrentClients)
        {
            if (!c.Equals(client))
            {
                Message.SendMessage(c.Connection, message, time);
            }
        }
    }

    private ushort GetNextClientId()
    {
        for(int i = 0; i <= ushort.MaxValue; i++)
        {
            if (!IdToClient.ContainsKey((ushort)i))
            {
                return (ushort)i;
            }
        }
        return 0;
    }

    private void TickLan()
    {
        LanServer?.Tick();
    }

    public void CloseLan()
    {
        IsOpenToLan = false;
        LanServer = null;

    }

    public void OpenToLan(string info)
    {
        LanServer = Relanrel.Server.CreateServer(6767, (ushort)Socket.InternalEndPoint.Port, 0x67676767, info);
        IsOpenToLan = true;
    }
}

using UnityEngine;
using Relunrel.Network;
using System;
using Relunrel.Connections;
using System.Collections.Generic;
using System.Net;

// This script will execute before default scripts
[DefaultExecutionOrder(-100)]
public class ClientNetwork : MonoBehaviour
{
    public RSocket Socket;
    public string Username;
    public ushort ClientId;
    public Guid ClientGuid;
    public ClientSideClient ThisClient;
    private Connection? CurrentConnection;
    public ClientState State = ClientState.Idle; 
    public List<ClientSideClient> CurrentClients = new();
    public List<ClientSideClient> NewClients = new();
    public List<ClientSideClient> NewClientInfo = new();
    public List<ClientSideClient> DeadClients = new();
    public Dictionary<ushort, ClientSideClient> IdToClient = new();
    public List<ReliableMessage> RecentReliableMessages = new();
    public List<UnreliableMessage> RecentUnreliableMessages = new();

    private int JoinResponseTimeoutSeconds = 10;
    private DateTime JoinResponseTimeout;

    public void ConnectTo(IPEndPoint ep, DateTime time)
    {
        CurrentConnection = Socket.ConnectTo(ep, time);
    }

    public void SendMessage(Message message, DateTime time)
    {
        if(CurrentConnection is null)
        {
            return;
        }
        Message.SendMessage(CurrentConnection, message, time);
    }

    public void Disconnect(DateTime time)
    {
        CurrentConnection?.Disconnect(time);
    }

    public void Reset()
    {
        CurrentConnection = null;
        State = ClientState.Idle;
        CurrentClients.Clear();
        NewClients.Clear();
        DeadClients.Clear();
        IdToClient.Clear();
        NewClientInfo.Clear();
        RecentReliableMessages.Clear();
        RecentUnreliableMessages.Clear();

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Socket = new RSocket();
        if(NetworkHelper.StunHosts.Count > 0)
        {
            Socket.BeginStun(NetworkHelper.StunHosts);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Socket.Tick(DateTime.UtcNow);
        RecentReliableMessages.Clear();
        RecentUnreliableMessages.Clear();
        NewClients.Clear();
        NewClientInfo.Clear();
        foreach(ClientSideClient deadClient in DeadClients)
        {
            IdToClient.Remove(deadClient.ClientId);
        }
        DeadClients.Clear();
        if(CurrentConnection is not null && CurrentConnection.State == ConnectionState.Connected)
        {
            switch (State)
            {
                case ClientState.Idle:
                    {
                        // now connected -> send join request
                        JoinRequest request = new JoinRequest("GAME", Username, Guid.Empty);
                        Message.SendMessage(CurrentConnection, request, DateTime.UtcNow);
                        State = ClientState.ResponseWait;
                        JoinResponseTimeout = DateTime.UtcNow + TimeSpan.FromSeconds(JoinResponseTimeoutSeconds);
                        break;
                    }
                case ClientState.ResponseWait:
                    {
                        ResponseWaitUpdate(CurrentConnection, DateTime.UtcNow);
                        break;
                    }
                case ClientState.Connected:
                    {
                        ConnectedUpdate(CurrentConnection);
                        break;
                    }
            }
        }
        else if(CurrentConnection is not null && CurrentConnection.State == ConnectionState.Disconnected)
        {
            State = ClientState.Disconnected;
        }
        else if(CurrentConnection is not null && CurrentConnection.State == ConnectionState.FinWaitActive)
        {
            State = ClientState.ActiveDisconnecting;
        }
        else if(CurrentConnection is not null && CurrentConnection.State == ConnectionState.FinWaitPassive)
        {
            State = ClientState.PassiveDisconnecting;
        }
    }

    void ResponseWaitUpdate(Connection connection, DateTime time)
    {
        while (connection.ReliableOrderedMessagesAvailable)
        {
            ReliableMessage? message = Message.InterpretReliableOrderedPacket(connection);
            if(message is null) continue;
            if(message is JoinResponse response)
            {
                if (!response.Accepted)
                {
                    // if not accepted -> disconnect!
                    connection.Disconnect(time);
                    return;
                }
                ClientId = response.ClientId;
                ClientGuid = response.Guid;
                State = ClientState.Connected;
                break;
            }
        }

        if(time >= JoinResponseTimeout)
        {
            // Timed out -> disconnect
            connection.Disconnect(time);
        }
    }

    void ConnectedUpdate(Connection connection)
    {
        while (connection.ReliableOrderedMessagesAvailable)
        {
            // go through all messages
            ReliableMessage? message = Message.InterpretReliableOrderedPacket(connection);
            if(message is null)
            {
                continue;
            }
            // manage message
            RecentReliableMessages.Add(message);

            // need to manage some special message types: ClientInfo, ClientJoin, ClientLeave
            if(message is ClientInfoMessage clientInfo)
            {
                ClientSideClient client = new ClientSideClient(clientInfo.ClientId, clientInfo.Name);
                CurrentClients.Add(client);
                IdToClient[clientInfo.ClientId] = client;
                if(clientInfo.ClientId == ClientId)
                {
                    ThisClient = client;
                }
                NewClientInfo.Add(client);
            }
            if(message is ClientJoinMessage clientJoin)
            {
                ClientSideClient client = new ClientSideClient(clientJoin.ClientId, clientJoin.Name);
                CurrentClients.Add(client);
                IdToClient[clientJoin.ClientId] = client;
                NewClients.Add(client);
            }
            if(message is ClientLeaveMessage clientLeave)
            {
                ClientSideClient client = IdToClient[clientLeave.ClientId];
                CurrentClients.Remove(client);
                NewClients.Remove(client);
                DeadClients.Add(client);
            }
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
            RecentUnreliableMessages.Add(message);
        }
    }

    
}

public enum ClientState
    {
        Idle,
        ResponseWait,
        Connected,
        ActiveDisconnecting,
        PassiveDisconnecting,
        Disconnected
    }

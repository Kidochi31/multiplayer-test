using UnityEngine;
using Relunrel.Network;
using System;
using System.Collections.Generic;
using Relunrel.Connections;
using System.Net;

public class ServerNetwork : MonoBehaviour
{
    public RSocket Socket;
    public List<IPEndPoint> NewConnections = new();
    public List<IPEndPoint> DeadConnections = new();
    public List<IPEndPoint> CurrentConnections = new();

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
        (var newC, var deadC) = Socket.Tick(DateTime.UtcNow);
        NewConnections.Clear();
        DeadConnections.Clear();
        NewConnections.AddRange(newC);
        DeadConnections.AddRange(deadC);
        CurrentConnections.AddRange(newC);
        foreach(var c in deadC)
        {
            CurrentConnections.Remove(c);
        }

        foreach(var c in CurrentConnections)
        {
            Debug.Log(Socket.Connections[c].State);
        }
    }
}

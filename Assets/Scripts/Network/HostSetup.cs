using System;
using System.Net;
using UnityEngine;

public class HostSetup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // if there is both a client and a server need to connect them
        ClientNetwork? client = FindAnyObjectByType<ClientNetwork>();
        ServerNetwork? server = FindAnyObjectByType<ServerNetwork>();
        if(client != null && server != null && client.CurrentConnection is null)
        {
            IPEndPoint target = new IPEndPoint(IPAddress.Loopback, server.Socket.InternalEndPoint.Port);
            client.CurrentConnection = client.Socket.ConnectTo(target, DateTime.UtcNow);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

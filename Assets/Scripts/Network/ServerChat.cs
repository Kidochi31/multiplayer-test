using System;
using System.Net;
using Relunrel.Connections;
using UnityEngine;

public class ServerChat : MonoBehaviour
{
    private ServerNetwork? Server;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Server = FindAnyObjectByType<ServerNetwork>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(IPEndPoint endpoint in Server!.CurrentConnections)
        {
            Connection connection = Server!.Socket.Connections[endpoint];
            while (connection.ReliableOrderedMessagesAvailable)
            {
                // go through all messages and send them to all other clients
                byte[] message = connection.DequeueReliableOrderedMessage();
                foreach(IPEndPoint target in Server!.CurrentConnections)
                {
                    if (!target.Equals(endpoint))
                    {
                        Connection targetConnection = Server!.Socket.Connections[target];
                        targetConnection.SendReliableOrdered(message, DateTime.UtcNow);
                    }
                }
            }
        }
    }
}

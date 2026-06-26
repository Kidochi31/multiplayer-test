using System;
using System.Net;
using System.Text;
using Relunrel.Connections;
using UnityEngine;

public class ServerChat : MonoBehaviour
{
    private ServerNetwork? Server;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Server = FindAnyObjectByType<ServerNetwork>();
    }

    // Update is called once per frame
    void Update()
    {
        // notify of any people leaving or joining
        foreach(IPEndPoint endpoint in Server!.CurrentConnections)
        {
            Connection connection = Server!.Socket.Connections[endpoint];
            // send people leaving
            {
                foreach(IPEndPoint deadC in Server!.DeadConnections)
                {
                    string message = $"<server>: {deadC} has left.";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    connection.SendReliableOrdered(messageBytes, DateTime.UtcNow);
                }
            }
            // send people joining
            {
                foreach(IPEndPoint newC in Server!.NewConnections)
                {
                    string message = $"<server>: {newC} has joined.";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    connection.SendReliableOrdered(messageBytes, DateTime.UtcNow);
                }
            }
            
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

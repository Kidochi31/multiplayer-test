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
        foreach(ServerSideClient endpoint in Server!.CurrentClients)
        {
            // send people leaving
            {
                foreach(ServerSideClient deadC in Server!.DeadClients)
                {
                    string message = $"{deadC.Name} has left.";
                    ServerMessage serverMessage = new ServerMessage(message);
                    Message.SendMessage(endpoint.Connection, serverMessage, DateTime.UtcNow);
                }
            }
            // send people joining
            {
                foreach(ServerSideClient deadC in Server!.NewClients)
                {
                    string message = $"{deadC.Name} has joined!";
                    ServerMessage serverMessage = new ServerMessage(message);
                    Message.SendMessage(endpoint.Connection, serverMessage, DateTime.UtcNow);
                }
            }
            
            foreach(ReliableMessage reliableMessage in endpoint.RecentReliableMessages)
            {
                if(reliableMessage is SendChatMessage chatMessage)
                {
                    ChatMessage message = new ChatMessage(endpoint.ClientId, chatMessage.Message);
                    foreach(ServerSideClient target in Server!.CurrentClients)
                    {
                        if (!target.Equals(endpoint))
                        {
                            Message.SendMessage(target.Connection, message, DateTime.UtcNow);
                        }
                    }
                }

                
            }

            foreach(UnreliableMessage unreliableMessage in endpoint.RecentUnreliableMessages)
            {
                if(unreliableMessage is SendAudioMessage audioMessage)
                {
                    AudioMessage message = new AudioMessage(endpoint.ClientId, audioMessage.Message);
                    foreach(ServerSideClient target in Server!.CurrentClients)
                    {
                        if (!target.Equals(endpoint))
                        {
                            Message.SendMessage(target.Connection, message, DateTime.UtcNow);
                        }
                    }
                }

                if(unreliableMessage is SendPositionMessage positionMessage)
                {
                    PositionMessage message = new PositionMessage(endpoint.ClientId, positionMessage.Position);
                    foreach(ServerSideClient target in Server!.CurrentClients)
                    {
                        if (!target.Equals(endpoint))
                        {
                            Message.SendMessage(target.Connection, message, DateTime.UtcNow);
                        }
                    }
                }
            }
        }
    }
}

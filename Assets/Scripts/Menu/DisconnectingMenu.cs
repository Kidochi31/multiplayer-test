using System;
using System.Collections.Generic;
using System.Net;
using Relunrel.Connections;
using Unity.VisualScripting;
using UnityEngine;

public class DisconnectingMenu : MonoBehaviour
{
    public List<GameObject> ThisMenu;
    public GameObject NewMenu;

    private ClientNetwork? Client;
    private ServerNetwork? Server;

    void OnEnable()
    {
        // once this is enabled, disconnect clients and servers
        Client = FindAnyObjectByType<ClientNetwork>();
        Server = FindAnyObjectByType<ServerNetwork>();

        IPEndPoint? clientEndpoint = null;

        if(Client != null)
        {
            // disconnect the client from the server
            if(Client.Socket.InternalEndPoint is not null)
            {
                clientEndpoint = new(IPAddress.Loopback, Client.Socket.InternalEndPoint.Port);
            }
            Client.CurrentConnection.Disconnect(DateTime.UtcNow);
        }

        if(Server != null)
        {
            Server.Socket.DisableListening();
            // disconnect the server from all of the clients
            foreach(Connection c in Server.Socket.Connections.Values)
            {
                if (!c.RemoteEndPoint.Equals(clientEndpoint))
                {
                    c.Disconnect(DateTime.UtcNow);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldReset = true;
        // only wait for client if there is also not a server
        if(Client != null && Server == null)
        {
            // check if it has been disconnected
            if(Client.CurrentConnection.State != ConnectionState.Disconnected)
            {
                shouldReset = false;
                Debug.Log($"Client not yet disconnected: {Client.CurrentConnection.State}");
            }
        }
        if(Server != null)
        {
            // disconnect the server from all of the clients
            foreach(Connection c in Server.Socket.Connections.Values)
            {
                if(c.State != ConnectionState.Disconnected)
                {
                    shouldReset = false;
                    Debug.Log($"Server not yet disconnected: {c.State}");
                }
            }
        }

        if (shouldReset)
        {
            // completely disconnected - go to main menu and destroy server/client
            if(Server != null)
            {
                Destroy(Server.gameObject);
            }
            if(Client != null)
            {
                Destroy(Client.gameObject);
            }
            
            NewMenu.SetActive(true);
            foreach(GameObject menu in ThisMenu)
            {
                menu.SetActive(false);
            }
        }
    }
}

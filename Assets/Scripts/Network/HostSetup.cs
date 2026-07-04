using System;
using System.Net;
using UnityEngine;

public class HostSetup : MonoBehaviour
{
    private bool settingUp = true;
    public MenuChangeButton MenuChange;
    public GameObject RemoveOnFailure;

    // Update is called once per frame
    void Update()
    {
        if (settingUp)
        {
            ClientNetwork? client = FindAnyObjectByType<ClientNetwork>();
            ServerNetwork? server = FindAnyObjectByType<ServerNetwork>();
            if(client != null && server != null && client.State == ClientState.Idle && server.Socket.InternalEndPoint is not null)
            {
                IPEndPoint target = new IPEndPoint(IPAddress.Loopback, server.Socket.InternalEndPoint.Port);
                client.ConnectTo(target, DateTime.UtcNow);
                settingUp = false;
                MenuChange.ChangeMenu();
            }
        }
    }

    void OnDisable()
    {
        settingUp = true;
    }
}

using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

public class HostSetup : MonoBehaviour
{
    private bool settingUp = true;
    public UnityEvent OnSetupComplete;

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
                OnSetupComplete.Invoke();
            }
        }
    }

    void OnDisable()
    {
        settingUp = true;
    }
}

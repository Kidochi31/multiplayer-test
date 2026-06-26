using UnityEngine;
using Relunrel.Network;
using System;
using Relunrel.Connections;

public class ClientNetwork : MonoBehaviour
{
    public RSocket Socket;
    public string Username;
    public Connection? CurrentConnection;

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
    }
}

using System;
using System.Net;
using TMPro;
using UnityEngine;

public class HostSettingsMenu : MonoBehaviour
{
    public string DefaultText = "...";
    public ServerNetwork ServerNetwork;
    public TextMeshProUGUI LoopbackText;
    public TextMeshProUGUI LanText;
    public TextMeshProUGUI WanText;
    void OnEnable()
    {
        ServerNetwork = FindAnyObjectByType<ServerNetwork>();
        LoopbackText.text = DefaultText;
        LanText.text = DefaultText;
        WanText.text = DefaultText;
        
    }

    void Update()
    {
        if(LoopbackText.text == DefaultText)
        {
            if(ServerNetwork.Socket.InternalEndPoint is not null)
            {
                int port = ServerNetwork.Socket.InternalEndPoint.Port;
                LoopbackText.text = $"127.0.0.1:{port}";
            }
        }
        if(LanText.text == DefaultText)
        {
            if(ServerNetwork.Socket.InternalEndPoint is not null)
            {
                int port = ServerNetwork.Socket.InternalEndPoint.Port;
                IPAddress ip = NetworkHelper.GetLocalIPv4();
                if(ip is not null)
                {
                    LanText.text = $"{ip}:{port}";
                }
            }
        }
        if(WanText.text == DefaultText)
        {
            if(ServerNetwork.Socket.ExternalEndPoint is not null)
            {
                int port = ServerNetwork.Socket.ExternalEndPoint.Port;
                IPAddress ip = ServerNetwork.Socket.ExternalEndPoint.Address;
                WanText.text = $"{ip}:{port}";
            }
        }
    }
}

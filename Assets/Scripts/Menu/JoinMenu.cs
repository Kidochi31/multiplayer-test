using System;
using System.Net;
using TMPro;
using UnityEngine;

public class JoinMenu : MonoBehaviour
{
    public string DefaultText = "...";
    public ClientNetwork ClientNetwork;
    public TextMeshProUGUI LoopbackText;
    public TextMeshProUGUI LanText;
    public TextMeshProUGUI WanText;
    void OnEnable()
    {
        GameObject ClientObject = new GameObject("ClientNetwork");
        ClientNetwork = ClientObject.AddComponent<ClientNetwork>();
        LoopbackText.text = DefaultText;
        LanText.text = DefaultText;
        WanText.text = DefaultText;
        
    }

    void Update()
    {
        if(LoopbackText.text == DefaultText)
        {
            if(ClientNetwork.Socket.InternalEndPoint is not null)
            {
                int port = ClientNetwork.Socket.InternalEndPoint.Port;
                LoopbackText.text = $"127.0.0.1:{port}";
            }
        }
        if(LanText.text == DefaultText)
        {
            if(ClientNetwork.Socket.InternalEndPoint is not null)
            {
                int port = ClientNetwork.Socket.InternalEndPoint.Port;
                IPAddress ip = NetworkHelper.GetLocalIPv4();
                if(ip is not null)
                {
                    LanText.text = $"{ip}:{port}";
                }
            }
        }
        if(WanText.text == DefaultText)
        {
            if(ClientNetwork.Socket.ExternalEndPoint is not null)
            {
                int port = ClientNetwork.Socket.ExternalEndPoint.Port;
                IPAddress ip = ClientNetwork.Socket.ExternalEndPoint.Address;
                WanText.text = $"{ip}:{port}";
            }
        }
    }

    public void DestroyClient()
    {
        Destroy(ClientNetwork.gameObject);
    }
}

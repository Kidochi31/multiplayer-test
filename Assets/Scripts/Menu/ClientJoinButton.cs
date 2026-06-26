using System;
using System.Net;
using TMPro;
using UnityEngine;

public class ClientJoinButton : MonoBehaviour
{
    public TMP_InputField UsernameField;
    public TMP_InputField AddressField;
    public GameObject JoiningMenu;

    public void Join()
    {
        if(UsernameField.text == "")
        {
            Debug.Log("Name empty");
            return;
        }
        if(AddressField.text == "")
        {
            Debug.Log("Address empty");
            return;
        }
        IPEndPoint? endpoint = GetIPEndPointFromText(AddressField.text);
        if(endpoint is null)
        {
            Debug.Log("Invalid endpoint");
            return;
        }
        ClientNetwork network = FindAnyObjectByType<ClientNetwork>();
        network.Username = UsernameField.text;
        network.CurrentConnection = network.Socket.ConnectTo(endpoint, DateTime.UtcNow);
        JoiningMenu.SetActive(true);
    }

    static IPEndPoint? GetIPEndPointFromText(string input)
    {
        input = input.Trim();
        if (!input.Contains(':'))
        {
            return null;
        }
        string[] split = input.Split(':');
        if(split.Length != 2)
        {
            return null;
        }
        string possibleIp = split[0];
        possibleIp = possibleIp.Trim();
        bool isIp = IPAddress.TryParse(possibleIp, out IPAddress? ip);
        if (!isIp || ip is null)
        {
            return null;
        }
        string possiblePort = split[1];
        possiblePort = possiblePort.Trim();
        bool isPort = int.TryParse(possiblePort, out int port);
        if (!isPort)
        {
            return null;
        }
        try{
            return new IPEndPoint(ip, port);
        } catch (Exception)
        {
            return null;
        }
    }
}

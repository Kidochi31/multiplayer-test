using System;
using System.Net;
using TMPro;
using UnityEngine;

public class Holepunch : MonoBehaviour
{
    public TMP_InputField AddressField;
    private ServerNetwork? Server;

    void OnEnable()
    {
        Server = FindAnyObjectByType<ServerNetwork>();
    }

    public void DoHolepunch()
    {
        if(AddressField.text == "")
        {
            Debug.Log("Address empty");
            return;
        }
        IPEndPoint? endpoint = NetworkHelper.GetIPEndPointFromText(AddressField.text);
        if(endpoint is null)
        {
            Debug.Log("Invalid endpoint");
            return;
        }
        Server!.Socket.BeginHolePunch(endpoint, 30, DateTime.UtcNow);
        AddressField.text = "";
    }
}

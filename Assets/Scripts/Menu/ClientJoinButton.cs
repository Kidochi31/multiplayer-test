using System;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ClientJoinButton : MonoBehaviour
{
    public TMP_InputField UsernameField;
    public TMP_InputField AddressField;
    public UnityEvent OnJoin;

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
        IPEndPoint? endpoint = NetworkHelper.GetIPEndPointFromText(AddressField.text);
        if(endpoint is null)
        {
            Debug.Log("Invalid endpoint");
            return;
        }
        ClientNetwork network = FindAnyObjectByType<ClientNetwork>();
        network.Username = UsernameField.text;
        network.ConnectTo(endpoint, DateTime.UtcNow);
        OnJoin.Invoke();
    }

    
}

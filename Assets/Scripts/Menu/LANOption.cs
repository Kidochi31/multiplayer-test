using System;
using System.Net;
using TMPro;
using UnityEngine;

public class LANOption : MonoBehaviour
{
    public TMP_Text DescriptionText;
    public TMP_InputField UsernameField;
    public IPEndPoint TargetEndPoint;
    public string Description;
    public GameObject JoiningMenu;

    public void OnEnable()
    {
        DescriptionText.text = Description;
    }

    public void Join()
    {
        if(UsernameField.text == "")
        {
            Debug.Log("Name empty");
            return;
        }
        ClientNetwork network = FindAnyObjectByType<ClientNetwork>();
        network.Username = UsernameField.text;
        network.ConnectTo(TargetEndPoint, DateTime.UtcNow);
        JoiningMenu.SetActive(true);
    }
}

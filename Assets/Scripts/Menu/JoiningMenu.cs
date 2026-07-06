using System;
using System.Collections.Generic;
using Relanrel;
using Relunrel.Connections;
using Relunrel.Network;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class JoiningMenu : MonoBehaviour
{
    public GameObject BackButton;
    public TMP_Text ErrorText;
    private ClientNetwork Network;
    public string ErrorMessage;
    public UnityEvent OnSuccess;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        BackButton.SetActive(false);
        ErrorText.text = "";
        Network = FindAnyObjectByType<ClientNetwork>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Network.State == ClientState.Disconnected)
        {
            Network.Reset();
            ErrorText.text = ErrorMessage;
            BackButton.SetActive(true);
        }
        if(Network.State == ClientState.Connected)
        {
            OnSuccess.Invoke();
        }
    }
}

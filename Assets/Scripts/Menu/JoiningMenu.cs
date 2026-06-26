using System;
using System.Collections.Generic;
using Relunrel.Connections;
using Relunrel.Network;
using TMPro;
using UnityEngine;

public class JoiningMenu : MonoBehaviour
{
    public GameObject BackButton;
    public TMP_Text ErrorText;
    private ClientNetwork Network;
    public string ErrorMessage;
    public List<GameObject> ThisMenu;
    public GameObject Game;
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
        Connection? connection = Network.CurrentConnection;
        if(connection is null)
        {
            return;
        }
        if(connection.State == ConnectionState.Disconnected)
        {
            Network.CurrentConnection = null;
            ErrorText.text = ErrorMessage;
            BackButton.SetActive(true);
        }
        if(connection.State == ConnectionState.Connected)
        {
            Debug.Log("connected");
            Game.SetActive(true);
            foreach(GameObject gameObject in ThisMenu)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

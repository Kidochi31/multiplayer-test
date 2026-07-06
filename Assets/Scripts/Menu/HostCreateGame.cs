using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HostCreateGame : MonoBehaviour
{
    public TMP_InputField UsernameField;
    public UnityEvent OnCreateGame;
    public void CreateGame()
    {
        if(UsernameField.text == "")
        {
            Debug.Log("Text field empty");
            return;
        }
        // create game with username
        GameObject ClientObject = new GameObject("ClientNetwork");
        var ClientNetwork = ClientObject.AddComponent<ClientNetwork>();
        ClientNetwork.Username = UsernameField.text;
        GameObject ServerObject = new GameObject("ServerNetwork");
        var ServerNetwork = ServerObject.AddComponent<ServerNetwork>();
        OnCreateGame.Invoke();
    }
}

using System;
using System.Text;
using TMPro;
using UnityEngine;

public class ClientChat : MonoBehaviour
{
    public string CurrentText;
    public TMP_Text ChatText;
    public TMP_InputField ChatInput;
    private ClientNetwork? Client;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Client = FindAnyObjectByType<ClientNetwork>();
        Debug.Log(Client);
    }

    // Update is called once per frame
    void Update()
    {
        if(Client != null)
        {
            if(Client.CurrentConnection is not null)
            {
                while (Client.CurrentConnection.ReliableOrderedMessagesAvailable)
                {
                    byte[] messageBytes = Client.CurrentConnection.DequeueReliableOrderedMessage()!;
                    string message = Encoding.UTF8.GetString(messageBytes);
                    CurrentText += message + "\n";
                    ChatText.text = CurrentText;
                }
            }
        }
        
    }

    public void SendChatMessage()
    {
        if(ChatInput.text == "")
        {
            return;
        }
        string text = ChatInput.text;
        ChatInput.text = "";

        if(Client != null)
        {
            if(Client.CurrentConnection is not null)
            {
                string message = Client.Username + ": " + text;
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                Client.CurrentConnection.SendReliableOrdered(messageBytes, DateTime.UtcNow);
                CurrentText += message + "\n";
                ChatText.text = CurrentText;
                Debug.Log("sent");
            }
            else
            {
                Debug.Log("Connection is null");
            }
        }
        else
        {
            Debug.Log("client is null");
        }
    }
}

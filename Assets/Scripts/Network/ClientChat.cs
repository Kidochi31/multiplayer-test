using System;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ClientChat : MonoBehaviour
{
    public string CurrentText;
    public TMP_Text ChatText;
    public TMP_InputField ChatInput;
    private ClientNetwork? Client;
    public CheckDisconnect CheckDisconnect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        var newClient = FindAnyObjectByType<ClientNetwork>();
        if(newClient != Client)
        {
            CurrentText = "";
            ChatText.text = CurrentText;
        }
        Client = newClient;
        CheckDisconnect.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Client != null)
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
            string message = Client.Username + ": " + text;
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Client.CurrentConnection.SendReliableOrdered(messageBytes, DateTime.UtcNow);
            CurrentText += message + "\n";
            ChatText.text = CurrentText;
            Debug.Log("sent");
        }
        else
        {
            Debug.Log("client is null");
        }
    }
}

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
            foreach(ReliableMessage message in Client.RecentReliableMessages)
            {
                if(message is ChatMessage chat)
                {
                    CurrentText += Client.IdToClient[chat.ClientId].Name + ": " + chat.Message + "\n";
                    ChatText.text = CurrentText;
                }
                if(message is ServerMessage server)
                {
                    CurrentText += "<server>: " + server.Message + "\n";
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
            string message = Client.Username + ": " + text;
            SendChatMessage sendMessage = new SendChatMessage(message);
            Client.SendMessage(sendMessage, DateTime.UtcNow);
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

using System;
using System.Collections.Generic;
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

    public GameObject VoiceChatExample;
    private Dictionary<ushort, GameObject> VoiceChatUsers = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Client = FindAnyObjectByType<ClientNetwork>();
        CurrentText = "";
        ChatText.text = CurrentText;
        CheckDisconnect.gameObject.SetActive(true);

        VoiceChatUsers.Clear();
        foreach(ClientSideClient client in Client.CurrentClients)
        {
            if (client.Equals(Client.ThisClient))
            {
                continue;
            }
            GameObject voiceChat = Instantiate(VoiceChatExample, transform);
            VoiceChatUsers[client.ClientId] = voiceChat;
            voiceChat.GetComponent<VoiceChatReceiver>().SourceClientId = client.ClientId;
            voiceChat.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Client != null)
        {
            foreach(ClientSideClient client in Client.DeadClients)
            {
                GameObject voicechat = VoiceChatUsers[client.ClientId];
                VoiceChatUsers.Remove(client.ClientId);
                Destroy(voicechat);
            }
            foreach(ClientSideClient client in Client.NewClientInfo)
            {
                if (client.Equals(Client.ThisClient))
                {
                    continue;
                }
                GameObject voiceChat = Instantiate(VoiceChatExample, transform);
                VoiceChatUsers[client.ClientId] = voiceChat;
                voiceChat.GetComponent<VoiceChatReceiver>().SourceClientId = client.ClientId;
                voiceChat.SetActive(true);
            }
            foreach(ClientSideClient client in Client.NewClients)
            {
                GameObject voiceChat = Instantiate(VoiceChatExample, transform);
                VoiceChatUsers[client.ClientId] = voiceChat;
                voiceChat.GetComponent<VoiceChatReceiver>().SourceClientId = client.ClientId;
                voiceChat.SetActive(true);
            }

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

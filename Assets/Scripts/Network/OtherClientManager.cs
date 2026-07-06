using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class OtherClientManager : MonoBehaviour
{
    public OtherClient UserExample;
    private ClientNetwork? Client;
    private Dictionary<ushort, GameObject> OtherUsers = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Client = FindAnyObjectByType<ClientNetwork>();

        
        foreach(ClientSideClient client in Client.CurrentClients)
        {
            AddClient(client);
        }
    }

    void AddClient(ClientSideClient client)
    {
        if (client.Equals(Client.ThisClient))
        {
            return;
        }
        GameObject user = Instantiate(UserExample.gameObject, transform);
        user.GetComponent<OtherClient>().ClientId = client.ClientId;
        OtherUsers[client.ClientId] = user;
        user.SetActive(true);
    }

    void RemoveClient(ClientSideClient client)
    {
        GameObject voicechat = OtherUsers[client.ClientId];
        OtherUsers.Remove(client.ClientId);
        Destroy(voicechat);
    }

    void OnDisable()
    {
        foreach(GameObject voiceChat in OtherUsers.Values)
        {
            Destroy(voiceChat);
        }
        OtherUsers.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if(Client != null)
        {
            foreach(ClientSideClient client in Client.DeadClients)
            {
                RemoveClient(client);
            }
            foreach(ClientSideClient client in Client.NewClientInfo)
            {
                AddClient(client);
            }
            foreach(ClientSideClient client in Client.NewClients)
            {
                AddClient(client);
            }

            foreach(UnreliableMessage message in Client.RecentUnreliableMessages)
            {
                if(message is PositionMessage position)
                {
                    OtherUsers[position.ClientId].transform.position = position.Position;
                }
            }
            
        }
        
    }
}

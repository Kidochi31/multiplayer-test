using UnityEngine;

public class OtherClientMovement : MonoBehaviour
{
    private ClientNetwork? Client;
    private ushort ClientId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Client = FindAnyObjectByType<ClientNetwork>();
        ClientId = GetComponentInParent<OtherClient>().ClientId;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(UnreliableMessage message in Client.RecentUnreliableMessages)
        {
            if(message is PositionMessage position && position.ClientId == ClientId)
            {
                transform.position = position.Position;
            }
        }
    }
}

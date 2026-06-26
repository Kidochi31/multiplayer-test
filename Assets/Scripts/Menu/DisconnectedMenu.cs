using UnityEngine;

public class DisconnectedMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        ServerNetwork? server = FindAnyObjectByType<ServerNetwork>();
        if(server != null)
        {
            Destroy(server.gameObject);
        }
        ClientNetwork? client = FindAnyObjectByType<ClientNetwork>();
        if(client != null)
        {
            Destroy(client.gameObject);
        }
    }
}

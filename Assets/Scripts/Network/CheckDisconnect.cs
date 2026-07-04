using UnityEngine;

public class CheckDisconnect : MonoBehaviour
{
    private ClientNetwork? Client;
    public MenuChangeButton OnDisconnect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Client = FindAnyObjectByType<ClientNetwork>();
        
    }

    public void Update()
    {
        if(Client != null && Client.State == ClientState.Disconnected || Client.State == ClientState.PassiveDisconnecting)
        {
            // disconnected
            OnDisconnect.ChangeMenu();
        }
    }
}

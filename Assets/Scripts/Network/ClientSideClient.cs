using UnityEngine;

public class ClientSideClient
{
    public ushort ClientId;
    public string Name;

    public ClientSideClient(ushort clientId, string name)
    {
        ClientId = clientId;
        Name = name;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class LANMenu : MonoBehaviour
{
    private Relanrel.Client Client;
    public GameObject ContentField;
    public LANOption ExampleOption;
    public List<LANOption> Options = new();
    public List<IPEndPoint> Targets = new();
    private ClientNetwork ClientNetwork;

    void OnEnable()
    {
        IPAddress? broadcast = Relanrel.Client.GetBroadcastAddress();
        if(broadcast is not null)
        {
            Client = Relanrel.Client.CreateClient(6767, 0x67676767, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), broadcast);
        }

        // need to delete all of the options except for the example option
        // the example child is necessarily the first, so skip it and go to the next one
        int children = ContentField.transform.childCount;
        for(int i = children - 1; i > 0 ; i--)
        {
            // DESTROY ALL THE CHILDREN!!!
            Destroy(ContentField.transform.GetChild(i).gameObject);
        }
        Options.Clear();
        Targets.Clear();

        GameObject ClientObject = new GameObject("ClientNetwork");
        ClientNetwork = ClientObject.AddComponent<ClientNetwork>();
    }

    public void DestroyClient()
    {
        Destroy(ClientNetwork.gameObject);
    }

    void CreateNewOption(IPEndPoint targetEndPoint, string description)
    {
        // need to create a option
        GameObject newOptionObject = Instantiate(ExampleOption.gameObject, ContentField.transform);
        LANOption newOption = newOptionObject.GetComponent<LANOption>();
        newOption.TargetEndPoint = targetEndPoint;
        newOption.Description = description;
        Options.Add(newOption);
        Targets.Add(targetEndPoint);
        newOptionObject.SetActive(true);
    }

    void RemoveOption(IPEndPoint targetEndPoint)
    {
        int index = Targets.IndexOf(targetEndPoint);
        if(index < 0)
        {
            return;
        }
        Targets.RemoveAt(index);
        LANOption option = Options[index];
        Destroy(option.gameObject);
        Options.RemoveAt(index);
    }

    void Update()
    {
        if(Client is null)
        {
            return;
        }
        (IPEndPoint[] deadS, IPEndPoint[] newS) = Client.Tick();
        foreach(IPEndPoint s in deadS)
        {
            RemoveOption(s);
        }
        foreach(IPEndPoint s in newS)
        {
            CreateNewOption(s, Client.Servers[s].Info);
        }
    }

    void OnDisable()
    {
        Client = null;
    }
}

using UnityEngine;

public class HostOnly : MonoBehaviour
{
    public GameObject HostOnlyObject;

    private void OnEnable()
    {
        // if there is no server - disable object
        if(FindAnyObjectByType<ServerNetwork>() == null)
        {
            HostOnlyObject.SetActive(false);
        }
        else
        {
            HostOnlyObject.SetActive(true);
        }
    }
}

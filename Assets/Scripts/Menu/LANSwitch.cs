using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LANSwitch : MonoBehaviour
{
    public Toggle LANToggle;
    public TMP_InputField LANText;
    ServerNetwork Server;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Server = FindAnyObjectByType<ServerNetwork>();
        if (Server.IsOpenToLan)
        {
            LANToggle.isOn = true;
        }
        else
        {
            LANToggle.isOn = false;
        }
    }

    public void LANTextChange(string text)
    {
        if (Server.IsOpenToLan)
        {
            Server.OpenToLan(text);
        }
    }

    public void Toggle(bool LAN)
    {
        if (LAN)
        {
            Server.OpenToLan(LANText.text);
        }
        else
        {
            Server.CloseLan();
        }
    }
}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public GameObject DisconnectingMenu;
    public CheckDisconnect DisconnectChecker;

    public void Quit()
    {
        // Go to the disconnecting menu to finish disconnecting
        DisconnectingMenu.SetActive(true);
        // disable the disconnect checker
        DisconnectChecker.gameObject.SetActive(false);
    }
}

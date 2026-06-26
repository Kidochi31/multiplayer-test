using System.Collections.Generic;
using UnityEngine;

public class MenuChangeButton : MonoBehaviour
{
    public List<GameObject> ThisMenus;
    public GameObject? ThisMenu;
    public GameObject? TargetMenu;

    public void ChangeMenu()
    {
        if(TargetMenu != null)
        {
            TargetMenu.SetActive(true);
        }
        foreach(GameObject menu in ThisMenus)
        {
            menu.SetActive(false);
        }
        if(ThisMenu != null)
        {
            ThisMenu.SetActive(false);
        }
    }
}

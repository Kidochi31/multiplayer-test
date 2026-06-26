using UnityEngine;

public class MenuChangeButton : MonoBehaviour
{
    public GameObject? ThisMenu;
    public GameObject? TargetMenu;

    public void ChangeMenu()
    {
        if(TargetMenu != null)
        {
            TargetMenu.SetActive(true);
        }
        if(ThisMenu != null)
        {
            ThisMenu.SetActive(false);
        }
    }
}

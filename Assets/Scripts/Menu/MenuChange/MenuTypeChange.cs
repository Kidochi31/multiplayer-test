using UnityEngine;

public class MenuTypeChange : MenuChange
{
    public MenuType? DisableType;
    public GameObject? NewMenu;
    public override void ChangeMenu()
    {
        if(DisableType != null)
        {
            foreach(Object menuObject in FindObjectsByType(DisableType.GetType(), FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                MenuType menu = (MenuType)menuObject;
                if (menu.isActiveAndEnabled)
                {
                    menu.gameObject.SetActive(false);
                }
            }
        }
        if(NewMenu != null)
        {
            NewMenu.SetActive(true);
        }
    }
}

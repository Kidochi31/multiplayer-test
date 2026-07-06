using UnityEngine;

public class MenuChangeObject : MenuChange
{
    public GameObject? Disable;
    public GameObject? Enable;
    public override void ChangeMenu()
    {
        if(Disable != null)
        {
            Disable.SetActive(false);
        }
        if(Enable != null)
        {
            Enable.SetActive(true);
        }
    }
}

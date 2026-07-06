using System.Collections.Generic;
using UnityEngine;

public class MenuChangeList : MenuChange
{
    public List<GameObject?> Disable;
    public List<GameObject?> Enable;
    public override void ChangeMenu()
    {
        foreach(GameObject disable in Disable)
        {
            if(disable != null)
            {
                disable.SetActive(false);
            }
        }
        foreach(GameObject enable in Enable)
        {
            if(enable != null)
            {
                enable.SetActive(true);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.Events;

public class GameMenu : MonoBehaviour
{
    public UnityEvent OnMenu;

    void OnEnable()
    {
        // game starts not in menu -> enable player input
        OnRemoveMenu();
    }

    public void OnRemoveMenu()
    {
        KeyInput.EnableInputs(KeyInput.PlayerInputType);
        KeyInput.DisableInputs(KeyInput.MenuInputType);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnIntoMenu()
    {
        KeyInput.DisableInputs(KeyInput.PlayerInputType);
        KeyInput.EnableInputs(KeyInput.MenuInputType);
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (KeyInput.GetKeyBinding<GameMenuKey>().DownThisFrame)
        {
            // disable all player inputs
            OnIntoMenu();
            OnMenu.Invoke();
        }
    }

    void OnDisable()
    {
        OnIntoMenu();
    }
}

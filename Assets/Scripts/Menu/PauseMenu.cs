using UnityEngine;

public class PauseMenu : MainMenu
{
    private bool isPaused;
    public GameObject buttons;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPauseMenu();
        }
    }

    public void SwitchPauseMenu()
    {
        isPaused = !isPaused;
        buttons.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }
}

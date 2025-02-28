using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuBehavior : MonoBehaviour
{
    public GameObject pause;
    public GameObject resume;
    public GameObject gameOver;
    private bool isPaused;

    public EventSystem eventSystem;
    // Start is called before the first frame update
    public void LoadLevel()
    {
        SceneManager.LoadScene("Level");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }
    // Update is called once per frame
    void Update()
    {
        if (isPaused != true)
        {
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                pauseGame();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                LoadContinueGame();
            }
        }

      
    }

    void pauseGame()
    {
        pause.SetActive(true);
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        eventSystem.sendNavigationEvents = false;
        Time.timeScale = 0;

    }

    public void LoadContinueGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        pause.SetActive(false);
        eventSystem.sendNavigationEvents = true;
    }

   public void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        gameOver.SetActive(true);
        // eventSystem.sendNavigationEvents = false;

    }
}

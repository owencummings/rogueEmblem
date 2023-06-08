using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static bool paused = false;
    public GameObject pauseMenu;

    // Start is called before the first frame update
    void Pause(){
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }

    void Resume(){
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    void Start(){ pauseMenu.SetActive(false); }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Resume();
                paused = false;
            }
            else 
            {
                Pause();
                paused = true;
            }
        }
    }
}

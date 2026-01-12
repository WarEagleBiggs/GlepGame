using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsToggle : MonoBehaviour
{
    public GameObject target;
    
    void Update()
    {
        if (!target) return;

        target.SetActive(Input.GetKey(KeyCode.Tab));
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene("TestScene");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    
    
}

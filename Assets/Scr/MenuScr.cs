using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScr : MonoBehaviour
{
    public GameObject Menu;
    public void PlayButton()
    {
        SceneManager.LoadScene(1);
    }
    public void MenuButton()
    {
        SceneManager.LoadScene(0);
    }
    public void ExitButton()
    {
        Application.Quit();
    }

    public void ReturnButton()
    {
        Menu.SetActive(false);
    }

    public void OpenMenuButton()
    {
        Menu.SetActive(true);
    }
}

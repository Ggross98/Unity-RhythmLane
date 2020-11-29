using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu, settingMenu, playMenu;

    public void ShowSettingMenu()
    {
        ShowSettingMenu(!settingMenu.activeSelf);
    }

    public void ShowSettingMenu(bool b)
    {
        mainMenu.SetActive(!b);
        settingMenu.SetActive(b);
    }

    public void ShowPlayMenu()
    {
        ShowPlayMenu(!playMenu.activeSelf);
    }

    public void ShowPlayMenu(bool b)
    {
        mainMenu.SetActive(!b);
        playMenu.SetActive(b);
    }


    void Start()
    {
        ShowSettingMenu(false);
    }


    public void Quit()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MusicSelect");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
public class SettingScene : MonoBehaviour
{
    public SettingPanel settingPanel;

    public void GotoScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }
}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Menu{
public class SettingPanel : MonoBehaviour
{
    public Slider seSlider, musicSlider;
    public Text seText, musicText;

    public Slider offsetSlider;
    public Text offsetText;

    public KeyCodeButton key0, key1, key2, key3, key4;
    private KeyCodeButton[] buttons;

    public Toggle clap;

    public Slider speedSlider;
    public Text speedText;

    public Dropdown resolution;

    public GameObject page0, page1;
    private GameObject[] pages;
    private int pageIndex = 0;


    void OnEnable()
    {
        PlayerSettings setting = PlayerSettings.Instance;

        seSlider.value = setting.seVolume;
        seText.text = (int)(100 * seSlider.value) + "";

        musicSlider.value = setting.musicVolume;
        musicText.text = (int)(100 * musicSlider.value) + "";

        buttons = new KeyCodeButton[]{key0, key1, key2, key3, key4};
        for(int i = 0;i<5;i++){
            buttons[i].SetKeyCode(setting.GetKeyCode(i));
        }
        /*
        key0.SetKeyCode(setting.KEY0);
        key1.SetKeyCode(setting.KEY1);
        key2.SetKeyCode(setting.KEY2);
        key3.SetKeyCode(setting.KEY3);
        key4.SetKeyCode(setting.KEY4);
        */
        clap.isOn = (setting.clap == 1);

        offsetSlider.value = setting.playerOffset;
        offsetText.text = setting.playerOffset + "";

        speedSlider.value = setting.speed;
        speedText.text = setting.speed + "";


        pages = new GameObject[] { page0, page1 };
        Switch(0);
    }

    void Update()
    {
        if (isActiveAndEnabled)
        {
            seText.text = (int)(100 * seSlider.value) + "";
            musicText.text = (int)(100 * musicSlider.value) + "";
            offsetText.text = (int)offsetSlider.value + "";
            speedText.text = (int)speedSlider.value + "";


        }
    }


    public void Switch()
    {
        Switch(1 - pageIndex);
    }

    public void Switch(int i)
    {
        pageIndex = i;
        pages[i].SetActive(true);
        pages[1 - i].SetActive(false);
    }



    public void Apply()
    {
        PlayerSettings setting = PlayerSettings.Instance;

        setting.SetKeyCodes(key0.keyCode, key1.keyCode, key2.keyCode, key3.keyCode, key4.keyCode);

        setting.SetSettings(musicSlider.value, seSlider.value, clap.isOn ? 1 : 0, (int)offsetSlider.value, (int)speedSlider.value);



    }





}
}



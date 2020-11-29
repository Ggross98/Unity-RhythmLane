using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoteEditor.Utility;

namespace Game
{
    public class PlayerSettings : SingletonMonoBehaviour<PlayerSettings>
    {
        public float musicVolume = 0.5f, seVolume = 0.5f;

        public int clap = 1;

        public int offset = 0;

        public int speed = 10;

        public int  KEY0 = (int)KeyCode.A, KEY1 = (int)KeyCode.S, KEY2 = (int)KeyCode.D, KEY3 = (int)KeyCode.F, KEY4 = (int)KeyCode.G;
        

        void Awake()
        {
            if (PlayerPrefs.HasKey("MusicVolume"))
                musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            if (PlayerPrefs.HasKey("SEVolume"))
                seVolume = PlayerPrefs.GetFloat("SEVolume");
            if (PlayerPrefs.HasKey("Clap"))
                clap = PlayerPrefs.GetInt("Clap");
            if (PlayerPrefs.HasKey("Offset"))
                offset = PlayerPrefs.GetInt("Offset");
            if (PlayerPrefs.HasKey("Speed"))
                speed = PlayerPrefs.GetInt("Speed");

            if (PlayerPrefs.HasKey("KEY0"))
                KEY0 = PlayerPrefs.GetInt("KEY0");
            if (PlayerPrefs.HasKey("KEY1"))
                KEY1 = PlayerPrefs.GetInt("KEY1");
            if (PlayerPrefs.HasKey("KEY2"))
                KEY2 = PlayerPrefs.GetInt("KEY2");
            if (PlayerPrefs.HasKey("KEY3"))
                KEY3 = PlayerPrefs.GetInt("KEY3");
            if (PlayerPrefs.HasKey("KEY4"))
                KEY4 = PlayerPrefs.GetInt("KEY4");

            //Input.GetKey()

        }

        public void SetSettings(float music, float se, int clap, int offset, int speed)
        {

            musicVolume = music;
            seVolume = se;
            this.clap = clap;
            this.offset = offset;
            this.speed = speed;

            PlayerPrefs.SetFloat("MusicVolume", music);
            PlayerPrefs.SetFloat("SEVolume", seVolume);
            PlayerPrefs.SetInt("Clap", clap);
            PlayerPrefs.SetInt("Offset", offset);
            PlayerPrefs.SetInt("Speed", speed);
            //DontDestroyOnLoad(gameObject);
        }

        public void SetKeyCodes(KeyCode k0, KeyCode k1, KeyCode k2, KeyCode k3, KeyCode k4)
        {
            KEY0 = (int)k0;
            KEY1 = (int)k1;
            KEY2 = (int)k2;
            KEY3 = (int)k3;
            KEY4 = (int)k4;


            PlayerPrefs.SetInt("KEY0", (int)k0);
            PlayerPrefs.SetInt("KEY1", (int)k1);
            PlayerPrefs.SetInt("KEY2", (int)k2);
            PlayerPrefs.SetInt("KEY3", (int)k3);
            PlayerPrefs.SetInt("KEY4", (int)k4);
        }
        public void SetKeyCodes(int k0, int k1, int k2, int k3, int k4)
        {
            KEY0 = k0;
            KEY1 = k1;
            KEY2 = k2;
            KEY3 = k3;
            KEY4 = k4;

            PlayerPrefs.SetInt("KEY0", k0);
            PlayerPrefs.SetInt("KEY1", k1);
            PlayerPrefs.SetInt("KEY2", k2);
            PlayerPrefs.SetInt("KEY3", k3);
            PlayerPrefs.SetInt("KEY4", k4);
        }

    }
}


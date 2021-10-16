using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MusicSelect
{
    public class ListItem : MonoBehaviour
    {

        Image image;
        Text text;

        //json文件名，无后缀
        public string fileName;

        void Awake()
        {
            image = GetComponent<Image>();
            text = GetComponentInChildren<Text>();
        }

        public void SetName(string n)
        {
            fileName = n;

            text.text = n;
        }

    }
}

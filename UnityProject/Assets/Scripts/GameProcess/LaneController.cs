using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoteEditor.Utility;

namespace Game.Process
{
    public class LaneController : SingletonMonoBehaviour<LaneController>
    {
        public GameObject lanePrefab;

        public Transform laneParent;

        private List<GameObject> laneList = new List<GameObject>();

        private List<Image> imgList = new List<Image>();

        private float width;

        public KeyCode KEY0 = KeyCode.A, KEY1 = KeyCode.S, KEY2 = KeyCode.D, KEY3 = KeyCode.F, KEY4 = KeyCode.G;

        private KeyCode[] keys;

        private bool playing = false;

        void Awake()
        {
            if(laneParent == null || laneParent == null)
            {
                Debug.LogError("Lane component lost!");
            }

            width = laneParent.GetComponent<RectTransform>().sizeDelta.x;

            KEY0 = (KeyCode)(PlayerSettings.Instance.KEY0);
            KEY1 = (KeyCode)(PlayerSettings.Instance.KEY1);
            KEY2 = (KeyCode)(PlayerSettings.Instance.KEY2);
            KEY3 = (KeyCode)(PlayerSettings.Instance.KEY3);
            KEY4 = (KeyCode)(PlayerSettings.Instance.KEY4);

        }

        public void CreateLanes(int laneCount)
        {




            keys = new KeyCode[] { KEY0, KEY1, KEY2, KEY3, KEY4 };

            if (laneCount<1 || laneCount > 5)
            {
                Debug.LogError("Incorrect lane number!");
                return;
            }

            if(laneList.Count == laneCount)
            {

            }
            else
            {
                for (int i = 0; i < laneCount; i++)
                {
                    GameObject obj = Instantiate(lanePrefab, laneParent);

                    obj.name = "lane " + i;

                    obj.GetComponentInChildren<Text>().text = keys[i].ToString();

                    laneList.Add(obj);


                    Image img = obj.transform.Find("ShowClick").GetComponent<Image>();
                    imgList.Add(img);
                }
            }

            

            playing = true;
        }

        void Update()
        {
            if (playing)
            {
                for(int i = 0; i < laneList.Count; i++)
                {
                    
                    if (Input.GetKey(keys[i]))
                    {
                        imgList[i].gameObject.SetActive(true);

                    }
                    else
                    {
                        imgList[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public float GetLaneX(int index)
        {
            if (index < 0 || index >= laneList.Count) return -1;
            else
            {
                //float x1 = laneList[index].GetComponent<RectTransform>().anchoredPosition.x;
                //float x2 = laneParent.GetComponent<RectTransform>().anchoredPosition.x;

                //Debug.Log(x1 + "-" + x2);

                float delta = width / laneList.Count;
                //float center = width/2;


                return delta * (0.5f + index) - width/2;

            }
        }



    }
}



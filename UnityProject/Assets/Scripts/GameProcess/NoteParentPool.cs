using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoteEditor.Utility;

namespace Game.Process
{
    public class NoteParentPool : SingletonMonoBehaviour<NoteParentPool>
    {
        [SerializeField] private GameObject parentPrefab;

        private List<NoteParent> objList = new List<NoteParent>();

        public Transform noteField;

        public int initSize = 20;

        public GameObject GetNoteParent(int num)
        {
            //寻找已经生成的对象
            for(int i = 0; i < objList.Count; i++)
            {
                NoteParent p = objList[i];
                GameObject obj = p.gameObject;

                if(obj.activeSelf && p.num == num)
                {
                    return obj;
                }
            }

            //将休眠对象激活
            for(int i = 0; i < objList.Count; i++)
            {
                NoteParent p = objList[i];
                GameObject obj = p.gameObject;

                if (!obj.activeSelf)
                {
                    p.num = num;
                    p.hit = false;
                    obj.name = "NoteParent " + num;
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, NotesController.Instance.maxY);
                    obj.SetActive(true);
                    return obj;
                }
            }

            //生成新对象
            GameObject _obj = CreateNoteParent(num);
            _obj.SetActive(true);
            return _obj;

        }

        private GameObject CreateNoteParent(int num)
        {
            GameObject obj = Instantiate(parentPrefab, noteField);
            obj.SetActive(false);

            NoteParent parent = obj.GetComponent<NoteParent>();
            parent.num = num;

            obj.name = "NoteParent " + num;
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, NotesController.Instance.maxY);

            objList.Add(parent);
            return obj;
        }

        public void Init()
        {
            for(int i = 0; i < initSize; i++)
            {
                CreateNoteParent(-1);
            }
            
        }


    }

}

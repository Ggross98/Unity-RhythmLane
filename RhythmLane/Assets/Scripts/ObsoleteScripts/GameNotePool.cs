using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoteEditor.Utility;

using NoteEditor.DTO;



namespace Game.Process
{
    public class GameNotePool : SingletonMonoBehaviour<GameNotePool>
    {
        [SerializeField]private GameObject notePrefab, holdPrefab;

        //private List<GameObject> noteList= new List<GameObject>();

        public Transform noteParent;

        //public int initSize = 1000;


        /*
        public GameObject GetNote()
        {
            for(int i = 0; i < noteList.Count; i++)
            {
                if (!noteList[i].activeSelf)
                {
                    noteList[i].SetActive(true);
                    return noteList[i];
                }
            }

            GameObject note = CreateNote();
            note.SetActive(true);
            return note;

        }
        */
        /*
        public GameObject GetNote(int num)
        {
            for (int i = 0; i < noteList.Count; i++)
            {
                if (!noteList[i].activeSelf)
                {
                    //noteList[i].transform.parent = NoteParentPool.Instance.GetNoteParent(num).transform;
                    noteList[i].SetActive(true);
                    return noteList[i];
                }
            }

            GameObject note = CreateNote(num);
            note.SetActive(true);
            return note;
        }*/

        /*
        public GameNote GetNote(MusicDTO.Note n)
        {
            for (int i = 0; i < noteList.Count; i++)
            {
                if (!noteList[i].activeSelf)
                {
                    //noteList[i].transform.parent = NoteParentPool.Instance.GetNoteParent(num).transform;
                    GameNote gn = noteList[i].GetComponent<GameNote>();
                    gn.Init(n);
                    gn.gameObject.SetActive(true);
                    //noteList[i].SetActive(true);
                    return gn;
                }
            }

            GameObject note = CreateNote(n);
            note.SetActive(true);
            GameNote _gn = note.GetComponent<GameNote>();
            return _gn;
        }
        */


        private GameObject CreateNote()
        {
            GameObject obj = Instantiate(notePrefab, noteParent);
            obj.SetActive(false);
            //noteList.Add(obj);
            return obj;
        }

        /*
        private GameObject CreateNote(int num)
        {
            GameObject obj = Instantiate(notePrefab);
            obj.SetActive(false);
            noteList.Add(obj);
            return obj;
        }*/

        public GameObject CreateNote(MusicDTO.Note n)
        {
            var obj = CreateNote();
            obj.GetComponent<NoteObject>().Init(n);
            return obj;
        }

        /*
        public void Init()
        {
            for(int i = 0; i < noteList.Count; i++)
            {
                noteList[i].SetActive(false);
            }

            for(int i = noteList.Count; i < initSize; i++)
            {
                CreateNote();
            }
        }*/


    }

}



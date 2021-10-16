using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoteEditor.Utility;
using NoteEditor.Model;

namespace Game.MusicSelect
{
    /// <summary>
    /// 存储所选择乐谱的信息
    /// </summary>
    public class NotesContainer : SingletonMonoBehaviour<NotesContainer>
    {
        public AudioClip music;
        public string json;
        public bool autoplay;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void DestroySelf(){
            Destroy(gameObject);
        }

    }
}

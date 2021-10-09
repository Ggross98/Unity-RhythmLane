using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoteEditor.Utility;
using NoteEditor.Model;

namespace Game.MusicSelect
{
    public class NotesContainer : SingletonMonoBehaviour<NotesContainer>
    {
        public AudioClip music;

        public string json;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);


        }

    }
}

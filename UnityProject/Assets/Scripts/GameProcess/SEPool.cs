using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoteEditor.Utility;

namespace Game.Process
{
    public class SEPool : SingletonMonoBehaviour<SEPool>
    {
        public GameObject audioPrefab;

        public Transform audioParent;

        public int size = 10;

        private List<AudioSource> playingList = new List<AudioSource>(), waitingList = new List<AudioSource>();

        public AudioClip clap;

        public bool muted = false;

        public float clapPauseTime = 0.05f;
        public bool clapPausing = false;

        private float volume;

        void Start()
        {
            for(int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(audioPrefab, audioParent);
                AudioSource audio = obj.GetComponent<AudioSource>();

                waitingList.Add(audio);
            }

            volume = PlayerSettings.Instance.seVolume;
        }

        public void PlaySound(AudioClip clip)
        {
            if (muted) return;

            if (waitingList.Count > 0)
            {
                AudioSource audio = waitingList[0];

                audio.clip = clip;
                audio.time = 0;
                audio.volume = volume;

                audio.Play();

                waitingList.Remove(audio);
                playingList.Add(audio);
            }
        }

        public void PlayClap()
        {
            if (clapPausing) return;
            PlaySound(clap);
            StartCoroutine(ClapPause());
        }
            

        private IEnumerator ClapPause()
        {
            clapPausing = true;
            yield return new WaitForSeconds(clapPauseTime);
            clapPausing = false;
        }

        void Update()
        {
            for(int i = 0; i < playingList.Count; i++)
            {
                AudioSource audio = playingList[i];

                if (!playingList[i].isPlaying)
                {
                    playingList.Remove(audio);
                    waitingList.Add(audio);
                }
            }
            /*
            if (Input.GetKeyUp(KeyCode.M))
            {
                muted = !muted;
            }*/
        }
            
            
    }
}



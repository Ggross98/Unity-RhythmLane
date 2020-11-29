using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoteEditor.Utility;


namespace Game.Process
{
    public class MusicController : SingletonMonoBehaviour<MusicController>
    {
        [SerializeField]private AudioSource music;

        [SerializeField] private int samples;

        private float volume;

        public Text musicName, musicTime;

        //public bool playing;

        void Start()
        {
            volume = PlayerSettings.Instance.musicVolume;
            music.volume = volume;
        }

        public bool IsPlaying()
        {
            return music.isPlaying;
        }

        public void PlayMusic()
        {
            music.Play();
        }

        public void PlayMusic(AudioClip clip)
        {
            music.clip = clip;
            music.Play();
        }

        public void SetMusic(AudioClip clip)
        {
            
            music.clip = clip;
            music.time = 0;
            
            music.Stop();

            musicName.text = music.clip.name;
        }

        public AudioClip GetMusic()
        {
            return music.clip;
        }

        public int GetSamples()
        {
            return music.timeSamples;
        }

        public float GetTime()
        {
            return music.time;
        }

        void Update()
        {
            samples = GetSamples();

            ShowMusicTime();
        }

        public float SampleToTime(float s)
        {
            if(music.clip == null) return 0;

            return ConvertUtils.SamplesToTime(s, music.clip.frequency);
        }

        public float TimeToSample(float time)
        {
            if (music.clip == null) return 0;

            return time * music.clip.frequency;
        }

        
        private void ShowMusicTime()
        {
            //musicName.text = music.clip.name;
            musicTime.text = ComputeUtility.FormatTwoTime((int)music.time) + ":" + ComputeUtility.FormatTwoTime((int)music.clip.length);
        }
    }
}



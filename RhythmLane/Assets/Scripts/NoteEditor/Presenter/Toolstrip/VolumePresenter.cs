﻿using NoteEditor.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class VolumePresenter : MonoBehaviour
    {
        [SerializeField]
        Slider volumeController = default;
        [SerializeField]
        Image image = default;
        [SerializeField]
        Sprite iconSound2 = default;
        [SerializeField]
        Sprite iconSound = default;
        [SerializeField]
        Sprite iconMute = default;

        void Awake()
        {
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            volumeController.OnValueChangedAsObservable().Subscribe(volume => Audio.Volume.Value = volume);
            Audio.Volume.DistinctUntilChanged().Subscribe(x => Audio.Source.volume = x);
            Audio.Volume.Select(volume => Mathf.Approximately(volume, 0f) ? iconMute : volume < 0.6f ? iconSound : iconSound2)
                .DistinctUntilChanged()
                .Subscribe(sprite => image.sprite = sprite);
        }
    }
}

﻿using NoteEditor.Model;
using System.IO;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class SettingWorkSpacePathPresenter : MonoBehaviour
    {
        [SerializeField]
        InputField workSpacePathInputField = default;
        [SerializeField]
        Text workSpacePathInputFieldText = default;
        [SerializeField]
        Color defaultTextColor = default;
        [SerializeField]
        Color invalidStateTextColor = default;

        void Awake()
        {
            workSpacePathInputField.OnValueChangedAsObservable()
                .Select(path => Directory.Exists(path))
                .Subscribe(exists => workSpacePathInputFieldText.color = exists ? defaultTextColor : invalidStateTextColor);

            workSpacePathInputField.OnValueChangedAsObservable()
                .Where(path => Directory.Exists(path))
                .Subscribe(path => Settings.WorkSpacePath.Value = path);

            Settings.WorkSpacePath.DistinctUntilChanged()
                .Subscribe(path => workSpacePathInputField.text = path);
        }
    }
}

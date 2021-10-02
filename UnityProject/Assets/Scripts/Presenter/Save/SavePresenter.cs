using NoteEditor.Model;
using NoteEditor.Utility;
using System.IO;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace NoteEditor.Presenter
{
    public class SavePresenter : MonoBehaviour
    {
        [SerializeField]
        Button saveButton = default;
        [SerializeField]
        Text messageText = default;
        [SerializeField]
        Color unsavedStateButtonColor = default;
        [SerializeField]
        Color savedStateButtonColor = Color.white;

        [SerializeField]
        GameObject saveDialog = default;
        [SerializeField]
        Button dialogSaveButton = default;
        [SerializeField]
        Button dialogDoNotSaveButton = default;
        [SerializeField]
        Button dialogCancelButton = default;
        [SerializeField]
        Text dialogMessageText = default;

        [SerializeField]
        Button exitButton = default;

        ReactiveProperty<bool> mustBeSaved = new ReactiveProperty<bool>();



        void Awake()
        {
            var editPresenter = EditNotesPresenter.Instance;

            //Back to main menu when pressing escape or exit button
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Merge(exitButton.OnClickAsObservable())
                .Subscribe(_ => /*Application.Quit()*/ TryBack());

            //Save when click ctrl+s
            var saveActionObservable = this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.S))
                .Merge(saveButton.OnClickAsObservable());

            mustBeSaved = Observable.Merge(
                    EditData.BPM.Select(_ => true),
                    EditData.OffsetSamples.Select(_ => true),
                    EditData.MaxBlock.Select(_ => true),
                    editPresenter.RequestForEditNote.Select(_ => true),
                    editPresenter.RequestForAddNote.Select(_ => true),
                    editPresenter.RequestForRemoveNote.Select(_ => true),
                    editPresenter.RequestForChangeNoteStatus.Select(_ => true),
                    Audio.OnLoad.Select(_ => false),
                    saveActionObservable.Select(_ => false))
                .SkipUntil(Audio.OnLoad.DelayFrame(1))
                .Do(unsaved => saveButton.GetComponent<Image>().color = unsaved ? unsavedStateButtonColor : savedStateButtonColor)
                .ToReactiveProperty();

            mustBeSaved.SubscribeToText(messageText, unsaved => unsaved ? "Not saved." : "");

            saveActionObservable.Subscribe(_ => Save());

            dialogSaveButton.AddListener(
                EventTriggerType.PointerClick,
                (e) =>
                {
                    mustBeSaved.Value = false;
                    saveDialog.SetActive(false);
                    Save();
                    //Application.Quit();
                    DoBack();
                });

            dialogDoNotSaveButton.AddListener(
                EventTriggerType.PointerClick,
                (e) =>
                {
                    mustBeSaved.Value = false;
                    saveDialog.SetActive(false);
                    //Application.Quit();
                    DoBack();
                });

            dialogCancelButton.AddListener(
                EventTriggerType.PointerClick,
                (e) =>
                {
                    saveDialog.SetActive(false);
                });

            //Application.wantsToQuit += Back;
        }

        public void TryBack()
        {
            if (mustBeSaved.Value)
            {
                dialogMessageText.text = "Do you want to save the changes you made in the note?"
                    + EditData.Name.Value + "' ?" + System.Environment.NewLine
                    + "Your changes will be lost if you don't save them.";
                saveDialog.SetActive(true);
                
            }
            else
            {
                DoBack();
            }
            
        }

        void DoBack()
        {
            SceneManager.LoadScene(0);
        }

        public void Save()
        {
            var fileName = Path.ChangeExtension(EditData.Name.Value, "json");
            var directoryPath = Path.Combine(Path.GetDirectoryName(MusicSelector.DirectoryPath.Value), "Notes");
            var filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = EditDataSerializer.Serialize();
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
            messageText.text = filePath + " saved.";
        }
    }
}

using NoteEditor.Model;
using NoteEditor.Notes;
using System.Collections;
using System.IO;
using UniRx;
using UnityEngine;

using UnityEngine.Networking;

namespace NoteEditor.Presenter
{
    public class MusicLoader : MonoBehaviour
    {
        void Awake()
        {
            ResetEditor();
        }

        public void Load(string fileName)
        {
            StartCoroutine(LoadMusic(fileName));
        }

        IEnumerator LoadMusic(string fileName)
        {
            using(var _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(Path.Combine(MusicSelector.DirectoryPath.Value, fileName), AudioType.WAV)){
                
                yield return _unityWebRequest.SendWebRequest();

                EditCommandManager.Clear();
                ResetEditor();
                
                AudioClip clip = null;

                // if (_unityWebRequest.isHttpError || _unityWebRequest.isNetworkError)
                if(_unityWebRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(_unityWebRequest.error.ToString());
                }
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
                    Audio.Source.clip = clip;

                    EditData.Name.Value = fileName;
                    LoadEditData();
                    Audio.OnLoad.OnNext(Unit.Default);
                }

            }
            
            /*
            using (var www = new WWW("file:///" + Path.Combine(MusicSelector.DirectoryPath.Value, fileName)))
            {
                yield return www;

                EditCommandManager.Clear();
                ResetEditor();
                Audio.Source.clip = www.GetAudioClip();

                if (Audio.Source.clip == null)
                {
                    // TODO: 読み込み失敗時の処理
                }
                else
                {
                    EditData.Name.Value = fileName;
                    LoadEditData();
                    Audio.OnLoad.OnNext(Unit.Default);
                }
            }
            */
        }

        void LoadEditData()
        {
            var fileName = Path.ChangeExtension(EditData.Name.Value, "json");
            //var directoryPath = Path.Combine(Path.GetDirectoryName(MusicSelector.DirectoryPath.Value), "Notes");
            var directoryPath = MusicSelector.DirectoryPath.Value;
            var filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                EditDataSerializer.Deserialize(json);
            }
            else
            {
                Debug.LogError("Edit data does not exist!");

            }
        }

        public void ResetEditor()
        {
            Audio.TimeSamples.Value = 0;
            Audio.SmoothedTimeSamples.Value = 0;
            Audio.IsPlaying.Value = false;
            Audio.Source.clip = null;
            EditState.NoteType.Value = NoteTypes.Single;
            EditState.LongNoteTailPosition.Value = NotePosition.None;
            EditData.BPM.Value = 120;
            EditData.OffsetSamples.Value = 0;
            EditData.Name.Value = "Note Editor";
            EditData.MaxBlock.Value = Settings.MaxBlock;
            EditData.LPB.Value = 4;

            foreach (var note in EditData.Notes.Values)
            {
                note.Dispose();
            }

            EditData.Notes.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}

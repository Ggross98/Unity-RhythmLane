using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using NoteEditor.DTO;
using NoteEditor.Notes;

namespace Game.MusicSelect
{
    public class GameMusicSelector : MonoBehaviour
    {
        public Text pathText, pathSpaceHolder;

        private string path;

        [SerializeField]private Transform contentField;

        List<GameListItem> items = new List<GameListItem>();

        [SerializeField]private GameObject itemPrefab;

        public Text pathInfo, musicInfo, fileInfo;

        public Text bpm, time, notes;

        [SerializeField]private AudioSource audio;

        private bool notesLoaded = false, musicLoaded = false;

        void Start()
        {
            //默认路径：游戏目录中的Notes文件夹
            pathSpaceHolder.text = System.Environment.CurrentDirectory + "\\Notes";

        }

        public void LoadJson()
        {
            if (pathText.text.Equals(""))
                path = pathSpaceHolder.text;
            else path = pathText.text;

            LoadJsonInDirectory(path);
        }

        public void Quit()
        {
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 显示指定路径中的json文件，并创建对应的按钮
        /// </summary>
        /// <param name="path">文件路径</param>
        private void LoadJsonInDirectory(string path)
        {
            ContentClear();

            Debug.Log(path);
            

            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                FileInfo[] files = directory.GetFiles();

                for(int i = 0; i < files.Length; i++)
                {
                    string fileName = files[i].Name;

                    if (!fileName.EndsWith(".json")) continue;

                    Debug.Log(fileName);

                    CreateListItem(fileName);


                }

                ShowLoadInfo(0, "Path is loaded.");
            }
            else
            {
                ShowLoadInfo(0, "Path does not exist!");
            }

            

        }





        /// <summary>
        /// 根据按钮的数据，生成json，读取音乐和谱面信息
        /// </summary>
        /// <param name="item"></param>
        public void LoadMusicInfo(GameListItem item)
        {
            if (item == null || !items.Contains(item))
            {
                ShowLoadInfo(1, "Incorrect Json file!");
                return;
            }

            notesLoaded = false;
            musicLoaded = false;

            //读取谱面信息
            string name = Path.ChangeExtension(item.fileName, ".json");
            string dir = Path.Combine(Path.GetDirectoryName(path), "Notes");
            string newpath = Path.Combine(dir, name);

            string json = File.ReadAllText(newpath, System.Text.Encoding.UTF8);

            LoadMusicInfoFromJson(json);

            //加载音乐
            StartCoroutine(LoadMusic(item.fileName));
        }

        private IEnumerator LoadMusic(string n)
        {
            ShowLoadInfo(2, "Music loading...");

            string name = Path.ChangeExtension(n, ".wav");
            string dir = Path.Combine(Path.GetDirectoryName(path), "Music");
            string newpath = Path.Combine(dir, name);

            WWW www = new WWW(newpath);

            yield return www;

            AudioClip clip = www.GetAudioClip();

            if(clip == null)
            {
                ShowLoadInfo(2, "Music loaded incorrectly!");
            }
            else
            {
                ShowLoadInfo(2, "Music loaded.");


                time.text = "Time\t\t" + ComputeUtility.FormatTwoTime((int)clip.length);

                audio.clip = clip;
                audio.Play();

                clip.name = n;

                NotesContainer.Instance.music = clip;

                musicLoaded = true;
            }

            yield return null;
        }


        /// <summary>
        /// 根据一个json文件读取音乐及其信息
        /// </summary>
        /// <param name="json"></param>
        private void LoadMusicInfoFromJson(string json)
        {
            var editData = JsonUtility.FromJson<MusicDTO.EditData>(json);

            if(editData == null)
            {
                ShowLoadInfo(1, "Incorrect Json file!");
                return;
            }

            ShowLoadInfo(1, "Json loaded!");
            notesLoaded = true;

            bpm.text = "BPM\t\t" + editData.BPM;
            notes.text = "Notes\t" + editData.notes.Count;

            NotesContainer.Instance.json = json;

        }



        public static void Deserialize(string json)
        {
            var editData = UnityEngine.JsonUtility.FromJson<MusicDTO.EditData>(json);
            //var notePresenter = EditNotesPresenter.Instance;

            /*EditData.BPM.Value = editData.BPM;
            EditData.MaxBlock.Value = editData.maxBlock;
            EditData.OffsetSamples.Value = editData.offset;*/




            /*
            foreach (var note in editData.notes)
            {
                if (note.type == 1)
                {
                    notePresenter.AddNote(ToNoteObject(note));
                    continue;
                }

                var longNoteObjects = new[] { note }.Concat(note.notes)
                    .Select(note_ =>
                    {
                        notePresenter.AddNote(ToNoteObject(note_));
                        return EditData.Notes[ToNoteObject(note_).position];
                    })
                    .ToList();

                for (int i = 1; i < longNoteObjects.Count; i++)
                {
                    longNoteObjects[i].note.prev = longNoteObjects[i - 1].note.position;
                    longNoteObjects[i - 1].note.next = longNoteObjects[i].note.position;
                }

                EditState.LongNoteTailPosition.Value = NotePosition.None;
            }*/
        }

        private void ContentClear()
        {
            for(int i = 0; i < items.Count; i++)
            {
                GameListItem item = items[i];
                items.Remove(item);

                Destroy(item.gameObject);
            }
        }

        private void CreateListItem(string f)
        {

            string fileName = f.Remove(f.LastIndexOf("."));

            GameObject obj = Instantiate(itemPrefab, contentField.transform);

            GameListItem item = obj.GetComponent<GameListItem>();
            item.SetName(fileName);

            items.Add(item);
        }

        private void ShowLoadInfo(int index, string info)
        {
            switch (index)
            {
                case 0:
                    pathInfo.text = info;
                    break;
                case 1:
                    fileInfo.text = info;
                    break;
                case 2:
                    musicInfo.text = info;
                    break;
            }
        }

        public void StartGame()
        {
            if(notesLoaded && musicLoaded)
            {
                DontDestroyOnLoad(NotesContainer.Instance.gameObject);

                SceneManager.LoadScene("Game");
            }
            else
            {
                Debug.LogError("File not fully loaded!");
            }
        }

    }
}

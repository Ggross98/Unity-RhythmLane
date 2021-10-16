using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using NoteEditor.DTO;
using NoteEditor.Notes;


//using Ggross.Utils;

namespace Game.MusicSelect
{
    public class MusicSelector : MonoBehaviour
    {
        [SerializeField]private Text pathText, pathSpaceHolder;

        private string path;

        [SerializeField]private Transform contentField;

        private List<ListItem> items = new List<ListItem>();

        [SerializeField] private GameObject itemPrefab;

        [SerializeField] private Text pathInfo, musicInfo, fileInfo;

        [SerializeField] private Text bpm, time, notes;

        [SerializeField] private AudioSource audioSource;

        private bool notesLoaded = false, musicLoaded = false;
        [SerializeField] private Button startButton, autoButton;


        void Awake()
        {
            startButton.onClick.AddListener(() => {
                StartGame(false);
            });
            startButton.interactable = false;

            autoButton.onClick.AddListener(() => {
                StartGame(true);
            });
            autoButton.interactable = false;
        }
        void Start()
        {
            //默认路径：游戏目录中的Notes文件夹

            var p = PlayerSettings.Instance.musicPath;
            if(p.Length < 3)
                p = System.Environment.CurrentDirectory + "\\Notes";

            pathSpaceHolder.text = p;


        }

        void Update(){
            
            var ready = musicLoaded && notesLoaded;
            startButton.interactable = ready;
            autoButton.interactable = ready;

            if(Input.GetKeyDown(KeyCode.Escape))
                Quit();
        }

        /// <summary>
        /// 从文本栏的地址中读取所有json文件并生成按钮
        /// </summary>
        public void LoadJson()
        {
            if (pathText.text.Equals(""))
                path = pathSpaceHolder.text;
            else path = pathText.text;

            var load = LoadJsonInDirectory(path);
            if (load == 1)
                PlayerSettings.Instance.SetMusicPath(path);
        }

        public void Quit()
        {
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 显示指定路径中的json文件，并创建对应的按钮
        /// </summary>
        /// <param name="path">文件路径</param>
        private int LoadJsonInDirectory(string path)
        {
            ContentClear();
            notesLoaded = false;
            musicLoaded = false;

            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                FileInfo[] files = directory.GetFiles();

                for(int i = 0; i < files.Length; i++)
                {
                    string fileName = files[i].Name;

                    if (!fileName.EndsWith(".json")) continue;
                    
                    CreateListItem(fileName);

                }

                ShowLoadInfo(pathInfo, "Path is loaded.");
                return 1;
            }
            else
            {
                ShowLoadInfo(pathInfo, "Path does not exist!");
                return 0;
            }
        }

        /// <summary>
        /// 根据按钮的数据，读取.wav音乐和.json文件存储的谱面信息
        /// </summary>
        /// <param name="item"></param>
        public void LoadMusicInfo(ListItem item)
        {
            if (item == null || !items.Contains(item))
            {
                ShowLoadInfo(fileInfo, "Incorrect Json file!");
                return;
            }

            notesLoaded = false;
            musicLoaded = false;

            //读取谱面信息
            var name = Path.ChangeExtension(item.fileName, ".json");
            var dir = path;
            //string dir = Path.Combine(Path.GetDirectoryName(path), "Notes");
            string jsonPath = Path.Combine(dir, name);

            string json = File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);

            LoadMusicInfoFromJson(json);

            //加载音乐
            StartCoroutine(LoadMusic(item.fileName));
        }

        //读取.wav音乐文件的携程
        private IEnumerator LoadMusic(string n)
        {
            ShowLoadInfo(musicInfo, "Music loading...");

            var name = Path.ChangeExtension(n, ".wav");
            var dir = path;
            //string dir = Path.Combine(Path.GetDirectoryName(path), "Music");
            string musicPath = Path.Combine(dir, name);

            /*
            WWW www = new WWW(musicPath);
            yield return www;
            AudioClip clip = www.GetAudioClip();
            */

            var _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(musicPath, AudioType.WAV);
            AudioClip clip = null;

            yield return _unityWebRequest.SendWebRequest();

            if (_unityWebRequest.isHttpError || _unityWebRequest.isNetworkError)
            {
                Debug.Log(_unityWebRequest.error.ToString());
            }
            else
            {
                clip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
            }

            
            if(clip == null)
            {
                ShowLoadInfo(musicInfo, "Music loaded incorrectly!");
            }
            else
            {
                ShowLoadInfo(musicInfo, "Music loaded.");


                time.text = "Time\t\t" + ComputeUtility.FormatTwoTime((int)clip.length);

                audioSource.clip = clip;
                audioSource.Play();

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
                ShowLoadInfo(fileInfo, "Incorrect Json file!");
                return;
            }

            ShowLoadInfo(fileInfo, "Json loaded!");
            notesLoaded = true;

            bpm.text = "BPM\t\t" + editData.BPM;
            notes.text = "Notes\t" + editData.notes.Count;

            NotesContainer.Instance.json = json;

        }



        public static void Deserialize(string json)
        {
            var editData = UnityEngine.JsonUtility.FromJson<MusicDTO.EditData>(json);
        }

        private void ContentClear()
        {
            for(int i = 0; i < items.Count; i++)
            {
                ListItem item = items[i];
                items.Remove(item);

                Destroy(item.gameObject);
            }
        }

        private void CreateListItem(string f)
        {

            string fileName = f.Remove(f.LastIndexOf("."));

            GameObject obj = Instantiate(itemPrefab, contentField.transform);

            ListItem item = obj.GetComponent<ListItem>();
            item.SetName(fileName);

            items.Add(item);
        }

        private void ShowLoadInfo(Text text, string info)
        {
            text.text = info;
            /*
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
            */
        }

        public void StartGame(bool autoplay)
        {   
            if(notesLoaded && musicLoaded)
            {
                NotesContainer.Instance.autoplay = autoplay;

                SceneManager.LoadScene("Game");
            }
            else
            {
                Debug.LogError("File not fully loaded!");
            }
        }

    }
}

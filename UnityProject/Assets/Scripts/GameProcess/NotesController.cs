using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoteEditor.Utility;
using NoteEditor.Notes;
using NoteEditor.DTO;
using Game.MusicSelect;

using DG.Tweening;

namespace Game.Process
{
    public class NotesController : SingletonMonoBehaviour<NotesController>
    {
        [SerializeField]
        public float maxY = 1120, minY = -40; //音符显示的最高和最低位置，低于最低位置的音符会被回收

        [SerializeField] private GameObject notePrefab, holdPrefab;
        //[SerializeField] private Transform noteParent;


        [SerializeField]
        Image targetLine, clickLine;

        //Click Field：音符进入此区域时，单击对应键才会进行判定
        [HideInInspector] public float targetY, clickFieldY;

        List<MusicDTO.Note> notesInfo;//音符信息

        List<GameNote> notesInScreen = new List<GameNote>();//激活的音符对象
        List<GameNote> notesWaiting = new List<GameNote>(); //等待激活的音符对象
        List<GameNote> notesOver = new List<GameNote>(); //激活结束的音符对象

        [SerializeField]
        public int notesPerFrame = 50; //每一帧最多生成的音符数量

        [HideInInspector]public int laneCount; //轨道数量

        [HideInInspector] public int BPM; //速度

        [HideInInspector] public int offset; //开始的偏移时间
        [HideInInspector] public float startTime = 2f; //开始游戏前等待时间
        [HideInInspector] public int playerOffset; //玩家调整的延迟


        //private float preOffset; //为了让第一个音符从顶部落下的偏移时间

        [HideInInspector] public float frequency; //音乐频率
        [HideInInspector] public float length; //音乐时长
        [HideInInspector] public float playTime = 0f; //音乐播放了多久

        private bool playing = false; //播放中
        private bool loading = false; //加载中
        private bool ready = false; //加载完毕


        [HideInInspector] public float noteMoveTime = 1.5f; //从出现到落到位置需要多久
        //public float noteWaitTime = 5f;   //在开始移动前多久生成

        private float noteMoveToEndTime;
        private float noteMoveY;

        //public AudioSource audioMusic;

        //自动播放
        public bool debugMoving = false;

        private AudioClip defaultClip;
        //private 

        //public AudioClip clap;

        //private List<NoteParent> parentWaiting = new List<NoteParent>();
        //private List<NoteParent> parentInScreen = new List<NoteParent>();
        

        [SerializeField] private Transform noteField;

        void Awake()
        {


            #region 获取判定点、判定生效区的位置
            if (targetLine == null)
            {
                Debug.Log("Target lost!");
                return;
            }

            targetY = targetLine.rectTransform.anchoredPosition.y;
            clickFieldY = clickLine.rectTransform.anchoredPosition.y;
            #endregion

            #region  获取音符下落速度信息
            int speed = PlayerSettings.Instance.speed;

            noteMoveTime = 2.5f - 0.02f * speed;
            noteMoveToEndTime = noteMoveTime * (maxY - minY) / (maxY - targetY);
            noteMoveY = (maxY - targetY) / noteMoveTime;
            #endregion

            #region  判断是否加载乐谱
            if (NotesContainer.Instance == null || NotesContainer.Instance.json == null)
            {
                Debug.LogError("Data not loaded!");
                return;
            }
            #endregion

            #region 读取乐谱信息
            var editData = JsonUtility.FromJson<MusicDTO.EditData>(NotesContainer.Instance.json);

            notesInfo = editData.notes;
            laneCount = editData.maxBlock;
            BPM = editData.BPM;
            offset = editData.offset;
            //Debug.Log("offset " + offset);
            #endregion

            #region 判断是否加载音乐
            if (NotesContainer.Instance.music == null)
            {
                Debug.LogError("Music not loaded!");
                return;
            }
            #endregion

            #region  加载音乐信息
            //audioMusic.clip = NotesContainer.Instance.music;

            frequency = NotesContainer.Instance.music.frequency;
            length = NotesContainer.Instance.music.length;
            #endregion

            playerOffset = PlayerSettings.Instance.offset;


        }

        void Start()
        {

            StartGame();

        }

        public void StartGame()
        {
            playing = false;
            loading = true;
            ready = false;
            

            ResetMusic();



            //GameNotePool.Instance.Init();

            PlayController.Instance.Init(laneCount);

            LaneController.Instance.CreateLanes(laneCount);

         
            StartCoroutine(GenerateNotes());            
            //playing = true;
            //audioMusic.Play();

        }

        private void ResetMusic()
        {
            
            MusicController.Instance.SetMusic(NotesContainer.Instance.music);
        }

        private void ClearNotes()
        {
            if (notesWaiting.Count > 0)
            {
                foreach (GameNote gn in notesWaiting)
                {
                    Destroy(gn.gameObject);
                }
            }
            notesWaiting.Clear();

            if (notesInScreen.Count > 0)
            {
                foreach (GameNote gn in notesInScreen)
                {
                    Destroy(gn.gameObject);
                }
            }
            notesInScreen.Clear();

            if (notesOver.Count > 0)
            {
                foreach (GameNote gn in notesOver)
                {
                    Destroy(gn.gameObject);
                }
            }
            notesOver.Clear();
        }

        private GameNote CreateNote()
        {
            GameObject obj = Instantiate(notePrefab, noteField);
            obj.SetActive(true);
            //noteList.Add(obj);
            return obj.GetComponent<GameNote>();
        }

        private IEnumerator GenerateNotes()
        {
            GameNotePool pool = GameNotePool.Instance;
            ClearNotes();
                        
            for (int i = 0 ; i < notesInfo.Count; i++)
            {
                //生成音符对象
                //GameNote gn = pool.GetNote(notesInfo[i]);
                var dto = notesInfo[i];

                var gn = CreateNote();
                gn.Init(dto);
                notesWaiting.Add(gn);

                gn.transform.SetParent(noteField);
                gn.transform.localScale = Vector3.one;

                //音符对象的时间
                gn.time = ConvertUtils.NoteToSamples(gn, frequency, BPM) + playerOffset;

                //音符对象的位置          
                float x = LaneController.Instance.GetLaneX(gn.Block());
                float y = targetY + noteMoveY *  MusicController.Instance.SampleToTime(gn.time + offset);

                //RectTransform rt = gn.GetComponent<RectTransform>();
                //rt.anchoredPosition = new Vector2(x,y);
                gn.SetPosition(new Vector2(x, y));

                
            }

            /*
            if(notesWaiting[0].GetPosition().y < maxY)
            {
                float dY = maxY - notesWaiting[0].GetPosition().y;

                preOffset = dY / noteMoveY;

                Vector2 pos = new Vector2(0, dY);

                notesField.GetComponent<RectTransform>().anchoredPosition = pos;


                Tweener tweener = notesField.GetComponent<RectTransform>().DOAnchorPosY(0, preOffset);

                yield return new WaitForSeconds(preOffset);

            }
            */

            //playing = true;

            //让音符提前下落，在正确的开始时间到达位置
            RectTransform rect = noteField.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, noteMoveY * startTime);
            
            Tweener tweener = rect.DOAnchorPosY(0, startTime);
            tweener.SetEase(Ease.Linear);
            tweener.OnComplete(()=>
            {
                ready = true;
            });
            //yield return new WaitForSeconds(startTime - Time.deltaTime);


            yield return null;
            
        }

        void Update()
        {
            if(loading && ready)
            {
                playing = true;
                loading = false;

                //audioMusic.Play();
                MusicController.Instance.PlayMusic();
                Tweener tweener = noteField.GetComponent<RectTransform>().DOAnchorPosY(-noteMoveY * length, length);
                tweener.SetEase(Ease.Linear);
            }


            if (!playing) return;

            

            //int min = Mathf.Min(notesPerFrame, notesInfo.Count);

            //Debug.Log("frequency: " + frequency);

            //List<RectTransform> newnotes = new List<RectTransform>(); //待移动音符

            //for(int i = 0; i < notesInfo.Count; i++)

            #region  old logics
            /*
            //激活音符
            if (notesInfo.Count > 0)
            {
                MusicDTO.Note note = notesInfo[0];


                //Debug.Log(note.num + ", " + note.LPB);

                //float time = ConvertUtils.NoteToSamples(note, frequency, BPM);
                //Debug.Log("note"+i+" "+time);

                playTime = audioMusic.time;

                if (playTime >= ConvertUtils.NoteToSamples(note, frequency, BPM) + offset - noteMoveTime - noteWaitTime)
                {
                    //读取或创建音符父对象
                    NoteParent np = NoteParentPool.Instance.GetNoteParent(note.num).GetComponent<NoteParent>();
                    np.time = ConvertUtils.NoteToSamples(note, frequency, BPM);

                    parentWaiting.Add(np);

                    while (note != null && ConvertUtils.NoteToSamples(note, frequency, BPM) + offset - noteMoveTime - noteWaitTime <= playTime)
                    {
                        //获得音符
                        GameObject noteObj = GameNotePool.Instance.GetNote(note.num);
                        GameNote n = noteObj.GetComponent<GameNote>();

                        noteObj.transform.parent = np.transform;

                        n.Init(note);
                        //n.time = time;
                        float x = LaneController.Instance.GetLaneX(note.block);
                        Vector2 startPos = new Vector2(x, 0);
                        n.SetPosition(startPos);
                        noteObj.SetActive(true);
                        notesWaiting.Add(n);


                        notesInfo.Remove(note);
                        if (notesInfo.Count > 0) note = notesInfo[0];
                        else note = null;

                        playTime = audioMusic.time;

                    }
                }

                //np.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, maxY);

                //n.StartMove(noteMoveY);
                //Vector2 endPos = new Vector2(x, minY);
                //newnotes.Add(n.GetComponent<RectTransform>());

                //Tweener tweener = n.GetComponent<RectTransform>().DOAnchorPos(endPos, noteMoveToEndTime-Time.deltaTime);
                //tweener.SetEase(Ease.Linear);

                //min--;
            }
            
            /*
            if (newnotes.Count > 0)
            {
                foreach(RectTransform rt in newnotes)
                {
                    Tweener tweener = rt.DOAnchorPosY(minY, noteMoveToEndTime - Time.deltaTime);
                    tweener.SetEase(Ease.Linear);
                }
            }*/

            /*
            for(int i = 0; i < parentWaiting.Count; i++)
            {
                GameNote n = notesWaiting[i];
                if(playTime >= n.time  + offset - noteMoveTime)
                {
                    Debug.Log(n.name+"start move");
                    //n.StartMove(noteMoveY);

                    /*RectTransform rt = n.GetComponent<RectTransform>();
                    Tweener tweener = rt.DOAnchorPosY(minY, noteMoveToEndTime - Time.deltaTime);
                    tweener.SetEase(Ease.Linear);

                    notesWaiting.Remove(n);
                    notesInScreen.Add(n);
                }*/
            /*
            NoteParent parent = parentWaiting[i];

            playTime = audioMusic.time;

            if (playTime >= parent.time + offset - noteMoveTime)
            {
                RectTransform rt = parent.GetComponent<RectTransform>();
                Tweener tweener = rt.DOAnchorPosY(minY, noteMoveToEndTime - Time.deltaTime);
                tweener.SetEase(Ease.Linear);

                parentWaiting.Remove(parent);
                parentInScreen.Add(parent);

                for(int j = 0; j < notesWaiting.Count; j++)
                {
                    if(notesWaiting[j].GetComponent<GameNote>().note.num == parent.num)
                    {
                        notesInScreen.Add(notesWaiting[j]);
                        notesWaiting.RemoveAt(j);
                    }
                }
            }
        }*/


            //float deltaY = noteMoveY * Time.deltaTime;
            /*

            //把落到底部的音符父对象回收
            for (int i = 0; i < parentInScreen.Count; i++)
            {
                NoteParent parent = parentInScreen[i];


                if (parent.GetComponent<RectTransform>().anchoredPosition.y <= minY)
                {
                    foreach (Transform child in parent.transform)
                    {
                        GameNote note = child.GetComponent<GameNote>();

                        if (note != null)
                        {
                            note.gameObject.SetActive(false);
                            notesInScreen.Remove(note);
                        }
                    }

                    parent.gameObject.SetActive(false);
                    parentInScreen.Remove(parent);
                }
            }*/

            /*
            //回收音符
            for (int i = 0; i < notesInScreen.Count; i++)
            {
                GameNote note = notesInScreen[i];

                if (note.gameObject.activeSelf == false)
                {
                    note.transform.parent = NoteParentPool.Instance.noteField;
                    notesInScreen.Remove(note);
                }

            }
            */
            #endregion

            if (notesWaiting.Count > 0)
            {
                if (notesWaiting[0].GetPosition().y + noteField.GetComponent<RectTransform>().anchoredPosition.y < clickFieldY)
                {
                    GameNote gn = notesWaiting[0];
                    notesWaiting.Remove(gn);
                    notesInScreen.Add(gn);

                    PlayController.Instance.NoteEnqueue(gn);

                    //Destroy(gn.gameObject);
                }
            }


            if (notesInScreen.Count > 0)
            {
                if (notesInScreen[0].GetPosition().y + noteField.GetComponent<RectTransform>().anchoredPosition.y < minY)
                {
                    GameNote gn = notesInScreen[0];
                    notesInScreen.Remove(gn);
                    notesOver.Add(gn);
                    //Destroy(gn.gameObject);
                }
            }
            
            //playTime += Time.deltaTime;
            playTime = MusicController.Instance.GetTime();
            //GameUI.Instance.ShowMusicTime((int)playTime, (int)length);

            if(!MusicController.Instance.IsPlaying())
            {
                playing = false;

                ComboPresenter.Instance.ShowResult();
            }

        }


    }



}


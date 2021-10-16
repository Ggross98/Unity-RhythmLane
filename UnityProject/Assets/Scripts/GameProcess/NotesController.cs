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
        private float maxY = 1120, minY = -40; //音符显示的最高和最低位置，低于最低位置的音符会被回收

        [SerializeField] private GameObject notePrefab, holdNotePrefab, holdBarPrefab;
        //[SerializeField] private Transform noteParent;


        [SerializeField]
        Image targetLine, clickLine;

        //Click Field：音符进入此区域时，单击对应键才会进行判定
        [HideInInspector] 
        public float targetY, clickFieldY;

        private List<MusicDTO.Note> notesInfo;//音符信息

        //private List<NoteObject> notesInScreen = new List<NoteObject>();//激活的音符对象
        private List<NoteObject> notesWaiting = new List<NoteObject>(); //等待激活的音符对象
        //List<NoteObject> notesOver = new List<NoteObject>(); //激活结束的音符对象

        [SerializeField]
        public int notesPerFrame = 50; //每一帧最多生成的音符数量

        [HideInInspector] public int laneCount; //轨道数量

        [HideInInspector] public int BPM; //速度

        [HideInInspector] public int offset; //开始的偏移时间
        [HideInInspector] public float startTime = 2f; //开始游戏前等待时间
        [HideInInspector] public int playerOffset; //玩家调整的延迟


        //private float preOffset; //为了让第一个音符从顶部落下的偏移时间

        [HideInInspector] public float frequency; //音乐频率
        [HideInInspector] public float length; //音乐时长
        //[HideInInspector] public float playTime = 0f; //音乐播放了多久

        private bool playing = false; //播放中
        private bool loading = false; //加载中
        private bool ready = false; //加载完毕


        [HideInInspector] public float noteMoveTime = 1.5f; //从出现到落到位置需要多久
        //public float noteWaitTime = 5f;   //在开始移动前多久生成

        private float noteMoveToEndTime;
        private float noteMoveY;

        //public AudioSource audioMusic;

        //自动播放
        //public bool debugMoving = false;

        //private AudioClip defaultClip;
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

            noteMoveTime = 2.5f - 0.021f * speed;
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

            #region 加载玩家设定
            playerOffset = PlayerSettings.Instance.playerOffset;
            #endregion


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
                foreach (NoteObject gn in notesWaiting)
                {
                    Destroy(gn.gameObject);
                }
            }
            notesWaiting.Clear();

            /*
            if (notesInScreen.Count > 0)
            {
                foreach (NoteObject gn in notesInScreen)
                {
                    Destroy(gn.gameObject);
                }
            }
            notesInScreen.Clear();
            */

            /*
            if (notesOver.Count > 0)
            {
                foreach (NoteObject gn in notesOver)
                {
                    Destroy(gn.gameObject);
                }
            }
            notesOver.Clear();
            */
        }

        private NoteObject CreateNote(int type)
        {
            var prefab = (type == 1) ? notePrefab : holdNotePrefab;

            GameObject obj = Instantiate(prefab);
            obj.SetActive(true);
            //noteList.Add(obj);
            return obj.GetComponent<NoteObject>();
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
                var type = dto.type;

                var n = CreateNote(type);
                n.Init(dto);

                notesWaiting.Add(n);

                n.transform.SetParent(noteField);
                n.transform.localScale = Vector3.one;

                //音符对象的时间
                n.time = ConvertUtils.NoteToSamples(n, frequency, BPM) + playerOffset;

                //音符对象的位置          
                float x = LaneController.Instance.GetLaneX(n.Block());
                float y = targetY + noteMoveY *  MusicController.Instance.SampleToTime(n.time + offset);
                n.SetPosition(new Vector2(x, y));

                //对于长按音符，生成结尾音符和hold条
                if(type == 2)
                {
                    if(dto.notes.Count == 0)
                    {
                        Debug.Log("Hold key has no ending!");
                    }
                    else
                    {
                        var endDto = dto.notes[0];

                        var nn = CreateNote(2);
                        nn.Init(endDto);
                        //notesWaiting.Add(nn);

                        nn.transform.SetParent(noteField);
                        nn.transform.localScale = Vector3.one;

                        //音符对象的时间
                        nn.time = ConvertUtils.NoteToSamples(nn, frequency, BPM) + playerOffset;

                        //音符对象的位置          
                        float xx = LaneController.Instance.GetLaneX(nn.Block());
                        float yy = targetY + noteMoveY * MusicController.Instance.SampleToTime(nn.time + offset);
                        nn.SetPosition(new Vector2(xx, yy));

                        //生成条带
                        var bar = Instantiate(holdBarPrefab, noteField).GetComponent<HoldingBar>();
                        bar.transform.SetAsFirstSibling();
                        bar.SetPosition(x, y);
                        bar.SetHeight(yy - y);

                        //将尾判音符和长条与头部音符绑定
                        n.AddChainedNote(nn);
                        n.AddHoldingBar(bar);
                        nn.AddHoldingBar(bar);


                    }

                }

                
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
            #region 加载完成后，音符开始下落
            if(loading && ready)
            {
                playing = true;
                loading = false;

                //audioMusic.Play();
                MusicController.Instance.PlayMusic();
                Tweener tweener = noteField.GetComponent<RectTransform>().DOAnchorPosY(-noteMoveY * length, length);
                tweener.SetEase(Ease.Linear);
            }
            #endregion

            if (!playing) return;

            /*
            #region 将进入一定距离的音符加入判定队列
            if (notesWaiting.Count > 0)
            {
                if (notesWaiting[0].GetPosition().y + noteField.GetComponent<RectTransform>().anchoredPosition.y < clickFieldY)
                {
                    NoteObject gn = notesWaiting[0];
                    notesWaiting.Remove(gn);
                    notesInScreen.Add(gn);

                    PlayController.Instance.NoteEnqueue(gn);
                    //Destroy(gn.gameObject);
                }
            }
            #endregion
            */

            #region 将所有音符加入判定队列中
            while(notesWaiting.Count > 0){
                var gn = notesWaiting[0];
                notesWaiting.Remove(gn);
                PlayController.Instance.NoteEnqueue(gn);

                if(gn.Type() == 2){
                    var cn = gn.GetChainedNote();
                    PlayController.Instance.NoteEnqueue(cn);
                }
            }
            #endregion

            //playTime = MusicController.Instance.GetTime();
            
            /*
            if (notesInScreen.Count > 0)
            {
                if (notesInScreen[0].GetPosition().y + noteField.GetComponent<RectTransform>().anchoredPosition.y < minY)
                {
                    NoteObject gn = notesInScreen[0];
                    notesInScreen.Remove(gn);
                    notesOver.Add(gn);
                    //Destroy(gn.gameObject);
                }
            }*/
            
            
            #region 音乐播放完成后，结束游戏
            if(!MusicController.Instance.IsPlaying())
            {
                playing = false;

                ComboPresenter.Instance.ShowResult();
            }
            #endregion

        }


    }



}


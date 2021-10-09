using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NoteEditor.Utility;


namespace Game.Process
{
    /// <summary>
    /// 管理键盘输入和音符判定
    /// </summary>
    public class PlayController : SingletonMonoBehaviour<PlayController>
    {

        [HideInInspector]public int laneCount;

        private List<Queue<NoteObject>> laneNotes = new List<Queue<NoteObject>>();

        private int offset;

        private int BPM, LPB;

        //private float playtime;

        private float D_PERFECT, D_GREAT, D_GOOD, D_BAD;

        public KeyCode KEY0 = KeyCode.A, KEY1 = KeyCode.S, KEY2 = KeyCode.D, KEY3 = KeyCode.F, KEY4 = KeyCode.G;

        private KeyCode[] keys;

        public bool autoplay;

        public void Init(int c)
        {
            laneCount = c;

            if (laneNotes.Count == laneCount)
            {
                for (int i = 0; i < laneCount; i++)
                {
                    laneNotes[i].Clear();
                }
            }
            else
            {
                laneNotes.Clear();
                for (int i = 0; i < laneCount; i++)
                {
                    laneNotes.Add(new Queue<NoteObject>());
                }
            }

            offset = NotesController.Instance.offset;
            BPM = NotesController.Instance.BPM;

            D_BAD = MusicController.Instance.TimeToSample(0.35f);
            D_GOOD = MusicController.Instance.TimeToSample(0.25f);
            D_GREAT = MusicController.Instance.TimeToSample(0.15f);
            D_PERFECT = MusicController.Instance.TimeToSample(0.08f);

            //Debug.Log("Perfect samples: " + D_PERFECT);

            //LPB = NotesController.Instance.offset;
            //offset = NotesController.Instance.offset;
            KEY0 = (KeyCode)(PlayerSettings.Instance.KEY0);
            KEY1 = (KeyCode)(PlayerSettings.Instance.KEY1);
            KEY2 = (KeyCode)(PlayerSettings.Instance.KEY2);
            KEY3 = (KeyCode)(PlayerSettings.Instance.KEY3);
            KEY4 = (KeyCode)(PlayerSettings.Instance.KEY4);

            keys = new KeyCode[] { KEY0, KEY1, KEY2, KEY3, KEY4 };

            //autoplay = true;
            if (autoplay) Debug.Log("Auto play mode");

        }

        public void Quit()
        {
            SceneManager.LoadScene("MusicSelect");
        }

        void Update()
        {
            //playtime = NotesController.Instance.playTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }


            if (MusicController.Instance.GetSamples() <= 0) return;

            for (int i = 0; i < laneCount; i++)
            {
                //ClickLane(i);
                TryEnqueue(i);
            }

            if (autoplay)
            {
                for (int i = 0; i < laneCount; i++)
                {
                    AutoClickLane(i);
                    
                }
            }
            else
            {
                
                for (int i = 0; i < laneCount; i++)
                {
                    if (Input.GetKeyDown(keys[i]))
                    {
                        ClickLane(i);
                    }
                }

            }




            
        }
        
        public void NoteEnqueue(NoteObject gn)
        {
            laneNotes[gn.Block()].Enqueue(gn);
        }

        //尝试将音符加入判定区
        private void TryEnqueue(int lane)
        {
            NoteObject gn;
            if (laneNotes[lane].Count > 0)
                gn = laneNotes[lane].Peek();
            else return;

            //如果该音符已经被错过，直接增加一个miss
            if(GetDeltaTime(gn) >= D_BAD)
            {
                ComboPresenter.Instance.Combo(-1);
                //Debug.Log(gn.name + " miss");
                laneNotes[lane].Dequeue().Miss();
            }
        }



        private void AutoClickLane(int lane)
        {
            NoteObject gn;
            if (laneNotes[lane].Count > 0)
                gn = laneNotes[lane].Peek();
            else return;

            if(Mathf.Abs(GetDeltaTime(gn)) < MusicController.Instance.TimeToSample(0.02f))
            {
                ClickLane(lane);
                Debug.Log("auto click");
            }
        }

        private void ClickLane(int lane)
        {
            if(PlayerSettings.Instance.clap == 1) SEPool.Instance.PlayClap();

            //获取最近的仍在判定区的音符
            NoteObject gn;
            if (laneNotes[lane].Count > 0)
                gn = laneNotes[lane].Peek();
            else return;

            //Debug.Log(GetDeltaTime(gn));

            var delta = Mathf.Abs(GetDeltaTime(gn));
            if ( delta < D_PERFECT)
            {
                //Debug.Log(gn.name + " perfect");
                ComboPresenter.Instance.Combo(0);
                laneNotes[lane].Dequeue().Click();
            }
            else if (delta < D_GREAT)
            {
                ComboPresenter.Instance.Combo(1);
                //Debug.Log(gn.name + " great");
                laneNotes[lane].Dequeue().Click();
            }
            else if (delta < D_GOOD)
            {
                ComboPresenter.Instance.Combo(2);
                //Debug.Log(gn.name + " good");
                laneNotes[lane].Dequeue().Click();
            }
            else if (delta < D_BAD)
            {
                ComboPresenter.Instance.Combo(3);
                //Debug.Log(gn.name + " bad");
                laneNotes[lane].Dequeue().Click();
            }
            else
            {
                ComboPresenter.Instance.Combo(-1);
                //Debug.Log(gn.name + " miss");
                laneNotes[lane].Dequeue().Miss();
            }

            

        }

        private float GetDeltaTime(NoteObject gn)
        {
            //Debug.Log(gn.num + ", " + ConvertUtils.NoteToSamples(gn.note, 1, BPM)+","+playtime);
            float d = (MusicController.Instance.GetSamples() - (gn.time + offset));
            //Debug.Log(gn.name + d);
            //Debug.Log("delta: "+d);
            return d;
        }


    

    }

}

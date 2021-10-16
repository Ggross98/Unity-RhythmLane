using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NoteEditor.Utility;
using Game.MusicSelect;


namespace Game.Process
{
    /// <summary>
    /// 管理键盘输入和音符判定
    /// </summary>
    public class PlayController : SingletonMonoBehaviour<PlayController>
    {

        [HideInInspector]public int laneCount;

        private List<Queue<NoteObject>> laneNotes = new List<Queue<NoteObject>>();
        private bool[] laneHolding;

        private int offset;

        private int BPM, LPB;

        //private float playtime;

        //private float D_PERFECT, D_GREAT, D_GOOD, D_BAD;
        private Judgement judgement;

        //public KeyCode KEY0 = KeyCode.A, KEY1 = KeyCode.S, KEY2 = KeyCode.D, KEY3 = KeyCode.F, KEY4 = KeyCode.G;
        //private KeyCode[] keys;

        private bool autoplay;

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

            laneHolding = new bool[laneCount];
            for(int i = 0; i< laneCount;i++)
                laneHolding[i] = false;
            

            offset = NotesController.Instance.offset;
            BPM = NotesController.Instance.BPM;

            judgement = new Judgement();
            judgement.SetSampleRange(0.1f, 0.2f, 0.3f, 0.4f);
            judgement.SetEarly(-0.6f);

            //LPB = NotesController.Instance.offset;
            //offset = NotesController.Instance.offset;
            /*
            KEY0 = (KeyCode)(PlayerSettings.Instance.KEY0);
            KEY1 = (KeyCode)(PlayerSettings.Instance.KEY1);
            KEY2 = (KeyCode)(PlayerSettings.Instance.KEY2);
            KEY3 = (KeyCode)(PlayerSettings.Instance.KEY3);
            KEY4 = (KeyCode)(PlayerSettings.Instance.KEY4);
            */

            //keys = new KeyCode[] { KEY0, KEY1, KEY2, KEY3, KEY4 };

            autoplay = NotesContainer.Instance.autoplay;
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
                TestMiss(i);

                if(autoplay)
                    LaneAutoPlay(i);
                else
                {
                    if (Input.GetKeyDown(PlayerSettings.Instance.GetKeyCode(i)))
                        LaneKeyDown(i);
                    
                    if(Input.GetKeyUp(PlayerSettings.Instance.GetKeyCode(i)))
                        LaneKeyUp(i);

                }
            }

        }
        
        public void NoteEnqueue(NoteObject gn)
        {
            if(gn == null)
                Debug.Log("Note object is null!");
            else if(gn.clicked)
                Debug.Log("Note object is already clicked!");
            else{
                var lane = gn.Block();
                if(lane < 0 || lane >= laneCount)
                    Debug.Log("Note's block is false!");
                else
                    laneNotes[gn.Block()].Enqueue(gn);
            }
                
        }

        //测试当前最近的音符是否miss
        private void TestMiss(int lane)
        {
            NoteObject gn;
            if (laneNotes[lane].Count > 0)
                gn = laneNotes[lane].Peek();
            else return;

            //如果该音符已经被错过，直接增加一个miss
            if(judgement.Out(gn))
            {
                ComboPresenter.Instance.Combo(-1);
                laneNotes[lane].Dequeue().Miss();

                //如果该音符是长押的第一个音，则第二个音符也miss
                if(gn.Type() == 2){
                    laneHolding[lane] = false;

                    var cn = gn.GetChainedNote();
                    if(cn != null){
                        if(cn == laneNotes[lane].Peek()){
                            ComboPresenter.Instance.Combo(-1);
                            laneNotes[lane].Dequeue().Miss();
                        }
                    }
                }
            }
        }

        private void LaneAutoPlay(int lane)
        {
            NoteObject gn;
            if (laneNotes[lane].Count > 0)
                gn = laneNotes[lane].Peek();
            else return;

            if(Mathf.Abs(GetDeltaTime(gn)) < MusicController.Instance.TimeToSample(0.02f))
            {
                var type = gn.Type();

                switch(type){
                    case 1:
                        LaneKeyDown(lane);
                        break;
                    case 2:
                        if(laneHolding[lane])  
                            LaneKeyUp(lane);
                        else
                            LaneKeyDown(lane);
                        break;
                }

            }
        }

        private void LaneKeyUp(int lane){
            if(laneHolding[lane]){
                laneHolding[lane] = false;

                //获取最近的仍在判定区的音符
                NoteObject gn;
                if (laneNotes[lane].Count > 0)
                    gn = laneNotes[lane].Peek();
                else{
                    return;
                }

                //判定
                var result = judgement.Judge(gn);
                //过早则不予以判定
                if(result == -2)
                    return;
                ComboPresenter.Instance.Combo(result);

                if(result == -1) {
                    laneNotes[lane].Dequeue().Miss();
                } 
                else {
                    laneNotes[lane].Dequeue().Click();
                }
            }
        }
        private void LaneKeyDown(int lane)
        {
            if(PlayerSettings.Instance.clap == 1) SEPool.Instance.PlayClap();

            //获取最近的仍在判定区的音符
            NoteObject gn;
            if (laneNotes[lane].Count > 0)
                gn = laneNotes[lane].Peek();
            else return;

            //判定
            var result = judgement.Judge(gn);
            //过早则不予以判定
            if(result == -2)
                return;
            ComboPresenter.Instance.Combo(result);

            var type = gn.Type();

            if(result == -1) {
                laneNotes[lane].Dequeue().Miss();

                //如果该音符是长押的第一个音，则第二个音符也miss
                if(gn.Type() == 2){
                    laneHolding[lane] = false;

                    var cn = gn.GetChainedNote();
                    if(cn != null){
                        if(cn == laneNotes[lane].Peek()){
                            ComboPresenter.Instance.Combo(-1);
                            laneNotes[lane].Dequeue().Miss();
                        }
                    }
                }
            } 
            else {
                laneNotes[lane].Dequeue().Click();

                if(type == 2){
                    laneHolding[lane] = true;
                }
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

    class Judgement{

        float[] sampleRange;

        float early;

        MusicController mc;
        float offset;
        int BPM;

        public Judgement(){
            mc = MusicController.Instance;
            offset = NotesController.Instance.offset;
            BPM = NotesController.Instance.BPM;
        }

        public void SetSampleRange(float perfect, float great, float good, float bad){

            sampleRange = new float[]{
                mc.TimeToSample(perfect),
                mc.TimeToSample(great),
                mc.TimeToSample(good),
                mc.TimeToSample(bad),

            };
        }

        public void SetEarly(float e){
            early = mc.TimeToSample(e);
        }

        public float GetDeltaSample(NoteObject gn)
        {
            float d = (mc.GetSamples() - (gn.time + offset));
            return d;
        }

        ///<summary>
        ///对音符进行判定
        ///<returns>0: perfect; 1: great; 2: good; 3: bad; -1: miss; -2: early, no judgement</returns>
        ///</summary>
        public int Judge(NoteObject gn){

            var delta = GetDeltaSample(gn);

            if(delta < early)
                return -2;

            delta = Mathf.Abs(delta);

            for(int i = 0; i<4; i++){
                if(delta <= sampleRange[i]) return i;
            }
            return -1;

        }

        public bool Out(NoteObject gn){
            var delta = GetDeltaSample(gn);
            return delta > sampleRange[3];
        }

        public bool IsMissed(NoteObject gn){
            return Judge(gn) == -1;
        }


    }

}

using System.Collections;
using System.Collections.Generic;
using NoteEditor.DTO;
using UnityEngine;
using UnityEngine.UI;


namespace Game.Process
{
    public class NoteObject : MonoBehaviour
    {
        //音符信息
        private MusicDTO.Note note;

        //public int num, block;
        [SerializeField] public float time;

        //图片对象
        private Image image;
        
        //是否已经被点击过
        [SerializeField] public bool clicked = false;
        //是否在判定队列中
        [SerializeField] public bool inQueue = false;

        //长音的后续音符
        [SerializeField]
        private List<NoteObject> chainedNotes;
        [SerializeField]
        private List<HoldingBar> bars;

        //public bool moving = false;
        //public float moveSpeed;
        //public float time;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        public Vector2 GetPosition()
        {
            return GetComponent<RectTransform>().anchoredPosition;
        }

        public void SetPosition(Vector2 pos)
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
        }

        public void Init(MusicDTO.Note n)
        {
            note = n;

            /*
            num = n.num;
            block = n.block;*/

            gameObject.name = Block() + "-" + Num();
        }

        public int LPB() { return note.LPB;  }

        public int Num() { return note.num;  }

        public int Block() { return note.block;  }

        public int Type() { return note.type;  }

        public void SetColor(Color c)
        {
            image.color = c;
        }

        public void AddHoldingBar(HoldingBar hb)
        {
            if (bars == null) bars = new List<HoldingBar>();
            else bars.Clear();

            bars.Add(hb);

        }

        public HoldingBar GetHoldingBar()
        {
            if (bars == null || bars.Count == 0) return null;
            else return bars[0];
        }

        public void AddChainedNote(NoteObject n)
        {
            if (chainedNotes == null) chainedNotes = new List<NoteObject>();
            else chainedNotes.Clear();

            chainedNotes.Add(n);
        }

        public NoteObject GetChainedNote()
        {
            if (chainedNotes == null || chainedNotes.Count == 0) return null;
            else return chainedNotes[0];
        }

        public void Click()
        {
            //EffectController.Instance.CreateClickEffect(new Vector2(GetComponent<RectTransform>().anchoredPosition.x, UISizeController.Instance.TARGET_LINE_Y));
            ParticleEffectsController.Instance.CreateClickEffect(new Vector3(transform.position.x, UISizeController.Instance.CLICK_EFFECT_Y));

            SetColor(Color.clear);

            /*
            if(Type() == 2){
                if(GetNote()!=null && GetHoldingBar()!=null)
                    StartCoroutine(HoldingEffect());
            }
            */

            //SEPool.Instance.PlayClap();
            clicked = true;
        }

        private IEnumerator HoldingEffect(){

            //Debug.Log("Show holding bar effect");

            var note = GetChainedNote();
            var bar = GetHoldingBar();
            yield return (note && bar);

            var parentRect = transform.parent.GetComponent<RectTransform>();
            var startY = parentRect.anchoredPosition.y;
            var startHeight = bar.GetHeight();

            while(!note.clicked){

                var y = startHeight - (startY - parentRect.anchoredPosition.y);

                bar.SetHeight(y);
                yield return new WaitForEndOfFrame();
            }

        }

        public void Miss()
        {
            SetColor(Color.gray);
            clicked = true;

            if(Type() == 2){
                if(GetChainedNote()){
                    GetChainedNote().Miss();
                }

                if(GetHoldingBar()){
                    GetHoldingBar().SetColor(Color.gray);
                }
            }

        }

        /*
        public void StartMove(float speedY)
        {
            moving = true;
            moveSpeed = speedY;
        }
        */

    }

}



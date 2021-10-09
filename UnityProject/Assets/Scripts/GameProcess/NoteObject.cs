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
        private List<NoteObject> notes;
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

        public void AddNote(NoteObject n)
        {
            if (notes == null) notes = new List<NoteObject>();
            else notes.Clear();

            notes.Add(n);
        }

        public NoteObject GetNote()
        {
            if (notes == null || notes.Count == 0) return null;
            else return notes[0];
        }

        public void Click()
        {
            //EffectController.Instance.CreateClickEffect(new Vector2(GetComponent<RectTransform>().anchoredPosition.x, UISizeController.Instance.TARGET_LINE_Y));
            ParticleEffectsController.Instance.CreateClickEffect(new Vector3(transform.position.x, UISizeController.Instance.CLICK_EFFECT_Y));

            SetColor(Color.clear);
            //SEPool.Instance.PlayClap();
            clicked = true;
        }

        public void Miss()
        {
            SetColor(Color.black);
            clicked = true;
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



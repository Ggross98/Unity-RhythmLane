using System.Collections;
using System.Collections.Generic;
using NoteEditor.DTO;
using UnityEngine;
using UnityEngine.UI;


namespace Game.Process
{
    public class GameNote : MonoBehaviour
    {
        //音符信息
        private MusicDTO.Note note;

        //public int num, block;
        public float time;

        //图片对象
        private Image image;
        
        //是否已经被点击过
        public bool clicked = false;
        //是否在判定队列中
        public bool inQueue = false;

        //长音的后续音符
        //public List<GameNote> notes;

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



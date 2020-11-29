using System.Collections;
using System.Collections.Generic;
using NoteEditor.DTO;
using UnityEngine;
using UnityEngine.UI;


namespace Game.Process
{
    public class GameNote : MonoBehaviour
    {
        public MusicDTO.Note note;

        private Image image;

        public int num;

        public float time;

        public bool clicked = false;

        public bool inQueue = false;

        //public bool moving = false;

        //public float moveSpeed;

        //public float time;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        public Vector2 GetPosition()
        {
            return image.rectTransform.anchoredPosition;
        }

        public void SetPosition(Vector2 pos)
        {
            image.rectTransform.anchoredPosition = pos;
        }

        public void Init(MusicDTO.Note n)
        {
            note = n;
            num = n.num;
            gameObject.name = note.block + "-" + note.num;
        }

        public void SetColor(Color c)
        {
            image.color = c;
        }

        public void Click()
        {
            EffectController.Instance.CreateClickEffect(transform.position);

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



using System.Collections.Generic;

namespace NoteEditor.DTO
{
    public class MusicDTO
    {
        [System.Serializable]
        public class EditData
        {
            public string name;
            public int maxBlock;
            public int BPM;
            public int offset;
            public List<Note> notes;
        }

        [System.Serializable]
        public class Note
        {
            //第几个小节的第几拍
            public int LPB;
            //在第几个小节
            public int num;
            //在第几轨
            public int block;
            //音符类型（单击，长音）
            public int type;
            //长音的后续音符
            public List<Note> notes;
        }
    }
}

using UnityEngine;

namespace NoteEditor.GLDrawing
{
    /// <summary>
    /// 用GL画的多边形
    /// </summary>
    public class Geometry
    {
        public Color color;
        public Vector3[] vertices; //顶点坐标

        public Geometry(Vector3[] vertices, Color color)
        {
            this.color = color;
            this.vertices = vertices;
        }
    }
}

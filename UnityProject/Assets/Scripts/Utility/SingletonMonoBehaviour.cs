using UnityEngine;

namespace NoteEditor.Utility
{
    /// <summary>
    /// 单例模式基类
    /// </summary>
    /// <typeparam name="T">派生类的名称</typeparam>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance_;
        public static T Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = FindObjectOfType<T>();
                }

                return instance_ ?? new GameObject(typeof(T).FullName).AddComponent<T>();
            }
        }
    }
}

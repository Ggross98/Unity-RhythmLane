using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoteEditor.Utility;

/// <summary>
/// 例子特效生成
/// </summary>
public class ParticleEffectsController : SingletonMonoBehaviour<ParticleEffectsController>
{
    [SerializeField]
    private ParticleSystem click;

    [SerializeField]
    //private Transform effectParent;

    public void CreateClickEffect(Vector3 pos)
    {
        ParticleSystem obj = Instantiate(click);
        obj.transform.position = pos;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoteEditor.Utility;

public class EffectController : SingletonMonoBehaviour<EffectController>
{
    public ParticleSystem click;

    public void CreateClickEffect(Vector3 pos)
    {
        ParticleSystem obj = Instantiate(click);
        obj.transform.position = pos;
    }

}

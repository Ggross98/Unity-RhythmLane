using NoteEditor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISizeController : SingletonMonoBehaviour<UISizeController>
{
    [HideInInspector]
    public float CLICK_EFFECT_Y;

    [SerializeField]
    private Transform targetLine;

    private void Awake()
    {
        if(targetLine != null)
        {
            CLICK_EFFECT_Y = targetLine.position.y;
        }
    }

}

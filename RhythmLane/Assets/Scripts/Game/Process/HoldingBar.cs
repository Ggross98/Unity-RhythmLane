using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldingBar : MonoBehaviour
{

    //private Vector2 size;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if(rect == null)
        {
            Debug.Log("No rect transform!");
            return;
        }
    }

    public void SetHeight(float height)
    {
        rect.sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, height);
    }

    public float GetHeight(){
        return rect.sizeDelta.y;
    }

    public void SetPosition(float x, float y)
    {
        rect.anchoredPosition = new Vector2(x, y);
    }

    public void SetColor(Color color){
        GetComponent<Image>().color = color;
    }
}

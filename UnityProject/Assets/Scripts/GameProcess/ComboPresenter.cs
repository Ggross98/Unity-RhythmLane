using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoteEditor.Utility;

public class ComboPresenter : SingletonMonoBehaviour<ComboPresenter>
{
    public Text clickText, comboText;

    public float showTime = 0.2f;

    public float leftTime = 0;

    public const int MISS = -1, PERFECT = 0, GREAT = 1, GOOD = 2, BAD = 3;

    public int miss, perfect, great, good, bad;

    public int combo, maxCombo;

    public GameObject resultMenu;

    public Text missText, perfectText, greatText, goodText, badText;
    public Text maxComboText;


    

    void Update()
    {
        if (leftTime <= 0)
        {
            clickText.text = "";
        }
        else
        {
            leftTime -= Time.deltaTime;
        }

        comboText.text = "Combo\t\t" + combo;
    }
    
    public void ShowClick(string c)
    {
        clickText.text = c;
        leftTime = showTime;
    }

    public void ShowResult()
    {
        if (resultMenu.activeSelf)
        {
            resultMenu.SetActive(false);
            return;
        }


        if (maxCombo < combo) maxCombo = combo;
        clickText.gameObject.SetActive(false);

        resultMenu.SetActive(true);

        missText.text = "Miss\t\t\t" + miss;
        perfectText.text = "Perfect\t\t" + perfect;
        greatText.text = "Great\t\t" + great;
        goodText.text = "Good\t\t" + good;
        badText.text = "Bad\t\t\t" + bad;

        maxComboText.text = "Max Combo\t" + maxCombo;
    }

    public void Combo(int i)
    {
        switch (i)
        {
            case MISS:
                miss++;
                ShowClick("Miss");
                EndCombo();
                break;
            case PERFECT:
                perfect++;
                ShowClick("Perfect");
                combo++;
                break;
            case GREAT:
                great++;
                ShowClick("Great");
                combo++;
                break;
            case GOOD:
                good++;
                ShowClick("Good");
                EndCombo();
                break;
            case BAD:
                bad++;
                ShowClick("Bad");
                EndCombo();
                break;

            default:
                break;
        }
    }

    private void EndCombo()
    {
        if (combo > maxCombo) maxCombo = combo;
        combo = 0;
    }
}

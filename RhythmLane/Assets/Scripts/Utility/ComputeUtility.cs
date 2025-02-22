using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUtility
{
    public static string FormatTwoTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        string mm = minutes < 10f ? "0" + minutes : minutes.ToString();
        int seconds = (totalSeconds - (minutes * 60));
        string ss = seconds < 10 ? "0" + seconds : seconds.ToString();
        return string.Format("{0}:{1}", mm, ss);
    }
}

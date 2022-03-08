using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public float tick;
    public float seconds = 0;
    public int minutes;
    public int hours;
    public int days = 1;

    public void Awake() => GlobalVariables.clock = this;

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (GameManager.State != GameState.INGAME)
            return;
        CalcTime();
    }

    /// <summary>
    /// Updates the clock
    /// </summary>
    public void CalcTime()
    {
        //Debug.Log(days + ":" + hours + ":" + minutes + ":" + seconds);
        seconds += Time.fixedDeltaTime * tick;
        if (seconds >= 60)
        {
            minutes = minutes + (int)(seconds/60);
            seconds = seconds%60;
        }
        if (minutes >= 60)
        {
            hours = hours + (int)(minutes/60);
            minutes = minutes%60;
        }
        if (hours >= 24)
        {
            days = days + (int)(hours/24);
            hours = hours%24;
        }
    }
}

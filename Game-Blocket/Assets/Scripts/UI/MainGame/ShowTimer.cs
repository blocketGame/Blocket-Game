using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTimer : MonoBehaviour
{
    public Clock clock;
    public Text text;

    public void FixedUpdate()
    {
        if (GameManager.State != GameState.INGAME)
            return;
        //clock.CalcTime();
        text.text = clock.hours + ":" + clock.minutes;
    }
}

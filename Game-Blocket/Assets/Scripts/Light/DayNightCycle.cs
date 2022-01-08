using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D globalLight;
    public static float seconds = 0;
    public static int minutes;
    public static int hours;
    // Start is called before the first frame update
    // Update is called once per frame
    void Update()
    {
        if (GameManager.GameState != GameState.INGAME)
            return;
        seconds += Time.deltaTime;
        if (seconds >= 60)
        {
            seconds = 0;
            minutes++;
        } if(minutes >= 60)
        {
            minutes = 0;
            hours++;
        } if(hours >= 24)
        {
            hours = 0;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.GameState != GameState.INGAME)
            return;
        if ((hours>19||hours<5) && globalLight.intensity>0)
        {
            globalLight.intensity -= 0.0001f;
        }
        else if (hours > 5 && globalLight.intensity<1)
        {
            globalLight.intensity += 0.0001f;
        }
    }
}

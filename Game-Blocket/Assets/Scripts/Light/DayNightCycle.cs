using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D globalLight;
    public Volume volume;

    public Clock clock;

    public int duskFrom;
    public int duskTo;

    public int dawnFrom;
    public int dawnTo;

    private void FixedUpdate()
    {
        if (GameManager.State != GameState.INGAME)
            return;
        ControllVolume();
    }

    public void ControllVolume()
    {
        if (clock.hours >= duskFrom && clock.hours < duskTo)
            volume.weight = (float)clock.minutes / ((duskTo - duskFrom) * 60);
            

        if(clock.hours >= dawnFrom && clock.hours < dawnTo)
            volume.weight = 1 - (float)clock.minutes / ((dawnTo - dawnFrom) * 60);
    }
}

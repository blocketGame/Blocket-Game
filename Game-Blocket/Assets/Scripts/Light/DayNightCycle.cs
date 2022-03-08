using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D globalLight;
    public Volume volume;

    public int duskFrom;
    public int duskTo;

    public int dawnFrom;
    public int dawnTo;

    //Intensity of the player light
    public float maxIntensity; 

    public Light2D playerLight;

    public void Awake() => GlobalVariables.dayNightCycle = this;

    private void FixedUpdate()
    {
        if (GameManager.State != GameState.INGAME)
            return;

        //not a good solution
        if(GlobalVariables.PlayerVariables != null && playerLight == null)
        {
            playerLight = GlobalVariables.PlayerVariables.playerLight;
        }

        ControllVolume();
    }

    /// <summary>
    /// Sets the value of the volume to simulate a day night cycle
    /// </summary>
    public void ControllVolume()
    {
        if (GlobalVariables.clock.hours >= duskFrom && GlobalVariables.clock.hours < duskTo)
        {
            volume.weight = (float)GlobalVariables.clock.minutes / ((duskTo - duskFrom) * 60);
            playerLight.intensity = ((float)GlobalVariables.clock.minutes / ((duskTo - duskFrom) * 60)) * maxIntensity;
        }
        else
        if ((GlobalVariables.clock.hours >= duskTo && GlobalVariables.clock.hours < 24) || GlobalVariables.clock.hours < dawnFrom)
        {
            volume.weight = 1;
            playerLight.intensity = maxIntensity;
        }
        else
        if(GlobalVariables.clock.hours >= dawnFrom && GlobalVariables.clock.hours < dawnTo)
        {
            volume.weight = 1 - (float)GlobalVariables.clock.minutes / ((dawnTo - dawnFrom) * 60);
            playerLight.intensity = maxIntensity - ((float)GlobalVariables.clock.minutes / ((duskTo - duskFrom) * 60)) * maxIntensity;
        }
        else
        if (GlobalVariables.clock.hours >= dawnTo && GlobalVariables.clock.hours < duskFrom)
        {
            volume.weight = 0;
            playerLight.intensity = 0;
        }
    }
}

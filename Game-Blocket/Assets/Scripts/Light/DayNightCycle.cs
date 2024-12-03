using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour{
	public static DayNightCycle Singleton { get; private set; }

	public Light globalLight;
	public Volume volume;

	public int duskFrom;
	public int duskTo;

	public int dawnFrom;
	public int dawnTo;

	//Intensity of the player light
	public float maxIntensity; 

	public Light playerLight;

	public void Awake() => Singleton = this;

	private void FixedUpdate()
	{
		if (GameManager.State != GameState.INGAME)
			return;

		//not a good solution
		if(PlayerVariables.Singleton != null && playerLight == null)
		{
			playerLight = PlayerVariables.Singleton.playerLight;
		}

		ControllVolume();
	}

	/// <summary>
	/// Sets the value of the volume to simulate a day night cycle
	/// </summary>
	public void ControllVolume(){
		ClockHandler clock = ClockHandler.Singleton;
		if (clock.hours >= duskFrom && clock.hours < duskTo){
			volume.weight = (float)clock.minutes / ((duskTo - duskFrom) * 60);
			playerLight.intensity = ((float)clock.minutes / ((duskTo - duskFrom) * 60)) * maxIntensity;
		}else if ((clock.hours >= duskTo && clock.hours < 24) || clock.hours < dawnFrom){
			volume.weight = 1;
			playerLight.intensity = maxIntensity;
		}else if(clock.hours >= dawnFrom && clock.hours < dawnTo){
			volume.weight = 1 - (float)clock.minutes / ((dawnTo - dawnFrom) * 60);
			playerLight.intensity = maxIntensity - ((float)clock.minutes / ((duskTo - duskFrom) * 60)) * maxIntensity;
		}else if (clock.hours >= dawnTo && clock.hours < duskFrom){
			volume.weight = 0;
			playerLight.intensity = 0;
		}
	}
}

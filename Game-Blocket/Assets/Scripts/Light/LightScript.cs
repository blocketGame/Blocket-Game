using UnityEngine;

/// <summary>
/// 
/// </summary>
public class LightScript : MonoBehaviour{
    public static LightScript Singleton { get; private set; }

    //THIS DOESNT WORK
    [SerializeField]
    private LightRegion[] lightRegions;
    [SerializeField]
    private Light[] lights;
    [SerializeField]
    private GameObject player;

    public GameObject Player { get => player; set => player = value; }
    public LightRegion[] LightRegions { get => lightRegions; set => lightRegions = value; }
    public Light[] Lights { get => lights; set => lights = value; }

    private void Awake() => Singleton = this;

    // Update is called once per frame
    private void FixedUpdate()
    {
        /*if (Player.transform.position.y < -10)
        {
            if(Player.transform.position.y > -26)
            {
                GlobalLight.intensity = 1-(Player.transform.position.y * -1 - 10) * 0.05f;
            }
            if (Player.transform.position.y > -16)
            {
                PlayerLight.intensity = (Player.transform.position.y*-1 - 10) * 0.05f;
            }
        }
        else
        {
            GlobalLight.intensity = 1;
            PlayerLight.intensity = 0;
        }*/
        foreach(LightRegion lr in lightRegions)
        {
            if(Player.transform.position.y>lr.EndY&& Player.transform.position.y < lr.StartY)
            {
                switch (lr.Options)
                {
                    case LightOptions.Decrease_Intensity:
                        if (lights[lr.LightSource].intensity >lr.Value )
                            lights[lr.LightSource].intensity -=0.01f;
                        break;
                    case LightOptions.Increase_Intensity:
                        if (lights[lr.LightSource].intensity < lr.Value)
                            lights[lr.LightSource].intensity += 0.01f;
                        break;
                }
                
            }
        }
        foreach(Light l in lights)
        {
            if (l.intensity < 0)
            {
                l.intensity = 0;
            }
        }
    }

}

[System.Serializable]
public struct LightRegion
{
    [SerializeField]
    private LightOptions options;
    [SerializeField]
    private int startY;
    [SerializeField]
    private int endY;
    [SerializeField]
    private float value;
    [SerializeField]
    private byte lightSource;

    public LightOptions Options { get => options; set => options = value; }
    public byte LightSource { get => lightSource; set => lightSource = value; }
    public int StartY { get => startY; set => startY = value; }
    public float Value { get => value; set => this.value = value; }
    public int EndY { get => endY; set => endY = value; }
}

public enum LightOptions{
    Increase_Intensity,Decrease_Intensity,Grow,Shrink
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightScript : MonoBehaviour
{

    [SerializeField]
    private Light2D globalLight;
    [SerializeField]
    private Light2D playerLight;
    [SerializeField]
    private GameObject player;

    public Light2D GlobalLight { get => globalLight; set => globalLight = value; }
    public Light2D PlayerLight { get => playerLight; set => playerLight = value; }
    public GameObject Player { get => player; set => player = value; }


    // Update is called once per frame
    void Update()
    {
        if (Player.transform.position.y < -10)
        {
            if(Player.transform.position.y > -20)
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
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTimer : MonoBehaviour
{

    void Update()
    {
        GetComponent<Text>().text = DayNightCycle.hours + ":" + DayNightCycle.minutes + ":" + Mathf.RoundToInt(DayNightCycle.seconds);
    }
}

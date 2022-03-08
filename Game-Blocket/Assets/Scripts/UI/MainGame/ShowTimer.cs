using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTimer : MonoBehaviour
{
    public Text text;

    public void FixedUpdate()
    {
        //clock.CalcTime();
        text.text = string.Format("{0:00}:{1:00}", GlobalVariables.clock.hours, GlobalVariables.clock.minutes);
    }
}

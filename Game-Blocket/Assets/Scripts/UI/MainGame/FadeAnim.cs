using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeAnim : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Color c = gameObject.GetComponent<Text>().color;
        c.a -= 0.00001f * Time.fixedDeltaTime;
        gameObject.GetComponent<Text>().color = c;
        if(c.a<=0) GameObject.Destroy(gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextOnValueChange : MonoBehaviour
{
    public Text outputText;
    [SerializeField]
    public InputValueType InputValueType;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        switch (InputValueType)
        {
            case InputValueType.SLIDER:
                outputText.text = Mathf.RoundToInt(GetComponent<Slider>().value) + "%";
                break;
            case InputValueType.DROPDOWN:
                outputText.text = GetComponent<Dropdown>().options[GetComponent<Dropdown>().value].text;
                //Screen.SetResolution(int.Parse(outputText.text.Split('x')[0]), int.Parse(outputText.text.Split('x')[1]), true); //geht nd lol
                break;
        }
        
    }
}

[Serializable]
public enum InputValueType
{
    SLIDER,
    DROPDOWN
}

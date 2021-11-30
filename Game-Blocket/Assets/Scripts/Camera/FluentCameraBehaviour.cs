using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluentCameraBehaviour : MonoBehaviour
{
    private float OffSetX = 0;
    private Vector3 originalPos;
    private void Awake()
    {
        originalPos = gameObject.transform.localPosition;
    }
    private void FixedUpdate()
    {
        //if (OffSetX > -5 && OffSetX < 5)
        //{
        //Debug.Log("WL"+(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToLocal(Input.mousePosition).x- GlobalVariables.World.GetComponentInChildren<Grid>().WorldToLocal(GlobalVariables.LocalPlayerPos).x-960));
        //Debug.Log("WTC"+(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(Input.mousePosition).x- GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(GlobalVariables.LocalPlayerPos).x-960));
        //Debug.Log("LTW"+ (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x - GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x-960));
        //if (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x-960>-100&& GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x - 960 < 100)
        //{
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //if(OffSetX< (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60) - 18  || -OffSetX > (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60 - 18))
                //if (0 < (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60) - 18)
                    OffSetX += ((GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60) - 18)/100;
            gameObject.transform.position = new Vector3(originalPos.x + OffSetX + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x ), this.gameObject.transform.position.y, this.gameObject.transform.position.z);
        }
        else if(OffSetX>0)
        {
            if (OffSetX > 1)
                OffSetX -= 0.2f;
            else
               if (OffSetX > 0.1)
                OffSetX -= 0.01f;
            gameObject.transform.position = new Vector3(originalPos.x + OffSetX + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x ), this.gameObject.transform.position.y, this.gameObject.transform.position.z);
        }
        else if (OffSetX < 0)
        {
            if (OffSetX < -1)
                OffSetX += 0.2f;
            else
                if (OffSetX < -0.1)
                OffSetX +=0.01f;
            gameObject.transform.position = new Vector3(originalPos.x + OffSetX + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x ), this.gameObject.transform.position.y, this.gameObject.transform.position.z);
        }
        //}
        //}
    }
}

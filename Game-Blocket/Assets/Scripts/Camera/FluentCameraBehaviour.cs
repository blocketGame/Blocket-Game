using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// @Cse19455
/// [TODO: Optimize with delegate]
/// </summary>
public class FluentCameraBehaviour : MonoBehaviour
{
    private float OffSetX = 0;
    private float OffSetY = 0;
    private Vector3 originalPos;
    private void Awake()
    {
        originalPos = gameObject.transform.localPosition;
    }
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //if(OffSetX< (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60) - 18  || -OffSetX > (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60 - 18))
            //if (0 < (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60) - 18)
            OffSetX += ((GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).x / 60) - 18) / 80;
            OffSetY += ((GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(Input.mousePosition).y / 60) - 8) / 80;
            gameObject.transform.position = new Vector3(originalPos.x + OffSetX + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x), originalPos.y + OffSetY + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).y), gameObject.transform.position.z);
        }
        else
        {
            if (OffSetX > 0)
            {
                if (OffSetX > 1)
                    OffSetX -= 0.2f;
                else
                   if (OffSetX > 0.1)
                    OffSetX -= 0.01f;
                gameObject.transform.position = new Vector3(originalPos.x + OffSetX + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x), this.gameObject.transform.position.y, this.gameObject.transform.position.z);
            }
            else if (OffSetX < 0)
            {
                if (OffSetX < -1)
                    OffSetX += 0.2f;
                else
                    if (OffSetX < -0.1)
                    OffSetX += 0.01f;
                gameObject.transform.position = new Vector3(originalPos.x + OffSetX + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).x), this.gameObject.transform.position.y, this.gameObject.transform.position.z);
            }

            if (OffSetY > 0)
            {
                if (OffSetY > 1)
                    OffSetY -= 0.2f;
                else
                   if (OffSetY > 0.1)
                    OffSetY -= 0.01f;
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, originalPos.y + OffSetY + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).y), this.gameObject.transform.position.z);
            }
            else if (OffSetY < 0)
            {
                if (OffSetY < -1)
                    OffSetY += 0.2f;
                else
                    if (OffSetY < -0.1)
                    OffSetY += 0.01f;
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, originalPos.y + OffSetY + (GlobalVariables.World.GetComponentInChildren<Grid>().LocalToWorld(GlobalVariables.LocalPlayerPos).y), this.gameObject.transform.position.z);
            }
        }
    }
}


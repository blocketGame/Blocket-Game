using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Trash
/// </summary>
public class BetterMovementScript : MonoBehaviour
{
   
    public float MovementSpeed = 300f;
    public float JumpForce = 6f;
    public float fallMulti = 1.06f;
    public float side;
    private bool lockvar;

    public Rigidbody2D playerRigidbody;


    void FixedUpdate()
    {
        //right,left movement
        Clipping();

        var movement = Input.GetAxis("Horizontal");
        //if(!lockvar)
        if(!lockvar)
        playerRigidbody.gameObject.transform.position += Time.deltaTime * MovementSpeed * new Vector3(movement, 0, 0);

        //jump
        if (playerRigidbody.velocity.y == 0 && Input.GetKey(GlobalVariables.jump))
        {
            playerRigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        }
        //fall
        if (playerRigidbody.velocity.y < 0)
        {
            if (playerRigidbody.velocity.y > -15)
            {
                playerRigidbody.gameObject.transform.position += Time.deltaTime * new Vector3(movement, (playerRigidbody.velocity.y) * fallMulti, 0);
            }
        }
        if (movement > 0)
        {
            side = 1f;
        }else if(movement<0)
        {
            side = -1f;
        }

        if(playerRigidbody.velocity.y ==0 && Input.GetKeyDown(GlobalVariables.roll))
        {
            playerRigidbody.AddForce(new Vector2(MovementSpeed*0.8f*side, JumpForce/1.5f), ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Creates an invisible Wall for the player (Collider)
    /// </summary>
    private void Clipping()
    {
        if (GlobalVariables.WorldData.GetBlockFormCoordinate(
            GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + (side*0.5f), playerRigidbody.position.y, 0)).x,
            GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + side, playerRigidbody.position.y - 1, 0)).y)
            != 0)
            lockvar = true;
        else
            lockvar = false;
    }
}

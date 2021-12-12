using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// @Cse19455
/// Real Blocket Controller
/// </summary>
public class BetterMovementScript : MonoBehaviour {
    #region Player-Settings
    public float MovementSpeed = 300f;
    public float JumpForce = 6f;
    public float fallMulti = 1.06f;
    #endregion

    #region State-variables
    private float side;
    private bool lockvar;
    private float countdown;
    private float movement;
    #endregion

    public Rigidbody2D playerRigidbody;
    private void FixedUpdate() {
        Clipping();
    }

    private void TurnAnim() {
        if (GlobalVariables.LocalPlayer == null)
            return;

        if (GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x != side && side != 0
            && GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x < 1
            && GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x > -1)
            GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale = new Vector3(GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x + side * 0.05f, 1, 0);

    }

    void Update() {
        TurnAnim();

        VelocityUpdate();
        /// Input Freeze Countdown
        if (countdown > 0) {
            countdown -= Time.deltaTime;
            return;
        }

        ///turn
        if (movement > 0) {
            if (side != 1) {
                side = 1f;
                GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale = new Vector3(GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x + side * 0.05f, 1, 0);
            }
        } else if (movement < 0) {
            if (side != -1) {
                side = -1f;
                GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale = new Vector3(GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x + side * 0.05f, 1, 0);
            }
        }

        ///Horizontal Movement
        if (!lockvar) {
            if (Input.GetAxis("Horizontal") != 0)
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x * 0.9f, playerRigidbody.velocity.y);
            playerRigidbody.gameObject.transform.position += Time.deltaTime * MovementSpeed * new Vector3(movement, 0, 0);
        }

        ///Player is in the AIR
        //Falling Acceleration
        if (playerRigidbody.velocity.y < 0)
            if (playerRigidbody.velocity.y > -15)
                playerRigidbody.gameObject.transform.position += Time.deltaTime * new Vector3(movement, (playerRigidbody.velocity.y) * fallMulti, 0);


    }


    /// <summary>
    /// 
    /// </summary>
    public void VelocityUpdate() {

        if (playerRigidbody.velocity.y != 0)
            //Walljump 
            if (Input.GetKeyDown(GlobalVariables.jump)) {
                if (GlobalVariables.WorldData.GetBlockFormCoordinate(
                GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + (-0.5f), playerRigidbody.position.y, 0)).x,
                GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + side, playerRigidbody.position.y - 1, 0)).y)
                != 0) {
                    side = -1f;
                    Walljump();
                } else if (GlobalVariables.WorldData.GetBlockFormCoordinate(
                  GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + (0.5f), playerRigidbody.position.y, 0)).x,
                  GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + side, playerRigidbody.position.y - 1, 0)).y)
                  != 0) {
                    side = 1f;
                    Walljump();
                }
            }
            //Wall kick
            else if (lockvar && Input.GetKeyDown(GlobalVariables.roll))
                Wallkick();
            else
                movement = Input.GetAxis("Horizontal");
        ///Player is Grounded
        else
            //Jump
            if (Input.GetKey(GlobalVariables.jump))
            Jump();
        //Roll
        else if (!lockvar && Input.GetKeyDown(GlobalVariables.roll))
            Roll();
        //Move  
        else
            movement = Input.GetAxis("Horizontal");

    }

    /// <summary>
    /// Creates an invisible Wall for the player (Collider)
    /// </summary>
    private void Clipping() {
        if (GlobalVariables.WorldData.GetBlockFormCoordinate(
            GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + (side * 0.5f), playerRigidbody.position.y, 0)).x,
            GlobalVariables.WorldData.Grid.WorldToCell(new Vector3(playerRigidbody.position.x + side, playerRigidbody.position.y - 1, 0)).y)
            != 0)
            lockvar = true;
        else
            lockvar = false;
    }

    /// <summary>
    /// More or less a Long jump
    /// </summary>
    private void Roll() {
        playerRigidbody.AddRelativeForce(new Vector2(MovementSpeed * 1.2f * side, JumpForce / 1.5f), ForceMode2D.Impulse);
    }

    /// <summary>
    /// Normal Jump on the Floor
    /// </summary>
    private void Jump() {
        playerRigidbody.AddRelativeForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
    }

    /// <summary>
    /// Kick yourself away with more speed
    /// </summary>
    private void Wallkick() {
        playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
        playerRigidbody.AddRelativeForce(new Vector2(side * -1 * 7, fallMulti * 1.5f), ForceMode2D.Impulse);
        countdown = 0.4f;
    }

    /// <summary>
    /// Normal Walljump
    /// </summary>
    private void Walljump() {
        playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
        playerRigidbody.AddRelativeForce(new Vector2(side * -1 * 4, fallMulti * 4f), ForceMode2D.Impulse);
        countdown = 0.4f;
    }

    /// <summary>
    /// Motion Sickness Incoming
    /// </summary>
    private void iSpinMyHeadRightRoundRightRoundWhenYouGoDown() {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(gameObject.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y, gameObject.transform.rotation.eulerAngles.z + 0.1f));
    }

}
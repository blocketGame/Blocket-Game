using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFollowing : MonoBehaviour{
    public static ItemFollowing Singleton { get; private set; }

    public float maxDistanceDelta = 20;
    public Animator anim;

    private void Awake() => Singleton = this;

    private void LateUpdate()
    {
        if(PlayerVariables.Singleton.Race == CharacterRace.MAGICIAN)
            TurnItemToMouseAngle();
        AnimateWeapon();
        Physics2D.IgnoreLayerCollision(0,6);
        
    }

    private void TurnItemToMouseAngle()
    {
        transform.rotation = Quaternion.FromToRotation(Vector2.up , NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position));
        transform.localPosition = Vector2.MoveTowards(transform.localPosition,NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position)*2,maxDistanceDelta);
    }

    private Vector2 NormalizeVector(Vector3 vector)
    {
        return Vector3.Normalize(vector); 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
    }

    private void AnimateWeapon()
    {
        if (Input.GetKeyDown(GameManager.SettingsProfile.MainInteractionKey)) {
            string animationname = ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)?.swingingAnimation;
            anim.Play(animationname== string.Empty ? "Default" : animationname ?? "Default");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //I think this is not used for drops ~Fabian
        if(collision.collider.gameObject.layer == LayerMask.GetMask("Drop"))
            return;
        Debug.Log("Collision");
        collision.gameObject.GetComponent<Rigidbody2D>().AddForce(collision.relativeVelocity*5);
    }
}

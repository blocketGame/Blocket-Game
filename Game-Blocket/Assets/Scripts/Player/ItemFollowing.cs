using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFollowing : MonoBehaviour
{
    public float maxDistanceDelta = 20;
    public Animator anim;

    private void LateUpdate()
    {
        if(GlobalVariables.PlayerVariables.Race == CharacterRace.MAGICIAN)
            TurnItemToMouseAngle();
        AnimateWeapon();
    }

    private void TurnItemToMouseAngle()
    {
        transform.rotation = Quaternion.FromToRotation(Vector2.up , NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position));
        transform.localPosition = Vector2.MoveTowards(transform.localPosition,NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position)*2,maxDistanceDelta);
        Vector2 v = Vector3.Normalize(transform.localPosition);
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
            string animationname = GlobalVariables.ItemAssets.GetItemFromItemID(GlobalVariables.Inventory.SelectedItemId)?.swingingAnimation;
            anim.Play(animationname== string.Empty ? "Default" : animationname ?? "Default");
        }
    }
}

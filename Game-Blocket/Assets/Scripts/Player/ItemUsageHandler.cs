using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUsageHandler : MonoBehaviour
{
    public static ItemUsageHandler Singleton { get; private set; }
    public Transform shotPoint;
    public float maxDistanceDelta = 20;
    public Animator anim;
    public int komboCounter;
    public float timer;
    private Vector2 NormalizeVector(Vector3 vector) => Vector3.Normalize(vector);

    private void Awake() => Singleton = this;

    private void LateUpdate()
    {
        if(PlayerVariables.Singleton.Race == CharacterRace.MAGICIAN)
            TurnItemToMouseAngle();

        //Don´t go into method if selected item is 0 (No Item in hand)
        if(Inventory.Singleton.SelectedItemId != 0)
            AnimateWeapon();
        Physics2D.IgnoreLayerCollision(0,6);//TODO: Use Projectsettings
    }

    private void TurnItemToMouseAngle()
    {
        transform.rotation = Quaternion.FromToRotation(Vector2.up , NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position));
        transform.localPosition = Vector2.MoveTowards(transform.localPosition,NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position)*2,maxDistanceDelta);
    }

    private void TurnItemUpOrDown()
    {
        Vector2 v = Input.GetKey(KeyCode.S) ? Vector2.down : (Input.GetKey(KeyCode.W) ? Vector2.up : Vector2.zero);

        if (v != Vector2.zero)
        {
            transform.rotation = Quaternion.FromToRotation(Vector2.up, v);
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, v, 5);
        }
    }

    private void AnimateWeapon()
    {   
        WeaponItem weapon = (WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem();
        if(weapon.weaponType==WeaponItem.WeaponType.RANGE)
        if(Inventory.Singleton.FindFirstItem(ItemAssets.Singleton.GetItemFromItemID(weapon.projectile))==null)
            return;

        TurnItemUpOrDown();
        switch (weapon.behaviour)
        {
            case CustomWeaponBehaviour.DEFAULT:

                timer += weapon.CoolDownTime < (timer) ? 0 : Time.deltaTime;
                if (Input.GetKeyDown(GameManager.SettingsProfile.MainInteractionKey) && timer >(weapon.CoolDownTime * 0.75f))
                {
                    if(timer<(weapon.CoolDownTime * 0.95f))
                    {
                        komboCounter++;
                        Debug.Log("KomboCounter" + komboCounter);
                    }
                    else
                        komboCounter = 0;
                    PlayAnim();
                    if (weapon.projectile != 0)
                        CreateProjectile(weapon);

                }
                else if (Input.GetKey(GameManager.SettingsProfile.MainInteractionKey) && timer > weapon.CoolDownTime)
                {
                    komboCounter = 0;
                    PlayAnim();
                    if (weapon.projectile != 0 && weapon.holdShooting)
                        CreateProjectile(weapon);
                }
                break;
            default:
                goto case CustomWeaponBehaviour.DEFAULT;
        }
    }

    #region DEFAULT-Weapon-Behaviour
    private void PlayAnim()
    {
        timer = 0;
        string animationname;
        if (((WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem()).weaponType == WeaponItem.WeaponType.RANGE)
        {
            animationname = ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)?.swingingAnimation;
        }
        else
        {
            WeaponItem weapon = (WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem();
            if (weapon.swingingAnimations.Count == 0)
                animationname = weapon?.swingingAnimation;
            else if (weapon.swingingAnimations.Count == komboCounter)
            {
                komboCounter = 0;
                animationname = weapon?.swingingAnimations[komboCounter];
            }
            else
                animationname = weapon?.swingingAnimations[komboCounter];
        }
        
        anim.Play(animationname == string.Empty ? "Default" : animationname ?? "Default");
    }

    private void CreateProjectile(WeaponItem weapon) => CreateProjectile(ItemAssets.Singleton.Projectiles.Find(item => item.id.Equals(weapon.projectile)) ?? new Projectile(), weapon);

    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        WeaponItem weapon = (WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem();
        if (!weapon.dmgOnColliderHit)
            return;

        Debug.Log("Collision");
        //Damage Enemy
        Mob mob = collision.gameObject.GetComponent<Mob>() ?? new Mob();
        DealDamageOnHit(mob);
    }

    /// <summary>
    /// Dealing Damage to Enemy instance
    /// </summary>
    /// <param name="mob"></param>
    private void DealDamageOnHit(Mob mob)
    {
        WeaponItem w = (WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem();
        if (w.dmgOnColliderHit)
        {
            Debug.Log(w.damage);
            mob.HealthNow -= w.damage;
        }
    }

    /// <summary>
    /// Creating the Projectile Object with the specific Parameters
    /// </summary>
    /// <param name="projectile"></param>
    public void CreateProjectile(Projectile projectile,WeaponItem w)
    {
        Inventory.Singleton.RemoveItem(ItemAssets.Singleton.GetItemFromItemID(w.projectile), 1);
        GameObject go = new GameObject(projectile.name);
        go.transform.position = shotPoint.position + projectile.SpawningPos3;
        go.AddComponent<ProjectileBehaviour>().Fill(w.maxHeight,w.maxDistance,projectile,w.damage);
        //Enemy should listen to Bullet Layer to see if it is hit by one.
    }
}

[Serializable]
public enum CustomWeaponBehaviour
{
    DEFAULT
}

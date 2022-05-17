using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class ProjectileBehaviour : MonoBehaviour
{
    public int maxHeight, maxDistance;
    // Start is called before the first frame update
    public Projectile projectile;
    public float weaponDamage;
    
    public float CalculatedDamage { get => (weaponDamage/2 + projectile.damage) +
            (1 +
            ((gameObject.GetComponent<Rigidbody2D>().velocity.x < 0) ? gameObject.GetComponent<Rigidbody2D>().velocity.x * -1 : gameObject.GetComponent<Rigidbody2D>().velocity.x)
            * ((gameObject.GetComponent<Rigidbody2D>().velocity.y < 0) ? gameObject.GetComponent<Rigidbody2D>().velocity.y * -1 : gameObject.GetComponent<Rigidbody2D>().velocity.y));
    }

    public Vector2 flyingDirection;

    public void CalcFlyingBehaviour()
    {
        flyingDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - this.transform.position).normalized *5 *projectile.flyingSpeed;

        if (flyingDirection.x < 0)
            flyingDirection = flyingDirection.x < -maxDistance ? new Vector2(-maxDistance, flyingDirection.y):flyingDirection;
        else if(flyingDirection.x >0)
            flyingDirection = flyingDirection.x > maxDistance ? new Vector2(maxDistance, flyingDirection.y):flyingDirection;

        if (flyingDirection.y < 0)
            flyingDirection = flyingDirection.y < -maxHeight ? new Vector2(flyingDirection.x, -maxHeight) : flyingDirection;
        else if (flyingDirection.y > 0)
            flyingDirection = flyingDirection.y > maxHeight ? new Vector2(flyingDirection.x, maxHeight) : flyingDirection;

    }

    public void Fill(int maxHeight, int maxDistance, Projectile projectile,float weaponDamage)
    {
        this.maxHeight = maxHeight;
        this.maxDistance = maxDistance;
        this.projectile = projectile;
        this.weaponDamage = weaponDamage;
        if (projectile.lightEmission != 0)
            gameObject.AddComponent<Light2D>().intensity = projectile.lightEmission;
        //Adding to bullet layer
        gameObject.AddComponent<Rigidbody2D>().gravityScale = projectile.gravityScale;
        gameObject.AddComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(this.gameObject.GetComponent<Collider2D>(), GlobalVariables.LocalPlayer.GetComponent<BoxCollider2D>());
        gameObject.layer = 11;
        gameObject.AddComponent<SpriteRenderer>().sprite = projectile.itemImage;
        CalcFlyingBehaviour();
        gameObject.GetComponent<Rigidbody2D>().AddForce(flyingDirection * 100 * projectile.flyingSpeed);
    }

    void Update()
    {
        //doesn't work
        Physics2D.IgnoreCollision(this.gameObject.GetComponent<Collider2D>(), GlobalVariables.LocalPlayer.GetComponentInChildren<BoxCollider2D>());
        
        //Despawn on hit....
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("HIT "+CalculatedDamage);
        EnemyBehaviour eb = collision.gameObject.GetComponentInChildren<EnemyBehaviour>();
        if (eb != null)
        {
            eb.Health -= (int)CalculatedDamage;
            InstantiateIndicator(collision.transform, (int)CalculatedDamage);
        }
        if(collision.gameObject.layer != 7)
            GameObject.Destroy(gameObject);
    }

    /// <summary>
	/// Instantiates the hit indicator
	/// </summary>
	/// <param name="mobT"></param>
	/// <param name="damage"></param>
	private void InstantiateIndicator(Transform mobT, int damage = -1)
    {
        Vector3 position = new Vector3(mobT.position.x - mobT.gameObject.GetComponent<BoxCollider2D>().size.x / 2, mobT.position.y + mobT.gameObject.GetComponent<BoxCollider2D>().size.y / 2);

        GameObject dmgIndicator = Instantiate(PrefabAssets.Singleton.DamageText, position, Quaternion.identity, mobT.transform);
        HitIndicator hitIndicator = dmgIndicator.GetComponent<HitIndicator>();
        hitIndicator.textmesh.text = string.Empty + damage;
        dmgIndicator.name = $"DamageIndicator-{damage}";
    }
}

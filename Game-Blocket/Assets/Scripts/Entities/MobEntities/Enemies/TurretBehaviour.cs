using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    [Range(1, 500)] [SerializeField]
    float healthPoints = 20f;
    float timeToLive = 30;
    float delay = 0.3f;
    float count = 0;
    GameObject player;
    double attackCooldownLeft;
    bool attackIsAllowed;
    public Sprite projectileSprite;
    static float localScaleX;


    [Range(0, 50)] [SerializeField]
    float attackDamage = 20f;
    [Range(3, 30)] [SerializeField]
    float projectileSpeed = 12f;
    [Range(0.01f, 4)] [SerializeField]
    float timeBeetweenEachNormalAttack = 3.2f;
    [Range(0, 20)] [SerializeField]
    float attackRadius = 10f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        attackCooldownLeft = timeBeetweenEachNormalAttack;
        attackIsAllowed = false;
        localScaleX = transform.localScale.x;
    }

    void Update()
    {
        if (attackCooldownLeft < 0)
        {
            attackIsAllowed = true;
            attackCooldownLeft = 0;
        }
        else attackCooldownLeft -= Time.deltaTime;
    }
    void FixedUpdate()
    {
        timeToLive -= Time.fixedDeltaTime;
        count += Time.fixedDeltaTime;

        if (count > delay)
        {

            if (timeToLive < 0)
            {
                Destroy(gameObject);
            }
        }


        float distanceBetweenPlayerAndMe = Vector2.Distance(transform.position, player.transform.position);
        if ((player.transform.position.x - transform.position.x) < 0)
        {
            transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
        }
        else transform.localScale = new Vector3(localScaleX * -1, transform.localScale.y, transform.localScale.z);

        if (distanceBetweenPlayerAndMe < attackRadius)
        {
            Attack();

        }
    }

    void Attack()
    {
        if (attackIsAllowed)
        {
            GameObject projectile = new GameObject();

            projectile.transform.position = transform.position;
            projectile.AddComponent<Rigidbody2D>();
            projectile.AddComponent<SpriteRenderer>();
            projectile.AddComponent<CircleCollider2D>();
            projectile.AddComponent<BulletHandler>();

            projectile.GetComponent<Rigidbody2D>().gravityScale = 0;
            projectile.GetComponent<CircleCollider2D>().isTrigger = true;
            projectile.GetComponent<SpriteRenderer>().sprite = projectileSprite;
            projectile.name = "Projectile";
            projectile.GetComponent<SpriteRenderer>().color = Color.green;
            //projectile.transform.SetParent(transform, true);

            Vector2 diff = player.transform.position - transform.position;
            if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) && Mathf.Abs(diff.x) > 1)
            {
                diff = diff / Mathf.Abs(diff.x);
            }
            else if (Mathf.Abs(diff.x) < Mathf.Abs(diff.y) && Mathf.Abs(diff.y) > 1)
                diff = diff / Mathf.Abs(diff.y);

            projectile.GetComponent<Rigidbody2D>().linearVelocity = (diff * projectileSpeed);

            addAttackCooldown(timeBeetweenEachNormalAttack);
        }
    }

    void addAttackCooldown(float seconds)
    {
        attackIsAllowed = false;
        attackCooldownLeft += seconds;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //deal dmg to player
        }

        if (collision.gameObject.tag == "Weapon")
        {
            //deal dmg to self
        }
    }

}

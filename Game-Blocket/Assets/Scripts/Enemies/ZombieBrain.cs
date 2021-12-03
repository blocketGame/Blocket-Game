using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBrain : MonoBehaviour
{
    [Range(1,20)]
    public float lineOfSite = 10f;
    [Range(0, 20)]
    public float lineOfMove = 1.45f;
    [Range(0, 20)]
    public float lineOfAttack = 1.5f;
    [Range(1, 20)]
    public float speed = 4f;

    private double activeCooldown;
    private bool attackAllowed;

    private int side = 0;
    private Transform player;
    
    void Start()
    {
        attackAllowed = true;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void TurnAnim()
    {
        if (gameObject.transform.localScale.x != side && side != 0
            && gameObject.transform.localScale.x < 1
            && gameObject.transform.localScale.x > -1)
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x + side * 0.05f, 1, 0);

    }
    void Update()
    {
        TurnAnim();
        if (activeCooldown > 0)
        {
            activeCooldown -= Time.deltaTime;
        }
        else if (activeCooldown > -1 && activeCooldown < 0)
        {
            attackAllowed = true;
        }

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceToPlayer > lineOfMove && distanceToPlayer < lineOfSite)
        {
            Vector2 vec = Vector2.MoveTowards(this.transform.position, new Vector2(player.position.x, transform.position.y), speed * Time.deltaTime);

            transform.position = vec;
            Debug.Log(side);
            
            if (player.position.x > this.transform.position.x)
            {
                //look to left dir
                if (side != 1)
                {
                    side = 1;
                    gameObject.transform.localScale = new Vector3(GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x + side * 0.05f, 1, 0);
                }
            }
            else
            {
                if (side != -1)
                {
                    side = -1;
                    gameObject.transform.localScale = new Vector3(GlobalVariables.LocalPlayer.GetComponentInChildren<SpriteRenderer>().gameObject.transform.localScale.x + side * 0.05f, 1, 0);
                }

            }



        }

        if (distanceToPlayer < lineOfAttack) {
            Attack();
        }
    }

    public void Attack() {
        if (attackAllowed) {
            //play animation attack 1
            //Player Health--
            Cooldown(1);
        }// Attack 1, 2 etc.
    }

    public void Cooldown(double waitInS)
    {
        activeCooldown = waitInS;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfMove);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lineOfAttack);
    }
}

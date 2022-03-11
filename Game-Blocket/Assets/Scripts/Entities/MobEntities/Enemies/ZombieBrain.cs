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
    [Range(1, 20)]
    public float JumpForce = 6f;

    private double animationCooldown;
    private double activeCooldown;
    private bool attackAllowed;
    private bool jumpAllowed;

   public Animator animator;

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
    void FixedUpdate()
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

        if (animationCooldown > 0)
        {
            animationCooldown -= Time.deltaTime;
        }
        else if (animationCooldown > -1 && animationCooldown < 0)
        {
            animator.SetBool("isNormalAttacking", false);
        }

        float distanceToPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceToPlayer > lineOfMove && distanceToPlayer < lineOfSite)
        {
            Vector2 vec = Vector2.MoveTowards(this.transform.position, new Vector2(player.position.x, transform.position.y), speed * Time.deltaTime);

            gameObject.transform.position = vec;
            /*
            if (!gameObject.transform.hasChanged)
            {
                Jump();
            }*/

            if(TerrainHandler.Singleton.GetBlockFormCoordinate(
                WorldData.Singleton.Grid.WorldToCell(new Vector3(gameObject.transform.position.x + (0.5f)*side, gameObject.transform.position.y, 0)).x,
                WorldData.Singleton.Grid.WorldToCell(new Vector3(gameObject.transform.position.x + side, gameObject.transform.position.y - 1, 0)).y)
                != 0){
                Jump();
            }

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
            animator.SetBool("isNormalAttacking", true);
            
            //animator.SetBool("isNormalAttacking", false);
            Cooldown(1);
        }// Attack 1, 2 etc.
    }

    public void Jump() {

        if (GetComponent<Rigidbody2D>().velocity.y == 0)
        {

            GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
            //play animation

            //animator.SetBool("isNormalAttacking", true);


        }
    }

    public void Cooldown(double waitInS)
    {
        animationCooldown = 0.25f;
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

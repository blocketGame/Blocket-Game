using UnityEngine;

/// <summary>
/// Trash
/// </summary>
public class BetterMovementScript : MonoBehaviour
{
   
   public float MovementSpeed = 300f;
    public float JumpForce = 6f;
    public float fallMulti = 1.06f;


    public BoxCollider2D Top;
    public BoxCollider2D Bot;
    public BoxCollider2D Above;

    //private bool jump = false;

    public Rigidbody2D playerRigidbody;


    //void Start()
    //{
    //    //Physics2D.IgnoreCollision(GetComponent<Collider2D>(), Bot.GetComponent<Collider2D>());
    //    //Physics2D.IgnoreCollision(GetComponent<Collider2D>(), Top.GetComponent<Collider2D>());
    //    //Physics2D.IgnoreCollision(GetComponent<Collider2D>(), Above.GetComponent<Collider2D>());
        
    //    rigidbody = GetComponentInParent<Rigidbody2D>();
    //}
    /**
    void Update()
    {
        if (false) //jump conditions
        {
            jump = true;
        }
    }*/

    void FixedUpdate()
    {
        //right,left movement

        var movement = Input.GetAxis("Horizontal");
        playerRigidbody.gameObject.transform.position += Time.deltaTime * MovementSpeed * new Vector3(movement, 0, 0);

        //jump
        if (playerRigidbody.velocity.y < 3 && Input.GetKey(KeyCode.Space))
        {
            playerRigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        }

        /**walk over block

        if (false)
        {


            Debug.Log("ccoliides");
            if (Top.isTrigger == false && Above.isTrigger == false)
            {
                transform.position = new Vector3(transform.position.x, (transform.position.y) + 1, transform.position.z);
            }
        }*/

        //fall
        if (playerRigidbody.velocity.y < 0)
        {
            if (playerRigidbody.velocity.y > -15)
            {
                playerRigidbody.gameObject.transform.position += Time.deltaTime * new Vector3(movement, (playerRigidbody.velocity.y) * fallMulti, 0);
            }
        }

    }
}

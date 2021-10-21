using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    [SerializeField]
    private World_Data w;
    public float MovementSpeed = 6f;
    public float JumpForce = 6f;
    public float fallMulti = 1.06f;

    private bool jump = false;

    private Rigidbody2D _rigidbody;

    public World_Data W { get => w; set => w = value; }
    public Rigidbody2D Rigidbody { get => _rigidbody; set => _rigidbody = value; }

    void Start() 
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (Input.GetButton("Jump") && Mathf.Abs(Rigidbody.velocity.y) < 0.001f)
        {
            jump = true;
        }
    }

    void FixedUpdate()
    {
        //right,left movement
        float thisX = transform.position.x;

        var movement = Input.GetAxis("Horizontal");
        transform.position += Time.deltaTime * MovementSpeed * new Vector3(movement, 0, 0);

        //jump
        if (jump) {
            Rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
            jump = false;
        }

        //walk over block
        if (W.Blocks[W.getBlockFormCoordinate((int) ((transform.position.x) - 0.5), (int) ((transform.position.y)-0.1))].BlockID != 0) {
            if (Mathf.Abs(Rigidbody.velocity.y) < 0.001f && W.Blocks[W.getBlockFormCoordinate((int)((transform.position.x) - 0.5), (int)((transform.position.y) + 1.1))].BlockID == 0) {
                transform.position = new Vector3(transform.position.x, (transform.position.y) + 1, transform.position.z);
            }
        } else
        if (W.Blocks[W.getBlockFormCoordinate((int) ((transform.position.x) + 0.5), (int)((transform.position.y)-0.1))].BlockID != 0) {        
            if (Mathf.Abs(Rigidbody.velocity.y) < 0.001f && W.Blocks[W.getBlockFormCoordinate((int)((transform.position.x) + 0.5), (int)((transform.position.y) + 1.1))].BlockID == 0) {            
                transform.position = new Vector3(transform.position.x, (transform.position.y) + 1, transform.position.z);
            }
                
        }

        //fall
        if (Rigidbody.velocity.y < 0) {
            if (Rigidbody.velocity.y > -15) {
                transform.position += Time.deltaTime * new Vector3(movement, (Rigidbody.velocity.y) * fallMulti, 0);
            }
        }
    }
}

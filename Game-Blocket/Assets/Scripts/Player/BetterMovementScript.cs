using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BetterMovementScript : MonoBehaviour
{

    public float MovementSpeed = 6f;
    public float JumpForce = 6f;
    public float fallMulti = 1.06f;
    private bool _jump;

    public bool belowTriggered;

    private Rigidbody2D rigidbody;


    void Start()
    {

        rigidbody = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jump = true;
        }
    }

        void FixedUpdate()
        {
            //right,left movement

            var movement = Input.GetAxis("Horizontal");
            transform.position += Time.deltaTime * MovementSpeed * new Vector3(movement, 0, 0);

            //jump
            if (_jump)
            {
                if (belowTriggered)
                {
                    rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);

                }
            }

            //walk over block


            //fall
            if (rigidbody.velocity.y < 0)
            {
                if (rigidbody.velocity.y > -15)
                {
                    transform.position += Time.deltaTime * new Vector3(movement, (rigidbody.velocity.y) * fallMulti, 0);
                }
            }

        }
    }


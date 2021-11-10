using System.Collections;
using System.Collections.Generic;

using MLAPI;
using MLAPI.Prototyping;

using UnityEngine;

/// <summary>
/// Trash
/// </summary>
public class MovementScript : NetworkBehaviour
{
	public float MovementSpeed = 6f;
	public float JumpForce = 6f;
	public float fallMulti = 1.06f;

	private bool jump = false;

	public new Rigidbody2D rigidbody;

	public NetworkTransform netTransform;

	//Not highercase because others 
	public new Transform transform { get => netTransform.transform; }

	void Update()
	{
		//GameObject player = GameObject.FindWithTag("Player").gameObject;
		if (Input.GetButton("Jump") && Mathf.Abs(rigidbody.velocity.y) < 0.001f)
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
			rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
			jump = false;
		}

		/*walk over block
		if (W.Blocks[W.GetBlockFormCoordinate((int) ((transform.position.x) - 0.5), (int) ((transform.position.y)-0.1))].BlockID != 0) {
			if (Mathf.Abs(Rigidbody.velocity.y) < 0.001f && W.Blocks[W.GetBlockFormCoordinate((int)((transform.position.x) - 0.5), (int)((transform.position.y) + 1.1))].BlockID == 0) {
				transform.position = new Vector3(transform.position.x, (transform.position.y) + 1, transform.position.z);
			}
		} else
		if (W.Blocks[W.GetBlockFormCoordinate((int) ((transform.position.x) + 0.5), (int)((transform.position.y)-0.1))].BlockID != 0) {        
			if (Mathf.Abs(Rigidbody.velocity.y) < 0.001f && W.Blocks[W.GetBlockFormCoordinate((int)((transform.position.x) + 0.5), (int)((transform.position.y) + 1.1))].BlockID == 0) {            
				transform.position = new Vector3(transform.position.x, (transform.position.y) + 1, transform.position.z);
			}
		}*/

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

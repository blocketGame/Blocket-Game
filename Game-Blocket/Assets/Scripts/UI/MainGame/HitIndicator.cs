using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public class HitIndicator : MonoBehaviour
{
	public TextMesh textmesh;

	public float SecondsUntilVanish => 1;
	public float DecreaseOpacity => 0.01f;

	public bool IsVanishing{ get; private set; }

	private Vector3 direction;

	// Start is called before the first frame update
	public void Start() {
		Random rnd = new Random();
		direction = new Vector3(rnd.Next(-10, 10), rnd.Next(0, 10))/100;
		Invoke(nameof(SetVanishing), SecondsUntilVanish);
	}
	
	public void SetVanishing() => IsVanishing = true;

	// Update is called once per frame
	public void FixedUpdate() {
		transform.position += direction;

		if(!IsVanishing)
			return;
		if(textmesh.color.a <= 0)
			Destroy(gameObject);
		else
			textmesh.color = new Color(textmesh.color.r, textmesh.color.g, textmesh.color.b, textmesh.color.a - DecreaseOpacity);
	}
}

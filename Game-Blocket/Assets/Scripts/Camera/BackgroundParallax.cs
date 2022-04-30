using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BackgroundParallax : ScriptableObject
{
	public string description;
	public List<ParalaxLayer> paralaxLayers = new List<ParalaxLayer>();
}

[Serializable]
public class ParalaxLayer{
	public string name;
	public float speed;
	public Sprite image;

	public float offsetX, offsetY;
}

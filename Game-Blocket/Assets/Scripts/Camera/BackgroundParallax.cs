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
	[Min(1)]
	public byte speed;
	public Sprite image;

	public float offsetX, offsetY;
}

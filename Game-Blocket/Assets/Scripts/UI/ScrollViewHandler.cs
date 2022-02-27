using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ScrollViewHandler : MonoBehaviour{
	[Header("Depedencies")]
	public GameObject contenGO;

	[Header("General Settings")]
	[Tooltip("If 0 there could be ulong.maxvalue")]
	public int maxItemsInContent;
	[Tooltip("If false it will throw nothing")]
	public bool throwWarningWhenFull;

	[Header("Display Settings")]
	[Tooltip("Show newest added element on top\n else show it on bottom")]
	public bool firstTop;

	public Queue<RectTransform> ContentQueue { get; } = new Queue<RectTransform>();

	/// <summary>
	/// All Elements must have the same hight
	/// </summary>
	/// <param name="toAdd"></param>
	public void Add(RectTransform toAdd){
		if (ContentQueue.Count > 0 && ContentQueue.Peek().rect.height != toAdd.rect.height){
			Rect lastElementRect = ContentQueue.Peek().rect;
			Debug.LogWarning($"All Heights must be ident! Last Rect-H: {lastElementRect.height}; This Rect-H: {toAdd.rect.height}");
			toAdd.rect.Set(lastElementRect.x, lastElementRect.y, lastElementRect.width, lastElementRect.height);
		}

		toAdd.SetParent(contenGO.transform);
		if (!firstTop)
			MoveAllElements(true);
		ContentQueue.Enqueue(toAdd);
		
		CheckQueue();
		
		Vector2 pos = firstTop ? new Vector2(0, ContentQueue.Count * toAdd.rect.height + toAdd.rect.height) : new Vector2(0, 0);
		

		toAdd.anchoredPosition = pos;
		
    }

	public void MoveAllElements(bool up){
		RectTransform[] rtA = ContentQueue.ToArray();
		for (int i = 0; i < rtA.Length; i++)
			rtA[i].position = up
                ? (Vector3)new Vector2(rtA[i].position.x, rtA[i].position.y + rtA[i].rect.height)
                : (Vector3)new Vector2(rtA[i].position.x, rtA[i].position.y - rtA[i].rect.height);
    }

	private void CheckQueue(){
		if (ContentQueue.Count > maxItemsInContent){
			Destroy(ContentQueue.Dequeue().gameObject);
			if (DebugVariables.ShowScrollViewInfo)
				Debug.Log("Destroyed ListContent");
        }
		Rect contentR = contenGO.GetComponent<RectTransform>().rect;
		contentR.Set(contentR.x, contentR.y, contentR.height, ContentQueue.Count() * ContentQueue.Peek().rect.height);

	}
}

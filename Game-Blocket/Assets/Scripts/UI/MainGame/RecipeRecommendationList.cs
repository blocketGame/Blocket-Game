using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeRecommendationList : MonoBehaviour
{
    public List<GameObject> currentRecommendations;

    public void PruneRecommendations()
    {
        foreach(GameObject g in currentRecommendations)
            GameObject.Destroy(g);
        currentRecommendations.Clear(); 
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

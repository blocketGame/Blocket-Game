using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private GameObject player;
    bool col = false;
    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        col = true;
    }



    private void OnTriggerExit2D(Collider2D other)
    {
        col = false;
    }

    private void FixedUpdate()
    {
        if (col) {
            player.GetComponent<BetterMovementScript>().belowTriggered = true;
        } else
            player.GetComponent<BetterMovementScript>().belowTriggered = false;
    }

    
}

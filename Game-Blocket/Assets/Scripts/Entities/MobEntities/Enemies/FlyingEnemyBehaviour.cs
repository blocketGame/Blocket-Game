using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behaviour for flying Enemies
/// </summary>
public class FlyingEnemyBehaviour : MonoBehaviour
{
    private Vector2 flyingDirection;
    private float[] flyingmodifyer = new float[2];
    private float counter=0;
    public TerrainChunk currentchunk;
    // Start is called before the first frame update
    void Start()
    {
        counter = 2;
        Rigidbody2D rb= gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        flyingDirection = new Vector2(transform.position.x + flyingmodifyer[0], transform.position.y + flyingmodifyer[1]) * 1.1f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position =Vector2.Lerp(transform.position, flyingDirection, Time.deltaTime);
        counter-=Time.deltaTime;
        if (counter <= 0)
        {
            flyingmodifyer[0] = (float)Random.Range(-2f, 2f);
            flyingmodifyer[1] = (float)Random.Range(-2f, 2f);
            flyingDirection = new Vector2(flyingDirection.x + flyingmodifyer[0], flyingDirection.y + flyingmodifyer[1]) * 1.1f;
            counter = 2;
        }
    }
}

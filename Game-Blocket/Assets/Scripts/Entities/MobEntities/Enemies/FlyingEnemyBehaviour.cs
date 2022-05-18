using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BlockData;

/// <summary>
/// Behaviour for flying Enemies
/// </summary>
public class FlyingEnemyBehaviour : EnemyBehaviour
{
    private Vector2 flyingDirection;
    private float[] flyingmodifyer = new float[2];
    private float counter=0;
    public TerrainChunk currentchunk;

    #region overwrittenFields
    private int damage;
    public override int Damage { get => damage; set => damage=value; }
    private int health;
    public override int Health { get => health; set => health = value; }
    private int maxHealth;
    public override int MaxHealth { get => maxHealth; set => maxHealth=value; }
    private int regeneration;
    public override int Regeneration { get => regeneration; set => regeneration=value; }
    private uint mobId;
    public override uint MobID { get=> mobId; set=>mobId=value; }

    [SerializeField]
    private List<BlockDropAble> drops;
    public override List<BlockDropAble> Drops { get => drops; set => drops = value; }
    #endregion

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

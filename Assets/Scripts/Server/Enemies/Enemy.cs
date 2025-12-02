using UnityEngine;
using Mirror;


public class Enemy : NetworkBehaviour
{
    protected const string PLAYER_TAG = "Player";

    protected Transform player1;
    protected Transform player2;
    protected Transform targetPlayer;

    [SerializeField]
    protected float rotationSpeed = 180f;
    [SerializeField]
    protected float moveSpeed = 1f;

    [SerializeField]
    private EnemyType enemyType;
    public EnemyType GetEnemyType {get {return enemyType;}}

    [SerializeField]
    protected float moveDistance = .25f;

    protected int spawnerId;

    public int SpawnerId {get {return spawnerId;} set{spawnerId = value;}}


    // Called when the enemy is instantiated
    protected virtual void Awake()
    {
        CachePlayers();
    }

    // Call this whenever a new player joins or spawns
    public virtual void OnPlayerConnected()
    {
        CachePlayers();
    }

    // Cache player transforms using the PLAYER_TAG
    protected void CachePlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(PLAYER_TAG);

        player1 = players.Length > 0 ? players[0].transform : null;
        player2 = players.Length > 1 ? players[1].transform : null;
    }

    protected Vector3 GetRotationDirectionToNearestPlayer()
    {
        targetPlayer = GetNearestPlayer();
        if (targetPlayer == null)
        {
            return transform.up;
            
        }

        Vector3 dir = (targetPlayer.position - transform.position).normalized;
        return dir;
        
    }



    protected virtual void Update()
    {

    }

    // Determine which player is closest
    protected Transform GetNearestPlayer()
    {
        if (player1 == null && player2 == null) return null;
        if (player1 != null && player2 == null) return player1;
        if (player1 == null && player2 != null) return player2;

        float dist1 = Vector3.Distance(transform.position, player1.position);
        float dist2 = Vector3.Distance(transform.position, player2.position);

        return dist1 < dist2 ? player1 : player2;
    }

 

    // Virtual methods for subclasses to override
    public virtual void ResetState() { }

    public virtual void Initialize(Vector2 direction) { }

    public virtual void TakeDamage() { }
}

using UnityEngine;
using Mirror;


public class Enemy : NetworkBehaviour
{
    protected const string PLAYER_TAG = "Player";
    protected enum EnemyState { Deactive, InitialWayPoint, Spawning, Idle, Moving }
    protected EnemyState currentState = EnemyState.Deactive;
    protected Transform player1;
    protected Transform player2;
    protected Transform targetPlayer;

    protected Vector3 initialWayPoint;

    protected Vector2 direction;

    [SerializeField]
    protected float rotationSpeed = 180f;
    [SerializeField]
    protected float moveSpeed = 1f;

    [SerializeField]
    private EnemyType enemyType;
    public EnemyType GetEnemyType {get {return enemyType;}}

    private Rigidbody2D rb;

    [SerializeField]
    protected float moveDistance = .25f;

    protected int spawnerId;

    public int SpawnerId {get {return spawnerId;} set{spawnerId = value;}}

    protected float internalTime = 0;

    protected float distance;

    protected Vector2 startPosition;

        private const string NON_COLLISION_LAYER = "NonCollision";
    static Vector3 ZERO_VECTOR = Vector3.zero;
    private const string ENEMY_LAYER = "EnemyHitBox";
    private Vector3 GetSlightlyOffsetDirection(Vector3 originalDir, float maxOffsetDeg = 25f)
    {
        // Pick a tiny random offset in degrees
        float offset = Random.Range(-maxOffsetDeg, maxOffsetDeg);

        // Rotate only around Z axis (2D)
        return Quaternion.Euler(0, 0, offset) * originalDir;
    }
    public virtual void FixedUpdate()
    {
        switch (currentState){
            case EnemyState.Spawning :
            {
                internalTime = 0;
                gameObject.layer = LayerMask.NameToLayer(NON_COLLISION_LAYER);
                currentState = EnemyState.InitialWayPoint;
                Vector3 start = rb.position;
                Vector3 directionToWaypoint = (initialWayPoint - start).normalized;
                direction = directionToWaypoint;
                distance = Vector3.Distance(start, initialWayPoint) + (Random.value * .5f);
                EnemyServerSpawnerManager.Instance.StartEnemyMove(this, directionToWaypoint, distance);

                startPosition = rb.position;
                DoNextStepToWayPoint();
                return;
            }
            case EnemyState.InitialWayPoint :
            {
                  DoNextStepToWayPoint();
                  return;  
            }
            case EnemyState.Idle :
            {
                direction = RecalibrateDirection();
                internalTime = 0;
                distance = RecalibrateDistance();
                currentState = EnemyState.Moving;
                startPosition = rb.position;
                EnemyServerSpawnerManager.Instance.StartEnemyMove(this, direction, distance);

                DoNextStepLive();
                return;

            }
            case EnemyState.Moving :
            {
                DoNextStepLive();
                return;

            }
        }
    }

    protected virtual void ArrivedAtDestination(){}

    protected virtual Vector2 RecalibrateDirection()
    {
        Vector3 playerDirection = GetRotationDirectionToNearestPlayer();
        return  GetSlightlyOffsetDirection(playerDirection);
    }

    protected virtual float RecalibrateDistance()
    {
        float randomOffset = (Random.value * .5f);
        return moveDistance + randomOffset;
    }

    protected virtual void DoNextStepToWayPoint()
    {   
 
        Debug.Log("server fixed update");
        // Normalized direction provided by server AI logic
        Vector2 dirNorm = direction.normalized;

        float travelTime = distance / moveSpeed;
        Vector2 end = startPosition + dirNorm * distance;
        if(internalTime >= travelTime)
        {
            gameObject.layer = LayerMask.NameToLayer(ENEMY_LAYER);
            currentState = EnemyState.Idle;
            rb.MovePosition(end);
            EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);
            return;
        }
        takeNextStep(travelTime, end);
    }
    protected virtual void DoNextStepLive()
    {
        Vector2 dirNorm = direction.normalized;

        float travelTime = distance / moveSpeed;
        Vector2 end = startPosition + dirNorm * distance;
        if(internalTime >= travelTime)
        {
            currentState = EnemyState.Idle;
            rb.MovePosition(end);
            EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);
            ArrivedAtDestination();
            return;
        }
        takeNextStep(travelTime, end);
    }

    private void takeNextStep(float travelTime, Vector3 end)
    {
        float t = internalTime / travelTime;
        Vector2 newPos = Vector2.Lerp(startPosition, end, t);

        rb.MovePosition(newPos);

        internalTime += Time.fixedDeltaTime;
    }



    // Called when the enemy is instantiated
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
    public virtual void ResetState()
    {

        currentState = EnemyState.Spawning;
        internalTime = 0;
    }

    public virtual void Initialize(Vector2 direction, Vector3 startPosition) { 
        if(rb!= null)
        {   
            rb.position = startPosition;
        }
        transform.position = startPosition;
        initialWayPoint = direction;
        currentState = EnemyState.Spawning;
        internalTime = 0; 
    }

    public virtual void TakeDamage() { }
}

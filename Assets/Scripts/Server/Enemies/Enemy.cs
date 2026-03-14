using UnityEngine;
using Mirror;
using TMPro;


public class Enemy : NetworkBehaviour, IDamagable, IEnemy
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

    [SerializeField]
    protected LayerMask wallLayer;

    private bool nextMoveBumped = false;

    protected int spawnerId;

    public int SpawnerId {get {return spawnerId;} set{spawnerId = value;}}

    protected float internalTime = 0;

    protected float distance;

    protected Vector2 startPosition;

    protected const string NON_COLLISION_LAYER = "NonCollision";

    private BoxCollider2D collider;

    public const string WALL="Wall";
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
                float adjustedDistance = CheckWallHit(distance);

                if (adjustedDistance < distance)
                {
                    distance = adjustedDistance;
                    nextMoveBumped = true;
                }
                currentState = EnemyState.Moving;
                startPosition = rb.position;
                //EnemyServerSpawnerManager.Instance.StartEnemyMove(this, direction, distance);
                DoInitialStepAfterRecalibration();
                //DoNextStepLive();
                return;

            }
            case EnemyState.Moving :
            {
                DoNextStepLive();
                return;

            }
        }
    }
    
    private void DrawSquare(Vector2 center, float size, Color color)
    {
        float half = size * 0.5f;

        Vector3 bl = new Vector3(center.x - half, center.y - half);
        Vector3 br = new Vector3(center.x + half, center.y - half);
        Vector3 tr = new Vector3(center.x + half, center.y + half);
        Vector3 tl = new Vector3(center.x - half, center.y + half);

        Debug.DrawLine(bl, br, color, 5f);
        Debug.DrawLine(br, tr, color, 5f);
        Debug.DrawLine(tr, tl, color, 5f);
        Debug.DrawLine(tl, bl, color, 5f);
    }

    private float CheckWallHit(float movementDistance)
    {
        Vector2 size = collider.size * transform.localScale;
        float angle = transform.eulerAngles.z;
        Vector2 origin = transform.position;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, direction, movementDistance, wallLayer);

        if (hit.collider != null && hit.collider.CompareTag(WALL))
        {
            return Mathf.Max(0, hit.distance - 0.1f);
        }

        return movementDistance;
    }

    protected virtual void DoInitialStepAfterRecalibration()
    {
        
        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, direction, distance);
        DoNextStepLive();
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
            if (nextMoveBumped)
            {
                DoWallBump();
                nextMoveBumped = false;
            }
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

        internalTime += Time.deltaTime;
    }



    // Called when the enemy is instantiated
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
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
        nextMoveBumped = false;
        transform.position = startPosition;
        initialWayPoint = direction;
        currentState = EnemyState.Spawning;
        internalTime = 0; 
    }

    public virtual void TakeDamage() { }

    public virtual void DoWallBump()
    {
        currentState = EnemyState.Idle;
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);
        ArrivedAtDestination();
    }

    public void ExitWallBump(Vector3 bumpedPosition, Vector2 contactVector)
    {
        return;
    }
}

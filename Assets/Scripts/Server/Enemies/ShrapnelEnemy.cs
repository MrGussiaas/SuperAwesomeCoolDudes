
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShrapnelEnemy : Enemy, IEnemy
{

    private Vector3[] cardinalDirs = {
        Vector3.up,
        Vector3.right,
        Vector3.down,
        Vector3.left
    };

    // Start is called before the first frame update
    int health = 1;

    private Rigidbody2D rb;

    private Vector3 initialBegin;

    private bool active = false;

    private bool duringInitial = false;
    private bool interrupted = false;

    public void DoWallBump(Vector3 bumpedPosition, Vector2 bumpedNormal)
    {
        //throw new System.NotImplementedException();
    }

    protected override Vector2 RecalibrateDirection()
    {
        Vector3 start = rb.position;
        Vector3 directionToWaypoint = (initialWayPoint - initialBegin).normalized;
        return SelectRandomAfterWayPointDirection(SnapToCardinal(directionToWaypoint));
    }

    protected override void ArrivedAtDestination()
    {
        BulletServerManager.Instance.SpawnShrapnelOnServer(this.transform.position);
        EnemyServerSpawnerManager.Instance.ReleaseEnemy(this);
    }

    protected override float RecalibrateDistance()
    {
        return 1.5f;
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Initialize(Vector2 direction, Vector3 position)
    {
        base.Initialize(direction, position);
        initialWayPoint = direction;
        currentState = EnemyState.Spawning;
        internalTime = 0;
        initialBegin = position;
        
    }

    public override void ResetState()
    {
        base.ResetState();
        health = 1;
        interrupted = false;
        transform.rotation = Quaternion.identity;
        //EnemyServerSpawner.Instance.RpcUpdateEnemyVisual(gameObject.GetInstanceID(), rotateTo);
        active = true;
    }



    private Vector2 SnapToCardinal(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }

    private Vector2 SelectRandomAfterWayPointDirection(Vector2 wayPointDirection)
    {
        int flag = Random.Range(0,2);
        if(wayPointDirection.y != 0)
        {
            return flag == 0 ? Vector2.left : Vector2. right;
        }
        return flag == 0 ? Vector2.down : Vector2.up;
    }

    
    public override void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            active = false;
            EnemyServerSpawnerManager.Instance.ReleaseEnemy(this);
        }
}
}
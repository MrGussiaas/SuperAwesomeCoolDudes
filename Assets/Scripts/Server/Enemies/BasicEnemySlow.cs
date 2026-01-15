
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicEnemySlow : Enemy, IEnemy
{
    int health = 1;
    

    private Rigidbody2D rb;

    private bool active = false;

    private bool interrupted = false;


    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            active = false;
            currentState = EnemyState.Deactive;
            EnemyServerSpawnerManager.Instance.ReleaseEnemy(this);
        }
    }

    public override void Initialize(Vector2 direction, Vector3 startPosition)
    {
        base.Initialize(direction, startPosition);

        
    }

    public override void ResetState()
    {
        base.ResetState();
        health = 1;
        currentState = EnemyState.Spawning;
        internalTime = 0;
    }

    public void DoWallBump(Vector3 bumpedPosition, Vector2 contactNormal)
    {
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, bumpedPosition, true);
        //float dot = Vector2.Dot(intendedDirection, contactNormal);
        //Vector3 slideDirection = (Vector3)((Vector2)intendedDirection - dot * contactNormal);
        //slideDirection.Normalize();
        interrupted = true;
        // Stop whatever coroutine is running

    }
}

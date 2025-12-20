
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicEnemySlow : Enemy, IEnemy
{
    int health = 1;

    private Rigidbody2D rb;

    private bool active = false;

    private Coroutine loopRoutine;

    private Coroutine initialWayPointRoutine;

    private Vector3 initialWayPoint;

    private bool duringInitial = false;
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
            

            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }
            if(initialWayPointRoutine != null)
            {
                StopCoroutine(initialWayPointRoutine);
                initialWayPointRoutine = null;
            }
            EnemyServerSpawnerManager.Instance.ReleaseEnemy(this);
        }
    }

    public override void Initialize(Vector2 direction)
    {
        base.Initialize(direction);
        initialWayPoint = direction;
        
    }

    public override void ResetState()
    {
        base.ResetState();
        health = 1;
        interrupted = false;
        transform.rotation = Quaternion.identity;
        //EnemyServerSpawner.Instance.RpcUpdateEnemyVisual(gameObject.GetInstanceID(), rotateTo);
        active = true;
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
        }

        if (initialWayPointRoutine != null)
        {
            StopCoroutine(initialWayPointRoutine);
        }
        initialWayPointRoutine = StartCoroutine(InitialWayPointRoutine());
    }

    private void RotateToo(Vector3 direction)
    {

    }


    private IEnumerator RotateTo(Vector3 direction)
    {
        // Ensure we stay in 2D
        direction.z = 0f;
        direction.Normalize();
        yield break;

        // Get current and target angles, correcting for "up" axis
        float startAngle = transform.eulerAngles.z;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 because up is forward

        // Find the shortest rotation delta
        float delta = Mathf.DeltaAngle(startAngle, targetAngle);

        if (Mathf.Abs(delta) < 2f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            yield break;
        }

        float duration = Mathf.Abs(delta) / rotationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        yield return new WaitForFixedUpdate();
    }


    
    private IEnumerator InitialWayPointRoutine()
    {
        yield return null;
        duringInitial = true;
        interrupted = false;
        Vector3 start = rb.position;
        Vector3 directionToWaypoint = (initialWayPoint - start).normalized;

        if (interrupted) yield break;
        
        
        float initialDistance = Vector3.Distance(start, initialWayPoint);
        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, directionToWaypoint, initialDistance);
        yield return StartCoroutine(MoveForward(directionToWaypoint, initialDistance));

        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);
        yield return new WaitForFixedUpdate();
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
        }
        loopRoutine = StartCoroutine(EnemyLoop());
        duringInitial = false;


    }

    private IEnumerator EnemyLoop()
    {
        // small delay to let players fully spawn
        interrupted = false;
        yield return new WaitForSeconds(0.1f);
        int enemyId = gameObject.GetInstanceID();
        while (active)
        {
            // 1. Rotate toward nearest player
            Vector3 direction = GetRotationDirectionToNearestPlayer();
            direction = GetSlightlyOffsetDirection(direction);
            // 2. Move forward (in "up" direction) a few units
            Vector3 start = rb.position;
            Vector3 worldUp = transform.up.normalized;
            Vector3 end = start + worldUp * moveDistance;
            EnemyServerSpawnerManager.Instance.StartEnemyMove(this, direction, moveDistance);

            yield return StartCoroutine(MoveForward(direction, moveDistance));
            EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);
            yield return new WaitForFixedUpdate();


            // 3. Repeat until enemy dies or is disabled
        }
    }
    
    private Vector3 GetSlightlyOffsetDirection(Vector3 originalDir, float maxOffsetDeg = 25f)
    {
        // Pick a tiny random offset in degrees
        float offset = Random.Range(-maxOffsetDeg, maxOffsetDeg);

        // Rotate only around Z axis (2D)
        return Quaternion.Euler(0, 0, offset) * originalDir;
    }
    
    private IEnumerator MoveForward(Vector2 direction, float distance)
    {
        Vector2 start = rb.position;

        // Normalized direction provided by server AI logic
        Vector2 dirNorm = direction.normalized;

        Vector2 end = start + dirNorm * distance;

        float travelTime = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            if (interrupted)
            {
                Debug.Log("interrupted enemy movement ending at: " + end);
                yield break;
            }

            float t = elapsed / travelTime;
            Vector2 newPos = Vector2.Lerp(start, end, t);

            rb.MovePosition(newPos);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(end);
    }

    public void DoWallBump(Vector3 bumpedPosition)
    {
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, bumpedPosition);
        interrupted = true;
        // Stop whatever coroutine is running
        if (initialWayPointRoutine != null)
        {
            StopCoroutine(initialWayPointRoutine);
            initialWayPointRoutine = null;
        }
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }

        // Restart appropriate routine
        if (duringInitial)
        {
            initialWayPointRoutine = StartCoroutine(InitialWayPointRoutine());
        }
        else
        {
            loopRoutine = StartCoroutine(EnemyLoop());
        }
    }
}

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

    private bool active = false;

    private Coroutine loopRoutine;

    private Coroutine initialWayPointRoutine;

    private Vector3 initialWayPoint;

    private bool duringInitial = false;
    private bool interrupted = false;

    public void DoWallBump(Vector3 bumpedPosition)
    {
        //throw new System.NotImplementedException();
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
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

    private IEnumerator MoveForward(float distance)
    {
        Vector3 start = rb.position;

        // Use world-space up direction normalized
        Vector3 worldUp = transform.up.normalized;

        Vector3 end = start + worldUp * distance;

        float travelTime = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            if (interrupted)
            {
                Debug.Log("interrupted enemy movement ending at: " + end);
                //EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, end);
                yield break;
            } 
            float t = elapsed / travelTime;
            Vector3 newPos = Vector3.Lerp(start, end, t);
            rb.MovePosition(newPos);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(end);
    }

    private IEnumerator RotateTo(Vector3 direction)
    {
        // Ensure we stay in 2D
        direction.z = 0f;
        direction.Normalize();

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
 
        EnemyServerSpawnerManager.Instance.StartRotation(this, directionToWaypoint);
        yield return StartCoroutine(RotateTo(directionToWaypoint));
        EnemyServerSpawnerManager.Instance.FinishRotation(this, directionToWaypoint);
        yield return new WaitForFixedUpdate();
        if (interrupted) yield break;
        
        
        float initialDistance = Vector3.Distance(start, initialWayPoint);
        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, initialDistance);
        yield return StartCoroutine(MoveForward(initialDistance));

        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);
        yield return new WaitForFixedUpdate();
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
        }
        Vector3 fwd = transform.up;
        Vector3 closestDir = Vector3.up;
        float maxDot = -Mathf.Infinity;

        foreach (var dir in cardinalDirs)
        {
            float d = Vector3.Dot(fwd, dir);
            if (d > maxDot)
            {
                maxDot = d;
                closestDir = dir;
            }
        }

        EnemyServerSpawnerManager.Instance.StartRotation(this, closestDir);
        yield return StartCoroutine(RotateTo(closestDir));
        EnemyServerSpawnerManager.Instance.FinishRotation(this, closestDir);

        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, .25f);
        yield return StartCoroutine(MoveForward(.25f));
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);

        int turn = Random.value < 0.5f ? -90 : 90;   // randomly choose +90 or -90 degrees
        Vector3 newDir = Quaternion.Euler(0, 0, turn) * closestDir;
        EnemyServerSpawnerManager.Instance.StartRotation(this, newDir);
        yield return StartCoroutine(RotateTo(newDir));
        EnemyServerSpawnerManager.Instance.FinishRotation(this, newDir);

        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, 1.25f);
        yield return StartCoroutine(MoveForward(1.25f));
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);

        BulletServerManager.Instance.SpawnShrapnelOnServer(this.transform.position);
        EnemyServerSpawnerManager.Instance.ReleaseEnemy(this);

        duringInitial = false;
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
}
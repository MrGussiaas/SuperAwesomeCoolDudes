
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

    private IEnumerator MoveForward(float distance, Vector2 direction)
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

    private IEnumerator InitialWayPointRoutine()
    {
        yield return null;
        duringInitial = true;
        interrupted = false;
        Vector3 start = rb.position;
        Vector3 directionToWaypoint = (initialWayPoint - start).normalized;

        yield return new WaitForFixedUpdate();
        if (interrupted) yield break;
        
        
        float initialDistance = Vector3.Distance(start, initialWayPoint);
        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, directionToWaypoint, initialDistance);
        yield return StartCoroutine(MoveForward(initialDistance, directionToWaypoint));

        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);
        yield return new WaitForFixedUpdate();
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
        }
        Vector3 fwd = transform.up;
        Vector3 closestDir = SnapToCardinal(directionToWaypoint);

        Debug.Log("randomlySelectedNextPosition " + closestDir);

        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, closestDir, .25f);
        yield return StartCoroutine(MoveForward(.25f, closestDir));
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position);

        Vector2 turnDirection = SelectRandomAfterWayPointDirection(SnapToCardinal(directionToWaypoint));

        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, turnDirection, 1.25f);
        yield return StartCoroutine(MoveForward(1.25f, turnDirection));
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
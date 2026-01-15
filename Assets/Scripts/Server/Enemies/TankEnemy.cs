using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : Enemy, IEnemy
{

    private Vector3[] cardinalDirs = {
        Vector3.up,
        Vector3.right,
        Vector3.down,
        Vector3.left
    };

    // Start is called before the first frame update
    int health = 1;

    private EnemyTurret turret;

    private Rigidbody2D rb;

    private bool active = false;

    private Coroutine loopRoutine;

    private Coroutine initialWayPointRoutine;

    private Vector3 initialWayPoint;

    private bool duringInitial = false;
    private bool interrupted = false;

    private const string TURRET = "Turret";

    public void DoWallBump(Vector3 bumpedPosition, Vector2 bumpNormal)
    {
        if (interrupted)
        {
            return;
        }
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, bumpedPosition, false);
        interrupted = true;
        if(loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
        StartCoroutine(ReverseDirection());
    }

    protected override void Awake()
    {
        base.Awake();
        InitVars();
    }

    private void InitVars()
    {
        rb = GetComponent<Rigidbody2D>();
        for(int i = 0, n = transform.childCount; i < n; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.CompareTag(TURRET))
            {
                turret = child.GetComponent<EnemyTurret>();
            }
        }
    }

    public override void Initialize(Vector2 direction, Vector3 position)
    {
        base.Initialize(direction, position);
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

    private IEnumerator ReverseDirection()
    {
        Vector3 newDir = -transform.up;
        EnemyServerSpawnerManager.Instance.StartRotation(this, newDir);
        yield return StartCoroutine(RotateTo(newDir));
        EnemyServerSpawnerManager.Instance.FinishRotation(this, newDir);
        interrupted = false;
        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, 2);
        yield return StartCoroutine(MoveForward(2));
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);
        yield return new WaitForFixedUpdate();
        loopRoutine = StartCoroutine(EnemyLoop());
        
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

    private Vector3 DirectionFromAngle(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
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
            int turn = Random.value < 0.5f ? -90 : 90;   // randomly choose +90 or -90 degrees
            Vector3 newDir = Quaternion.Euler(0, 0, turn) * transform.up;
            EnemyServerSpawnerManager.Instance.StartRotation(this, newDir);
            yield return StartCoroutine(RotateTo(newDir));
            EnemyServerSpawnerManager.Instance.FinishRotation(this, newDir);
            yield return new WaitForFixedUpdate();
            // 2. Move forward (in "up" direction) a few units
            Vector3 start = rb.position;
            Vector3 worldUp = transform.up.normalized;
            Vector3 end = start + worldUp * moveDistance;
            EnemyServerSpawnerManager.Instance.StartEnemyMove(this, moveDistance);

            yield return StartCoroutine(MoveForward(moveDistance));
            EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);
            yield return new WaitForFixedUpdate();
            Vector3 bulletTrajectory = DirectionFromAngle(0);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(bulletTrajectory));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);
            bulletTrajectory = DirectionFromAngle(45);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(DirectionFromAngle(45)));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);
            bulletTrajectory = DirectionFromAngle(90);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(DirectionFromAngle(90)));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);
            bulletTrajectory = DirectionFromAngle(135);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(DirectionFromAngle(135)));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);
            bulletTrajectory = DirectionFromAngle(180);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(DirectionFromAngle(180)));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);
            bulletTrajectory = DirectionFromAngle(225);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(DirectionFromAngle(225)));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);
            bulletTrajectory = DirectionFromAngle(0);
            EnemyServerSpawnerManager.Instance.StartEnemyAim(this, bulletTrajectory);
            yield return StartCoroutine(turret.AimAt(DirectionFromAngle(0)));
            EnemyServerSpawnerManager.Instance.FinishEnemyAim(this, bulletTrajectory);
            BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, bulletTrajectory);



            // 3. Repeat until enemy dies or is disabled
        }
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

        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);
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

        EnemyServerSpawnerManager.Instance.StartEnemyMove(this, 1.25f);
        yield return StartCoroutine(MoveForward(1.25f));
        EnemyServerSpawnerManager.Instance.FinishEnemyMove(this, transform.position, false);

        loopRoutine = StartCoroutine(EnemyLoop());
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
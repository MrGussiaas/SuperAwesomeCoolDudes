using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TankEnemy : Enemy, IEnemy
{

    private Vector3[] cardinalDirs = {
        Vector3.up,
        Vector3.right,
        Vector3.down,
        Vector3.left
    };

    private Vector3[] shotVectors;


    private enum internalStates  {ROTATING, DEFAULT, FINISHED_WAY_POINT_ROTATION, BUMPED_WALL, SHOOTING};

    private internalStates internalState = internalStates.DEFAULT;

    // Start is called before the first frame update
    int health = 1;

    private float internalTick = 0;

    private EnemyTurret turret;

    private Rigidbody2D rb;

    private bool active = false;

    private Coroutine loopRoutine;

    private Coroutine initialWayPointRoutine;

    private Vector3 initialWayPoint;

    private bool interrupted = false;

    private const string TURRET = "Turret";

    private Vector3 directionToRotate;
    private float startRotation;
    private float endRotation;
    private float rotationDuration;

    [SerializeField]
    private bool lockRotation;




    protected override void ArrivedAtDestination()
    {
        if(internalState == internalStates.BUMPED_WALL)
        {
            return;
        }
        internalState = internalStates.SHOOTING;
        EnemyServerSpawnerManager.Instance.StartEnemyAim(this, Vector3.zero);
        turret.StartFull360();
       // internalState = internalStates.ROTATING;
    }



    public override void DoWallBump()
    {
        internalState = internalStates.BUMPED_WALL;
        internalTick = 0;
        
        base.DoWallBump();

    }

    protected override void DoNextStepLive()
    {
        if(internalState == internalStates.DEFAULT){
            base.DoNextStepLive();
        }
        if(internalState == internalStates.ROTATING)
        {
            DoRotationStep();
        }
    }

    protected  void InitializeRotation(Vector2 newRotationVector)
    {
        RecalibrateInternalRotationVars(newRotationVector);
        internalState = internalStates.ROTATING;
        internalTick = 0;
    }

    protected Vector2 GetClosestCardinalDirection()
    {
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
        return closestDir;
    }

    protected override Vector2 RecalibrateDirection()
    {
        if(internalState == internalStates.BUMPED_WALL)
        {
          
            Vector3 newDir = -transform.up;
            newDir.z = 180;
            newDir.Normalize();
            internalTick = 0;
            RecalibrateInternalRotationVars(newDir);
            EnemyServerSpawnerManager.Instance.StartRotation(this, newDir);
            internalState = internalStates.ROTATING;
            return newDir;
        }
        else if(internalState == internalStates.FINISHED_WAY_POINT_ROTATION)
        {
            

            Vector2 closestDir = GetClosestCardinalDirection();
            EnemyServerSpawnerManager.Instance.StartRotation(this, closestDir);
            RecalibrateInternalRotationVars(closestDir);
            internalState = internalStates.ROTATING;
            internalTick = 0;
            return closestDir;
        }
        else
        {
             int turn = Random.value < 0.5f ? -90 : 90;   // randomly choose +90 or -90 degrees
            Vector3 newDir = Quaternion.Euler(0, 0, turn) * transform.up;
            newDir.z = 0;
            newDir.Normalize();
            RecalibrateInternalRotationVars(newDir);
            EnemyServerSpawnerManager.Instance.StartRotation(this, newDir);
            internalState = internalStates.ROTATING;
            internalTick = 0;
             return newDir;
        }
    }

    protected override void DoInitialStepAfterRecalibration()
    {

    }
    protected override void DoNextStepToWayPoint()
    {
        if(internalState != internalStates.ROTATING){
            base.DoNextStepToWayPoint();
            return;
        }
        
    }

    protected override void Awake()
    {
        base.Awake();
        shotVectors = new Vector3[]
        {
            DirectionFromAngle(0),
            DirectionFromAngle(45),
            DirectionFromAngle(90),
            DirectionFromAngle(135),
            DirectionFromAngle(180),
            DirectionFromAngle(225),
            DirectionFromAngle(0)
        };

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

    private  Vector3 RecalibrateInternalRotationVars(Vector3 rotationPoint)
    {
       // Vector3 start = rb.position;
       // directionToRotate = (rotationPoint - start).normalized;
       
        directionToRotate = rotationPoint;

        startRotation = transform.eulerAngles.z;
        endRotation = Mathf.Atan2(directionToRotate.y, directionToRotate.x) * Mathf.Rad2Deg - 90f; // subtract 90 because up is forward
        float delta = Mathf.DeltaAngle(startRotation, endRotation);
        rotationDuration = Mathf.Abs(delta) / rotationSpeed;
        if (Mathf.Abs(delta) < 2f)
        {
            //transform.rotation = Quaternion.Euler(0f, 0f, endRotation);
        }
        return directionToRotate; 
    }

    public override void FixedUpdate()
    {
        if(currentState == EnemyState.Spawning && internalState == internalStates.ROTATING && internalTick <= 0)
        {
            Vector3 start = rb.position;
            Vector3 vecToWayPoint = (initialWayPoint - start).normalized;
            vecToWayPoint.z = 0f;
            vecToWayPoint.Normalize();
            gameObject.layer = LayerMask.NameToLayer(NON_COLLISION_LAYER);
            RecalibrateInternalRotationVars(vecToWayPoint);
            EnemyServerSpawnerManager.Instance.StartRotation(this, vecToWayPoint);
            DoRotationStep();
            return;
        }
        else if(internalState == internalStates.ROTATING)
        {
            DoRotationStep();
        }
        else if(internalState == internalStates.SHOOTING)
        {

            if (turret.IsDoneAiming)
            {
                turret.FinishAim(this);
                //turret.InitiateAim(this, shotVectors[internalShotCounter % shotVectors.Length]);

                if (turret.StartNextStep())
                {
                    internalState = internalStates.DEFAULT;
                }
                BulletServerManager.Instance.SpawnEnemyBulletOnServer(turret.transform.position, turret.transform.up);
                return;   
            }
            turret.DoAimStep();

        }
        else
        {
           
          base.FixedUpdate(); 
        }

    }

    protected virtual void RotationFinishNotification(float angle )
    {
        
    }

    private void DoRotationStep()
    {
        if(internalTick >= rotationDuration)
        {
            transform.rotation = !lockRotation ? Quaternion.Euler(0f, 0f, endRotation) : Quaternion.Euler(0f, 0f, startRotation);
            internalTick = 0;
            internalState = internalStates.DEFAULT;
            if(currentState == EnemyState.Spawning)
            {
                internalState = internalStates.FINISHED_WAY_POINT_ROTATION;
            }
            else
            {
                EnemyServerSpawnerManager.Instance.StartEnemyMove(this, direction, distance);
            }
            RotationFinishNotification(endRotation);
            return;
        }
        internalTick+= Time.deltaTime;
        float t = internalTick / rotationDuration;
        float currentAngle = !lockRotation ? Mathf.LerpAngle(startRotation, endRotation, t) : startRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public override void ResetState()
    {
        base.ResetState();
        internalState = internalStates.ROTATING;
        health = 1;
        interrupted = false;
        transform.rotation = Quaternion.identity;

        active = true;

    }





    private Vector3 DirectionFromAngle(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrone : TankEnemy
{

    [SerializeField]
    private LaserBeam topLaser;

   [SerializeField]
    private LaserBeam bottomLaser;

       [SerializeField]
    private LaserBeam leftLaser;

    [SerializeField]
    private LaserBeam rightLaser;



    private enum internalStates  {INITIAL_WAYPOINT, SHOOTING, BUMPEDWALL, ROTATION_STEP1, ROTATION_STEP2,ROTATION_STEP2_INITIAL, ROTATION_STEP_COMPLETE};

    private internalStates internalState; 

    private float internalTick = 0;

    private BoxCollider2D collider;

    private Vector2 virtualUp = Vector2.up;

    private Vector2 previousUp = Vector2.up;


    // Start is called before the first frame update
    protected override void Awake()
    {
        initInternalVars();
        base.Awake();
    }

    private void initInternalVars()
    {
        collider = GetComponent<BoxCollider2D>();
    }


 
    // Update is called once per frame
    public override void FixedUpdate()
    {
        if(internalState == internalStates.SHOOTING)
        {
            CheckForLaserHit();
        }
        base.FixedUpdate();
    }

    private bool CheckForLaserHit()
    {
        if(GetFacingDirection() == Direction.North || GetFacingDirection() == Direction.South)
        {
            return rightLaser.CheckPlayerHit() ||  leftLaser.CheckPlayerHit();
        }
        else if (GetFacingDirection() == Direction.East || GetFacingDirection() == Direction.West)
        {
            return topLaser.CheckPlayerHit() || bottomLaser.CheckPlayerHit();
        }
        return false;
    }

    public Direction GetFacingDirection()
    {
        Vector3 forward = virtualUp;
        
        float northDot = Vector3.Dot(forward, Vector3.up);
        float southDot = Vector3.Dot(forward, Vector3.down);
        float eastDot = Vector3.Dot(forward, Vector3.right);
        float westDot = Vector3.Dot(forward, Vector3.left);

        float max = Mathf.Max(northDot, southDot, eastDot, westDot);

        if (max == northDot) return Direction.North;
        if (max == southDot) return Direction.South;
        if (max == eastDot)  return Direction.East;
        return Direction.West;
    }

    protected override  float RecalibrateDistance()
    {
        if(internalState == internalStates.ROTATION_STEP1)
        {
            return 0;
        }
        if(internalState == internalStates.ROTATION_STEP2 || internalState == internalStates.INITIAL_WAYPOINT)
        {
            return .25f;
        }
        if(internalState == internalStates.ROTATION_STEP_COMPLETE || internalState == internalStates.SHOOTING)
        {
            return 100;
        }
        return base.RecalibrateDistance();
    }

    protected override void RotationFinishNotification(float endAngle)
    {
        float rad = endAngle * Mathf.Deg2Rad;
        previousUp = virtualUp;
        virtualUp = new Vector2(-Mathf.Round(Mathf.Sin(rad)), Mathf.Round(Mathf.Cos(rad)));
        if(internalState == internalStates.ROTATION_STEP_COMPLETE)
        {
            internalState = internalStates.SHOOTING;
            ActivateLasers();
        }
    }

    private Vector2 ComputeRotationStep()
    {
        Vector2 size = collider.size * transform.localScale;
        float angle = transform.eulerAngles.z;
        Vector2 origin = transform.position;
        if(internalState == internalStates.ROTATION_STEP1 || internalState == internalStates.INITIAL_WAYPOINT)
        {
            int choice = Random.Range(0, 2);
            return  (virtualUp == Vector2.right || virtualUp == Vector2.left) ? (choice == 0 ?Vector2.up : Vector2.down) : (choice == 0 ?Vector2.left : Vector2.right);
        }
        return internalState == internalStates.ROTATION_STEP2 ? -previousUp : previousUp;
    }

    protected override Vector2 RecalibrateDirection()
    {
        
        Vector2 currentDir = virtualUp;
        Vector2 newOrientation = GetRandomReOrientation(currentDir);
        float alignment = Vector2.Dot(currentDir, newOrientation);
        if(internalState == internalStates.INITIAL_WAYPOINT)
        {
            if (alignment > 0.9f){
                base.InitializeRotation(newOrientation);
                internalState = internalStates.SHOOTING;
                ActivateLasers();
                return newOrientation;
            }
            else
            {
               
               
               newOrientation = ComputeRotationStep();
               internalState = internalStates.ROTATION_STEP2_INITIAL;
               EnemyServerSpawnerManager.Instance.StartRotation(this, newOrientation);
               base.InitializeRotation(newOrientation);
               return newOrientation;
            }
            base.InitializeRotation(newOrientation);
            return newOrientation;
        }
        if(internalState == internalStates.ROTATION_STEP1)
        {
            newOrientation = ComputeRotationStep();
            internalState = internalStates.ROTATION_STEP2;
            EnemyServerSpawnerManager.Instance.StartRotation(this, newOrientation);
            base.InitializeRotation(newOrientation);
            return newOrientation;
        }
        if(internalState == internalStates.ROTATION_STEP2 || internalState == internalStates.ROTATION_STEP2_INITIAL)
        {
            newOrientation = ComputeRotationStep();
            EnemyServerSpawnerManager.Instance.StartRotation(this, newOrientation);
            internalState = internalStates.ROTATION_STEP_COMPLETE;
            base.InitializeRotation(newOrientation);
            return newOrientation;
        }
        return base.RecalibrateDirection();
        //base.InitializeRotation(newOrientation);
        //return newOrientation;       
    }

    private void ActivateLasers()
    {
    
        if(GetFacingDirection() == Direction.East || GetFacingDirection() == Direction.West)
        {
            topLaser.StartShoot();
            bottomLaser.StartShoot();
        }
        else if(GetFacingDirection() == Direction.North || GetFacingDirection() == Direction.South)
        {
            leftLaser.StartShoot();
            rightLaser.StartShoot();
        }
        EnemyServerSpawnerManager.Instance.ActivateWeapon(this);
    }

    private void DeactivateLasers()
    {
        if(GetFacingDirection() == Direction.North || GetFacingDirection() == Direction.South)
        {
            leftLaser.EndShoot();
            rightLaser.EndShoot();
        }
        else if(GetFacingDirection() == Direction.East || GetFacingDirection() == Direction.West)
        {
            topLaser.EndShoot();
            bottomLaser.EndShoot();
        }
        EnemyServerSpawnerManager.Instance.DeactivateWeapon(this);
    }

    private Vector2 GetRandomReOrientation(Vector2 currentDir)
    {
        Vector2 snapped;

        if (Mathf.Abs(currentDir.x) > Mathf.Abs(currentDir.y))
            snapped = new Vector2(Mathf.Sign(currentDir.x), 0);
        else
            snapped = new Vector2(0, Mathf.Sign(currentDir.y));

        int choice = Random.Range(-1, 2);
        float angle = choice * 90f;

        return Quaternion.Euler(0, 0, angle) * snapped;
    }

    public override void DoWallBump()
    {
        internalState = internalStates.ROTATION_STEP1;
        DeactivateLasers();
        base.DoWallBump();
    }

    protected override void ArrivedAtDestination()
    {

       // base.ArrivedAtDestination();
    }

    public override void ResetState()
    {
        base.ResetState();
        internalState = internalStates.INITIAL_WAYPOINT;
        virtualUp = Vector2.up;
        previousUp = Vector2.up;
        transform.rotation = Quaternion.identity;
        internalTick = 0;
        base.ResetState();
    }

   

}

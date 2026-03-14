using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEnemyDrone : VisualEnemy
{

    [SerializeField]
    private VisualLaser topLaser;

    [SerializeField]
    private VisualLaser bottomLaser;

    [SerializeField]
    private VisualLaser leftLaser;

    [SerializeField]
    private VisualLaser rightLaser;

    private const string DRIVE_NORTH = "DriveNorth";

    private const string DRIVE_SOUTH = "DriveSouth";

    private const string DRIVE_WEST = "DriveWest";

    private const string DRIVE_EAST = "DriveEast";

    private const string ROTATE_NW="RotateNW";

    private const string ROTATE_WN = "RotateWN";

    private const string ROTATE_WS = "RotateWS";

    private const string ROTATE_SW = "RotateSW";

    private const string ROTATE_SE = "RotateSE";

    private const string ROTATE_ES = "RotateES";

    private const string ROTATE_EN = "RotateEN";

    private const string ROTATE_NE = "RotateNE";

    private Direction virtualUp = Direction.North;

    private Animator anim;

    protected override void Awake()
    {
        initVars();
        base.Awake();
    }

    public override void Begin()
    {
        virtualUp = Direction.North;
        base.Begin();
    }

    public override void ActivateWeapon()
    {
        if(virtualUp == Direction.East || virtualUp == Direction.West)
        {
            topLaser.ActivateLaser();
            bottomLaser.ActivateLaser();
        }
        else
        {
            rightLaser.ActivateLaser();
            leftLaser.ActivateLaser();  
        }

        base.ActivateWeapon();
    }

    public override void DeActivateWeapon()
    {
        topLaser.DeactivateLaser();
        bottomLaser.DeactivateLaser();
        rightLaser.DeactivateLaser();
        leftLaser.DeactivateLaser();
        base.DeActivateWeapon();
    }

    private void initVars()
    {
        anim = GetComponent<Animator>();
    }

    public override void RotateTo(Vector2 dir){
        dir.Normalize();
        Vector2 snapped = GetNextRotation(dir);

        string nexAnimationToPlay =GetNextRotationAnimation(snapped);
        if(nexAnimationToPlay != null){
            anim.Play(nexAnimationToPlay, 0, 0);
        }
    }

    private Vector2 GetNextRotation(Vector2 nextDirection)
    {

        if (Mathf.Abs(nextDirection.x) > Mathf.Abs(nextDirection.y))
        {
            return new Vector2(Mathf.Sign(nextDirection.x), 0f);
        }
        else
        {
            return new Vector2(0f, Mathf.Sign(nextDirection.y));
        }

    }

    private string GetNextRotationAnimation(Vector2 nextRotation)
    {
        
        switch (virtualUp)
        {
            case Direction.North:
                {
                    virtualUp = nextRotation == Vector2.up ? Direction.North : (nextRotation == Vector2.down ? Direction.South : (nextRotation == Vector2.left ? Direction.West : Direction.East));
                    return nextRotation == Vector2.up ? null : (nextRotation == Vector2.left ? ROTATE_NW : ROTATE_NE);
                }
            case Direction.West :
                {
                    virtualUp = nextRotation == Vector2.left ? Direction.West : (nextRotation == Vector2.right ? Direction.East : (nextRotation == Vector2.up ?  Direction.North : Direction.South));
                    return nextRotation == Vector2.left ? null :  (nextRotation == Vector2.up ? ROTATE_WN : ROTATE_WS);
                }
            case Direction.South :
            {
                virtualUp = nextRotation == Vector2.down ? Direction.South : (nextRotation == Vector2.up ? Direction.North : (nextRotation == Vector2.left ? Direction.West : Direction.East));
                return nextRotation  == Vector2.down ? null : (nextRotation == Vector2.left ? ROTATE_SW : ROTATE_SE);
            }
            case Direction.East :
                {
                    virtualUp = nextRotation == Vector2.right ? Direction.East : (nextRotation == Vector2.left ? Direction.West : (nextRotation == Vector2.down ? Direction.South : Direction.North));
                    return nextRotation ==  Vector2.down ? ROTATE_ES : ROTATE_EN;
                }

        }
        return null;
    }

    public override void MoveForward(Vector2 direction, float distance)
    {
        switch (virtualUp)
        {
            case Direction.North :
                {
                    anim.Play(DRIVE_NORTH,0, 0);
                    break;
                }
            case Direction.East:
                {
                    anim.Play(DRIVE_EAST,0, 0);
                    break;
                }
            case Direction.South :
                {
                    anim.Play(DRIVE_SOUTH,0,0);
                    break;
                }
            case Direction.West :
                {
                    anim.Play(DRIVE_WEST,0,0);
                    break;
                }
        }
        base.MoveForward(direction, distance);
    }

}

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class VisualEnemyTank : VisualEnemy
{

    private const string TURRET = "Turret";

    private const string ROTATION_N_W = "RotateN_W";

    private const string ROTATION_W_N = "RotateW_N";

    private const string ROTATION_W_S = "RotateW_S";

    private const string ROTATION_S_W = "RotateS_W";

    private const string ROTATION_S_E = "RotateS_E";

    private const string ROTATION_E_S = "RotateE_S";

    private const string ROTATION_E_N = "RotateE_N";

    private const string ROTATION_N_E = "RotateN_E";

    private const string ROTATION_N_S = "RotateN_S";

    private const string ROTATION_S_N = "RotateS_N";

    private const string ROTATION_W_E = "RotateW_E";

    private const string ROTATION_E_W = "RotateE_W";

    private const string DRIVE_N = "DriveNorth";

    private const string DRIVE_E = "DriveEast";

    private const string DRIVE_S = "DriveSouth";

    private const string DRIVE_W = "DriveWest";

    private Direction virtualUp = Direction.North;

    private Animator anim;

    private VisualEnemyTurret turret;

    private void initVars()
    {
        
        anim = GetComponent<Animator>();
        for(int i = 0, n = transform.childCount; i < n; i++)
        {
            GameObject gObj = transform.GetChild(i).gameObject;
            if (gObj.CompareTag(TURRET))
            {
                turret = gObj.GetComponent<VisualEnemyTurret>();
            }
        }
    }

    protected override void Awake()
    {
        initVars();
        base.Awake();
    }

    public override void StartAim(Vector3 dir)
    {
        base.StartAim(dir);
        turret.BeginAim(dir);
    }

    public override void FinishAim(Vector3 dir)
    {
        base.FinishAim(dir);
        turret.CompleteAim(dir);
    }

    public override void Begin()
    {
        virtualUp = Direction.North;
        turret.Begin();
        base.Begin();
    }

    private Vector2 GetNextRotation(Vector2 nextDirection)
    {

        if (Mathf.Abs(nextDirection.x) > Mathf.Abs(nextDirection.y))
        {
            // Horizontal is dominant
            return new Vector2(Mathf.Sign(nextDirection.x), 0f);
        }
        else
        {
            // Vertical is dominant
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
                    return nextRotation == Vector2.up ? null : (nextRotation == Vector2.down ? ROTATION_N_S : (nextRotation == Vector2.left ? ROTATION_N_W : ROTATION_N_E));
                }
            case Direction.West :
                {
                    virtualUp = nextRotation == Vector2.left ? Direction.West : (nextRotation == Vector2.right ? Direction.East : (nextRotation == Vector2.up ?  Direction.North : Direction.South));
                    return nextRotation == Vector2.left ? null : (nextRotation == Vector2.right ? ROTATION_W_E : (nextRotation == Vector2.up ? ROTATION_W_N : ROTATION_W_S));
                }
            case Direction.South :
            {
                virtualUp = nextRotation == Vector2.down ? Direction.South : (nextRotation == Vector2.up ? Direction.North : (nextRotation == Vector2.left ? Direction.West : Direction.East));
                return nextRotation  == Vector2.down ? null : (nextRotation == Vector2.up ? ROTATION_S_N : (nextRotation == Vector2.left ? ROTATION_S_W : ROTATION_S_E));
            }
            case Direction.East :
                {
                    virtualUp = nextRotation == Vector2.right ? Direction.East : (nextRotation == Vector2.left ? Direction.West : (nextRotation == Vector2.down ? Direction.South : Direction.North));
                    return nextRotation == Vector2.right ? null : (nextRotation == Vector2.left ? ROTATION_E_W : (nextRotation == Vector2.down ? ROTATION_E_S : ROTATION_E_N));
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
                    anim.Play(DRIVE_N,0, 0);
                    break;
                }
            case Direction.East:
                {
                    anim.Play(DRIVE_E,0, 0);
                    break;
                }
            case Direction.South :
                {
                    anim.Play(DRIVE_S,0,0);
                    break;
                }
            case Direction.West :
                {
                    anim.Play(DRIVE_W,0,0);
                    break;
                }
        }
        base.MoveForward(direction, distance);
    }

    public override void RotateTo(Vector2 dir){
        dir.Normalize();
        Vector2 snapped = GetNextRotation(dir);

        string nexAnimationToPlay =GetNextRotationAnimation(snapped);
        if(nexAnimationToPlay != null){
            turret.RotateTo(snapped);
            anim.Play(nexAnimationToPlay, 0, 0);
        }

       
    }
}

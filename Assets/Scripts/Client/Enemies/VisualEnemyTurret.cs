using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEnemyTurret : MonoBehaviour
{

    private Coroutine aimRoutine;

    private const string ROTATEN_N = "RotateN_N";

    private const string ROTATEW_W = "RotateW_W";

    private const string ROTATES_S = "RotateS_S";

    private const string ROTATEE_E="RotateE_E";

    private const string ROTATEE_W="RotateE_W";

    private const string ROTATEW_E="RotateW_E";

    private const string ROTATES_N="RotateS_N";

    private const string ROTATEN_S="RotateN_S";

    private const string ROTATEN_E="RotateN_E";

    private const string ROTATEE_N="RotateE_N";

    private const string ROTATEE_S="RotateE_N";

    private const string ROTATES_E="RotateS_E";

    private const string ROTATES_W="RotateS_W";

    private const string ROTATEW_S="RotateW_S";

    private const string ROTATEW_N="RotateW_N";

    private const string ROTATEN_W="RotateN_W";

    private Direction virtualUp = Direction.North;

    private Animator anim;


    // Start is called before the first frame update
    void Awake()
    {
        InitVars();
    }

    public void Begin()
    {
        virtualUp = Direction.North;
    }
    private void InitVars()
    {
        anim = GetComponent<Animator>();
    }

    public void RotateTo(Vector2 direction)
    {
        anim.Play(GetNextRotationAnimation(direction));
    }

    private string GetNextRotationAnimation(Vector2 nextRotation)
    {
        switch (virtualUp)
        {
            case Direction.North:
                {
                    virtualUp = nextRotation == Vector2.down ? Direction.South : (nextRotation == Vector2.left ? Direction.West : Direction.East);
                    return nextRotation == Vector2.down ? ROTATEN_S : (nextRotation == Vector2.left ? ROTATEN_W : ROTATEN_E);
                }
            case Direction.West :
                {
                    virtualUp = nextRotation == Vector2.right ? Direction.East : (nextRotation == Vector2.up ?  Direction.North : Direction.South);
                    return nextRotation == Vector2.right ? ROTATEW_E : (nextRotation == Vector2.up ? ROTATEW_N : ROTATEW_S);
                }
            case Direction.South :
            {
                virtualUp = nextRotation == Vector2.up ? Direction.North : (nextRotation == Vector2.left ? Direction.West : Direction.East);
                return nextRotation == Vector2.up ? ROTATES_N : (nextRotation == Vector2.left ? ROTATES_W : ROTATES_E);
            }
            case Direction.East :
                {
                    virtualUp = nextRotation == Vector2.left ? Direction.West : (nextRotation == Vector2.down ? Direction.South : Direction.North);
                    return nextRotation == Vector2.left ? ROTATEE_W : (nextRotation == Vector2.down ? ROTATEE_S : ROTATEE_N);
                }

        }
        return ROTATEN_W;
    }

    public void BeginAim(Vector3 direction)
    {
        switch(virtualUp)
        {
            case Direction.North :
                {
                    anim.Play(ROTATEN_N,0,0);
                    break;
                }
            case Direction.East :
                {
                    anim.Play(ROTATEE_E,0,0);
                    break;
                }
            case Direction.South :
                {
                    anim.Play(ROTATES_S,0,0);
                    break;
                }
            case Direction.West :
                {
                    anim.Play(ROTATEW_W,0,0);
                    break;
                }
        }
    }

    public void CompleteAim(Vector3 direction)
    {
        if(aimRoutine != null)
        {
            //StopCoroutine(aimRoutine);
            aimRoutine = null;
        }
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 because up is forward
        //transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

    }

    private IEnumerator AimAt(Vector3 direction)
    {
        direction.z = 0f;
        direction.Normalize();
        float rotationSpeed = 180;

        // Get current and target angles, correcting for "up" axis
        float startAngle = transform.eulerAngles.z;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 because up is forward

        // Find the shortest rotation delta
        float delta = Mathf.DeltaAngle(startAngle, targetAngle);

        if (Mathf.Abs(delta) < 2f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
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
}

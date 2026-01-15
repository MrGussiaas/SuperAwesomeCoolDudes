
using System.Collections;
using System.IO;
using Mirror.BouncyCastle.Crypto.Agreement.Srp;
using Unity.VisualScripting;
using UnityEngine;

public class VisualEnemy : MonoBehaviour
{
    private Vector2 targetDirection = Vector2.up;

    private Coroutine rotateTowards;

    private Coroutine moveTowards;

    private readonly WaitForFixedUpdate FIXED_UPDATE = new WaitForFixedUpdate();

    private bool arrived = false;
    

    protected float rotationSpeed = 180f;
    
    [SerializeField]
    protected float moveSpeed = 1f;

    protected float moveDistance = .25f;

    [SerializeField]
    private EnemyType enemyType;

    private Vector2 movingDirection;

    private float movingDistance;

    private float internalClock = 0;
    private Vector3 startPosition;


    public EnemyType GetEnemyType { get {return enemyType;}}

    public void FixedUpdate()
    {
        if(arrived) return;
        CalculateNextStep();

    }

    public Vector3 CalculateNextStep()
    {
        
        //Vector3 start = transform.position;
        Vector2 dirNorm = movingDirection.normalized;
        Vector3 end = startPosition + (Vector3)(dirNorm * movingDistance);
        float travelTime = movingDistance / moveSpeed;
        if(internalClock >= travelTime)
        {
            arrived = true;
            return end;
            
        }
        float t = internalClock / travelTime;
        Vector3 newPos = Vector3.Lerp(startPosition, end, t);
        transform.position = newPos;
        internalClock += Time.fixedDeltaTime;
   
        return newPos;
    }

    protected virtual void Awake()
    {

    }

    public void SetTargetDirection(Vector2 dir)
    {
        targetDirection = dir.normalized;
    }

    public virtual void RotateTo(Vector2 dir){
        if (rotateTowards != null)
        {
            StopCoroutine(rotateTowards);
        }
        rotateTowards = StartCoroutine(RotateTowards(dir));
    }

    public virtual void MoveForward(float distance)
    {
        if (moveTowards != null)
        {
            StopCoroutine(moveTowards);
        }
        moveTowards = StartCoroutine(AdvanceForward(distance));
    }

    public virtual void MoveForward(Vector2 direction, float distance)
    {
        if (moveTowards != null)
        {
            StopCoroutine(moveTowards);
        }
        arrived = false;
        internalClock = 0;
        movingDistance = distance;
        movingDirection = direction;
        startPosition = transform.position;
        //moveTowards = StartCoroutine(AdvanceForward(direction, distance));
    }

    public virtual void MoveForward(Vector3 startPosition, Vector2 direction, float distance)
    {
        Debug.Log("Moving visual forward");
        transform.position = startPosition;
        MoveForward(direction, distance);
    }

    private IEnumerator RotateTowards(Vector3 direction)
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
    }

    private IEnumerator AdvanceForward(Vector2 direction, float distance)
    {
            yield return FIXED_UPDATE;
            Vector3 start = transform.position;

            Vector2 dirNorm = direction.normalized;
            Vector3 end = start + (Vector3)(dirNorm * distance);

            float travelTime = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                float t = elapsed / travelTime;
                Vector3 newPos = Vector3.Lerp(start, end, t);
                transform.position = newPos;
                elapsed += Time.fixedDeltaTime;
                yield return FIXED_UPDATE;
            }

            transform.position = end;
    }

    
    private IEnumerator AdvanceForward(float distance)
    {
            yield return FIXED_UPDATE;
            Vector3 start = transform.position;

            // Use world-space up direction normalized
            Vector3 worldUp = transform.up.normalized;

            Vector3 end = start + worldUp * distance;

            float travelTime = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                float t = elapsed / travelTime;
                Vector3 newPos = Vector3.Lerp(start, end, t);
                transform.position = newPos;
                elapsed += Time.fixedDeltaTime;
                yield return FIXED_UPDATE;
            }

            transform.position = end;
    }

    public virtual void StartAim(Vector3 dir)
    {

    }
    

    public virtual void FinishAim(Vector3 dir)
    {

    }

    public virtual void FinishRotation(Vector3 dir){
        if(rotateTowards != null){
            StopCoroutine(rotateTowards);
            rotateTowards = null;
        }
        transform.up = dir;
        
    }

    public virtual void FinishMovement(Vector3 finalPosition, bool movementCancelled){

        if (moveTowards != null)
        {
            StopCoroutine(moveTowards);
            moveTowards = null;
        }
        transform.position = finalPosition;
        arrived = true;
        internalClock = 0;
    }

    private void OnDisable()
    {
        internalClock = 0;
        arrived = true;
        transform.position = Vector3.zero;
    }
}

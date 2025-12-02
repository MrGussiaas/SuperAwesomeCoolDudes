
using System.Collections;
using UnityEngine;

public class VisualEnemy : MonoBehaviour
{
    private Vector2 targetDirection = Vector2.up;

    private Coroutine rotateTowards;

    private Coroutine moveTowards;


    protected float rotationSpeed = 180f;
    
    [SerializeField]
    protected float moveSpeed = 1f;

    protected float moveDistance = .25f;

    [SerializeField]
    private EnemyType enemyType;

    public EnemyType GetEnemyType { get {return enemyType;}}

    void Update()
    {
        //float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f; // -90 because up is default
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 10f);
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
    
    private IEnumerator AdvanceForward(float distance)
    {
            yield return new WaitForFixedUpdate();
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
                yield return new WaitForFixedUpdate();
            }

            transform.position = end;
    }

    public virtual void StartAim(Vector3 dir)
    {
        Debug.Log("base start aim");
    }
    

    public virtual void FinishAim(Vector3 dir)
    {
        Debug.Log("base finish aim");
    }

    public virtual void FinishRotation(Vector3 dir){
        if(rotateTowards != null){
            StopCoroutine(rotateTowards);
            rotateTowards = null;
        }
        transform.up = dir;
        
    }

    public virtual void FinishMovement(Vector3 finalPosition){
        if (moveTowards != null)
        {
            StopCoroutine(moveTowards);
            moveTowards = null;
        }
        transform.position = finalPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEnemyTurret : MonoBehaviour
{

    private Coroutine aimRoutine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void BeginAim(Vector3 direction)
    {
        if(aimRoutine != null)
        {
            StopCoroutine(aimRoutine);
            aimRoutine = null;
        }
        aimRoutine = StartCoroutine(AimAt(direction));
    }

    public void CompleteAim(Vector3 direction)
    {
        if(aimRoutine != null)
        {
            StopCoroutine(aimRoutine);
            aimRoutine = null;
        }
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 because up is forward
        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

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

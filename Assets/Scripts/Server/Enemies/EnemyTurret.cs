using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{

    private bool isDoneAiming = true;
    public bool IsDoneAiming {get {return isDoneAiming;}}

    private int currentStep = 0;
    private const int steps = 8;   // 360 / 45
    private const float stepAngle = 45f; // CCW

    private float startRotation;
    private float endRotation;

    private float internalTick = 0;

    private float rotationSpeed = 90;

    private float aimDuration = 0;

    private Quaternion startQuat;

    // Start is called before the first frame update
    void Awake()
    {
        startQuat = transform.localRotation;
    }

    public void StartFull360()
    {
        currentStep = 0;
        StartNextStep();
    }

    public bool StartNextStep()
    {
        if (currentStep >= steps){
            transform.localRotation = startQuat;
            return true; // done 360
        }

        float targetRotation = transform.localEulerAngles.z + stepAngle;
        InitiateAimStep(targetRotation);
        return false;
    }

    private void InitiateAimStep(float targetLocalZ)
    {
        startRotation = NormalizeAngle(transform.localEulerAngles.z);
        endRotation = NormalizeAngle(targetLocalZ);
        internalTick = 0;
        aimDuration = Mathf.Abs(Mathf.DeltaAngle(startRotation, endRotation)) / rotationSpeed;
        isDoneAiming = false;
    }

    public void InitiateAim(Enemy enemy, Vector3 direction)
    {
        isDoneAiming = false;
        direction.z = 0f;
        direction.Normalize();

        // Get current and target angles, correcting for "up" axis
        startRotation = transform.localEulerAngles.z;
        endRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 because up is forward

        // Find the shortest rotation delta
        float delta = Mathf.DeltaAngle(startRotation, endRotation);

        if (Mathf.Abs(delta) < 2f)
        {
            //transform.rotation = Quaternion.Euler(0f, 0f, endRotation);
        }
        internalTick = 0;
        aimDuration = Mathf.Abs(delta) / rotationSpeed;
        EnemyServerSpawnerManager.Instance.StartEnemyAim(enemy, direction);
    }


    public void FinishAim(Enemy enemy)
    {
        internalTick = 0;
        transform.localRotation = Quaternion.Euler(0f, 0f, endRotation);
        EnemyServerSpawnerManager.Instance.FinishEnemyAim(enemy, transform.up);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    public void DoAimStep()
    {
        if (isDoneAiming)
            return;

        internalTick += Time.deltaTime;
        float t = internalTick / aimDuration;
        t = Mathf.Clamp01(t);
        float currentAngle = Mathf.LerpAngle(startRotation, endRotation, t);
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

        if (t >= 1f)
        {
            isDoneAiming = true;
            currentStep++;
        }

    }

    public IEnumerator AimAt(Vector3 direction)
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

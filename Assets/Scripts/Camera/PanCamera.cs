using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCamera : MonoBehaviour
{

    private const float PAN_TIME = .35f;

    // Start is called before the first frame update
    public void PanCameraTo(Vector3 newLocation)
    {
        StartCoroutine(PanCameraTowards(new Vector3(newLocation.x, newLocation.y, transform.position.z)));
    }

    private IEnumerator PanCameraTowards(Vector3 nextPosition)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < PAN_TIME)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / PAN_TIME;

            // Smooth step (better feel than raw linear)
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(start, nextPosition, t);

            yield return null;
        }

        // Snap to final just to ensure perfect accuracy
        transform.position = nextPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualLaser : MonoBehaviour
{

    private LineRenderer lr;

    [SerializeField]
    private LayerMask wallLayer;

    [SerializeField]
    private float castDistance = 4;

 

    private float GetDistanceToWAll()
    {
        Vector2 origin = transform.position;

        Vector2 dir = transform.right;
        Vector2 worldDir = transform.localRotation * Vector3.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, worldDir, castDistance, wallLayer);

        if (hit.collider != null && hit.collider.CompareTag(Constants.TAGS.WALL))
        {
            return Mathf.Max(0, hit.distance - 0.1f);
        }
        return castDistance;
    }

    // Start is called before the first frame update
    void Awake()
    {
        InitInternals();
    }

    // Update is called once per frame
    void InitInternals()
    {
        lr = GetComponent<LineRenderer>();
         
    }

    public void ActivateLaser()
    {
        lr.enabled = true;
    }

    public void DeactivateLaser()
    {
        lr.enabled = false;
    }

    public void Update()
    {
        if (!lr.enabled)
        {
            return;
        }

        float distanceToWAll = GetDistanceToWAll();
        lr.SetPosition(lr.positionCount - 1, new Vector2(distanceToWAll, 0));

    }
}

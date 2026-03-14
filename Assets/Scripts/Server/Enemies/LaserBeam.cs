using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Properties;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField]
    private float castDistance;

    [SerializeField]
    private Vector2 shootDirection;

    [SerializeField]
    private LayerMask playerLayer;

    private LineRenderer lineRenderer;


    public void Awake()
    {
        InitVars();
    }

    public void StartShoot()
    {
        //lineRenderer.enabled = true;
    }

    public void EndShoot()
    {
        //lineRenderer.enabled= false;
    }

    private void InitVars()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnDrawGizmos()
    {
        Vector3 localDir = transform.localRotation * Vector3.right;
        Vector3 start = transform.position;
        Vector3 end = start + (localDir * castDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);


        Vector3 boxScale = Vector3.Scale(Vector3.one * 0.1f, transform.lossyScale);
        Gizmos.DrawWireCube(end, boxScale);

    }

   public bool CheckPlayerHit()
    {
        Vector2 origin = transform.position;

        Vector2 dir = shootDirection.normalized;
        Vector2 worldDir = transform.localRotation * Vector3.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, worldDir, castDistance, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag(Constants.TAGS.PLAYER))
        {
            hit.collider.GetComponent<PlayerCollisions>().KillPlayer();
            return true;
        }

        return false;

    }



}

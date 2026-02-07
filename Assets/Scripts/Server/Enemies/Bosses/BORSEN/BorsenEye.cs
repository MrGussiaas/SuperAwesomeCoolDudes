using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class BorsenEye : NetworkBehaviour
{
    private enum SweepStates {IDLE, LEFT_TO_MID, MID_TO_RIGHT, RIGHT_TO_MID, MID_TO_LEFT}
    private SweepStates sweepStates = SweepStates.IDLE;
    private const float ANIMATION_FRAMES = 24;
    private const int ANIMATION_FPS = 60;

    private const string PLAYER = "Player";

    private static float ANIMATION_TIME = (ANIMATION_FRAMES / ANIMATION_FPS) / Borsen.ANIMATION_SPEED;

    private static float HALF_TIME = ANIMATION_TIME / 2;

    [SerializeField]
    private LayerMask playerLayer;

    [SerializeField]
    private Vector3 rightExtremeStart;
    [SerializeField]
    private Vector3 rightExtremeEnd;
    [SerializeField]
    private Vector3 midPointStart;
    [SerializeField]
    private Vector3 midPointEnd;
    [SerializeField]
    private Vector3 leftExtremeStart;
    [SerializeField]
    private Vector3 leftExtremeEnd;

    private float elapsedTime = 0;

    private LineRenderer lineRenderer;

    public void BeginLeftSweep()
    {
        elapsedTime = 0;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, leftExtremeStart);
        lineRenderer.SetPosition(1, leftExtremeEnd);
        sweepStates = SweepStates.LEFT_TO_MID;
    }
    public void BeginRightSweep()
    {
        lineRenderer.enabled = true;
        elapsedTime = 0;
        lineRenderer.SetPosition(0, rightExtremeStart);
        lineRenderer.SetPosition(1, rightExtremeEnd);
        sweepStates = SweepStates.RIGHT_TO_MID;
    }

    public void EndSweep()
    {
        elapsedTime = 0;
        lineRenderer.enabled = false;
        sweepStates = SweepStates.IDLE;
    }

    public void SyncMidPoint()
    {
        elapsedTime = 0;
        if(sweepStates == SweepStates.LEFT_TO_MID)
        {
            sweepStates = SweepStates.MID_TO_RIGHT;
            lineRenderer.SetPosition(0, midPointStart);
            lineRenderer.SetPosition(1, midPointEnd);
        }
        else if(sweepStates == SweepStates.RIGHT_TO_MID)
        {
            sweepStates = SweepStates.MID_TO_LEFT;
            lineRenderer.SetPosition(0, midPointStart);
            lineRenderer.SetPosition(1, midPointEnd);
        }
        if(isServer){
            CheckForPlayerHit(midPointStart, midPointEnd);
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void CheckForPlayerHit(Vector3 startPoint, Vector3 endPoint)
    {
        Vector2 worldStart = transform.TransformPoint(startPoint);
        Vector2 worldEnd   = transform.TransformPoint(endPoint);
        Vector2 dir   = (worldEnd - worldStart).normalized;
        float dist    = Vector2.Distance(worldStart, worldEnd);

        RaycastHit2D hit = Physics2D.Raycast(worldStart, dir, dist, playerLayer);
        if (hit.collider != null && hit.collider.CompareTag(PLAYER))
        {
            Transform root = hit.collider.transform;
            PlayerCollisions collisions = root.GetComponent<PlayerCollisions>();
            collisions.KillPlayer();
        }

    }

    private void Update()
    {
        if(sweepStates == SweepStates.IDLE)
        {
            return;
        }
        Vector3 startPoint = Vector3.zero;
        Vector3 endPoint = Vector3.zero;
        
        if(sweepStates == SweepStates.LEFT_TO_MID)
        {
            startPoint = Vector3.Lerp(leftExtremeStart, midPointStart, elapsedTime / HALF_TIME);
            endPoint = Vector3.Lerp(leftExtremeEnd, midPointEnd, elapsedTime / HALF_TIME);
            elapsedTime += Time.deltaTime;
        }
        else if(sweepStates == SweepStates.MID_TO_RIGHT)
        {
            startPoint = Vector3.Lerp(midPointStart, rightExtremeStart, elapsedTime / HALF_TIME);
            endPoint = Vector3.Lerp(midPointEnd, rightExtremeEnd, elapsedTime / HALF_TIME);
            elapsedTime += Time.deltaTime;
        }
        else if(sweepStates == SweepStates.RIGHT_TO_MID)
        {
            startPoint = Vector3.Lerp(rightExtremeStart, midPointStart, elapsedTime / HALF_TIME);
            endPoint = Vector3.Lerp(rightExtremeEnd, midPointEnd, elapsedTime / HALF_TIME);
            elapsedTime += Time.deltaTime;
        }
        else if(sweepStates == SweepStates.MID_TO_LEFT)
        {
            startPoint = Vector3.Lerp(midPointStart, leftExtremeStart, elapsedTime / HALF_TIME);
            endPoint = Vector3.Lerp(midPointEnd, leftExtremeEnd, elapsedTime / HALF_TIME);
            elapsedTime += Time.deltaTime;
        }
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        if(isServer){
            CheckForPlayerHit(startPoint, endPoint);
        }
    }

    private void OnDrawGizmos()
    {
        // Right extreme (red)
        DrawLaserPath(rightExtremeStart, rightExtremeEnd, Color.red);

        // Midpoint (yellow)
        DrawLaserPath(midPointStart, midPointEnd, Color.yellow);

        // Left extreme (cyan)
        DrawLaserPath(leftExtremeStart, leftExtremeEnd, Color.cyan);
    }

    private void DrawLaserPath(Vector3 localStart, Vector3 localEnd, Color color)
    {
        Vector3 worldStart = transform.TransformPoint(localStart);
        Vector3 worldEnd   = transform.TransformPoint(localEnd);

        Gizmos.color = color;

        // Line
        Gizmos.DrawLine(worldStart, worldEnd);

        // Start marker
        Gizmos.DrawSphere(worldStart, 0.05f);

        // End marker
        Gizmos.DrawSphere(worldEnd, 0.05f);
    }
}

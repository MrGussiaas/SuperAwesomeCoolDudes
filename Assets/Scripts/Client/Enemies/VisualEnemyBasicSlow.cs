using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class VisualEnemyBasicSlow : VisualEnemy
{

    SpriteRenderer sr;

    Animator anim;

    private const string NORTH = "NORTH";

    private const string NORTH_EAST = "NORTH_EAST";

    private const string EAST = "EAST";

    private const string SOUTH_EAST = "SOUTH_EAST";

    private const string SOUTH = "SOUTH";

    private void InitVars()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

private string GetDirectionTrigger(Vector2 dir)
{
    Vector2 normDir = dir.normalized;
    float angle = Mathf.Atan2(normDir.y, normDir.x) * Mathf.Rad2Deg;
    if (angle < 0) angle += 360f;
    if (angle >= 337.5f || angle < 22.5f) return EAST;
    if (angle >= 22.5f && angle < 67.5f) return NORTH_EAST;
    if (angle >= 67.5f && angle < 112.5f) return NORTH;
    if (angle >= 112.5f && angle < 157.5f) return NORTH_EAST;
    if (angle >= 157.5f && angle < 247.5f) return EAST;
    if (angle >= 247.5f && angle < 292.5f) return SOUTH;
    if (angle >= 292.5f && angle < 337.5f) return SOUTH_EAST;

    return SOUTH;
}

    private void Awake()
    {
        InitVars();
    }

    private bool NeedsFlip(Vector3 startPosition, Vector3 endPosition)
    {
        return endPosition.x < startPosition.x;
    }

    public override void MoveForward(Vector2 direction, float distance)
    {
        string trigger = GetDirectionTrigger(direction);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (Vector3)(direction.normalized * distance);
        sr.flipX = NeedsFlip(startPosition, endPosition);
        anim?.SetTrigger(trigger);
        base.MoveForward( direction,  distance);
    }
}

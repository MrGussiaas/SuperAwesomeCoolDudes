using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Billiards;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{

    private const string WALL = "Wall";

    private const string POWER_UP = "PowerUp";

    private BoxCollider2D boxCollider;

    private Rigidbody2D rb;

    private Vector3 correction = Vector3.zero;

    private bool needsCorrection = false;

    private float halfWidth;

    private float halfHeight;

    private float skin = 0.05f;

    private AbilityPickup abilityPickup;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        halfWidth = boxCollider.size.x / 2;
        halfHeight = boxCollider.size.y / 2;
        abilityPickup = GetComponent<AbilityPickup>();
    }

    private void FixedUpdate()
    {
        if (!needsCorrection)
        {
            return;
        }
        rb.MovePosition(correction);
        needsCorrection = false;
        
    }

    private void OnTriggerEnter2D(Collider2D wall)
    {
        if (wall.CompareTag(POWER_UP))
        {
            PowerUp powerUp = wall.GetComponent<PowerUp>();
            Debug.Log("Do Power up logic pickup here: " + powerUp.PowerUpType);
            abilityPickup.DoPickeup(powerUp.PowerUpType, powerUp.IntitaliAmmoAmount);
            powerUp.gameObject.SetActive(false);

        }
        if (!wall.CompareTag(WALL)) return;

        Bounds wallBounds = wall.bounds;
        Vector2 pos = transform.position;

        float dx = pos.x - wallBounds.center.x;
        float px = (wallBounds.extents.x + halfWidth) - Mathf.Abs(dx);

        float dy = pos.y - wallBounds.center.y;
        float py = (wallBounds.extents.y + halfHeight) - Mathf.Abs(dy);

        Vector2 normal = Vector2.zero;
        Vector2 correctedPos = pos;

        if (px < py)
        {
            // resolving along X axis
            normal = (dx > 0) ? Vector2.right : Vector2.left;
            correctedPos.x += normal.x * px;
        }
        else
        {
            // resolving along Y axis
            normal = (dy > 0) ? Vector2.up : Vector2.down;
            correctedPos.y += normal.y * py;
        }

        // Apply skin offset using YOUR method:
        correctedPos += normal * skin;
        correction = correctedPos;
        needsCorrection = true;
    }
}

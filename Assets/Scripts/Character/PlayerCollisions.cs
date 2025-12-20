using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Billiards;
using UnityEngine;

public class PlayerCollisions : NetworkBehaviour
{

    private const string WALL = "Wall";

    private const string POWER_UP = "PowerUp";

    private const string ENEMY = "Enemy";

    private const string ENEMY_BULLET = "EnemyBullet";

    private BoxCollider2D boxCollider;

    private Rigidbody2D rb;

    private Vector2 correction = Vector3.zero;

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

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!needsCorrection)
        {
            return;
        }
        rb.MovePosition(rb.position + correction);
        needsCorrection = false;
        correction = Vector3.zero;

        rb.MovePosition(rb.position + correction);
        
    }

    private void KillPlayer()
    {
        RoomController.ActiveRoom.RespawnPlayer(this.gameObject);
        //this.gameObject.SetActive(false);
        Debug.Log("Player killed");
    }
    

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D wall)
    {
        Debug.Log("Player triggered: " + wall.tag);
        if (wall.CompareTag(POWER_UP))
        {
            PowerUp powerUp = wall.GetComponent<PowerUp>();
            Debug.Log("Do Power up logic pickup here: " + powerUp.PowerUpType);
            abilityPickup.DoPickeup(powerUp.PowerUpType, powerUp.IntitaliAmmoAmount);
            GameEvents.PowerUpRemovedFromRoom(powerUp);
            

        }
        if (wall.CompareTag(ENEMY) || wall.CompareTag(ENEMY_BULLET))
        {
            KillPlayer();
        }

    }

    [ServerCallback]
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player collided with: " + collision.collider.tag);
        if (!collision.collider.CompareTag("Wall")) return;
        // Take the average normal of contacts
        Vector2 avgNormal = Vector2.zero;
        foreach (var c in collision.contacts)
            avgNormal += c.normal;
        avgNormal.Normalize();
        if(avgNormal == Vector2.zero)
        {
            return;
        }
        correction = avgNormal * skin;
        needsCorrection = true;
    }

}

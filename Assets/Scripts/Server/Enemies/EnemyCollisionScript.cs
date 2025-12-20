using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Billiards;
using UnityEngine;

public class EnemyCollisionScript : NetworkBehaviour
{

    private const string WALL = "Wall";

    private BoxCollider2D boxCollider;

    private Rigidbody2D rb;

    private Vector2 correction = Vector3.zero;

    private IEnemy iEnemy;

    private bool needsCorrection = false;

    private float halfWidth;

    private float halfHeight;

    [SerializeField]
    private float skin = 0.05f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        iEnemy = GetComponent<IEnemy>();
        rb = GetComponent<Rigidbody2D>();
        halfWidth = boxCollider.size.x / 2;
        halfHeight = boxCollider.size.y / 2;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!needsCorrection) return;

        rb.MovePosition(rb.position + correction);

        if (iEnemy != null)
            iEnemy.DoWallBump(rb.position);

        needsCorrection = false;
        correction = Vector3.zero;
        
    }

    [ServerCallback]
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Wall")) return;
        Debug.Log("Tank bumped into: " + collision.collider.tag);
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

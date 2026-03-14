using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Billiards;
using Unity.VisualScripting;
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


  


    
}

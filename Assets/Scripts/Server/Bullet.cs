using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{


    [SerializeField]
    private float bulletSpeed = 10;

    private Vector2 direction = Vector2.up;

    private Rigidbody2D rb;

    [SerializeField]
    BulletType bulletType;

    public BulletType GetBulletType {get {return bulletType;}}

        public bool IsSpawned { get; private set; }

    public void MarkSpawned(bool value)
    {
        IsSpawned = value;
    }


    // Start is called before the first frame update
    void Awake()
    {
        initProperties();
    }

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }



    public void ResetState()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.velocity = Vector2.zero;
        rb.position = Vector2.zero;
        transform.rotation = Quaternion.identity;
        direction = Vector2.up;
    }


    private void initProperties()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 newPos = rb.position + direction * bulletSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
    

    // Later when bullet expires or hits something:

}

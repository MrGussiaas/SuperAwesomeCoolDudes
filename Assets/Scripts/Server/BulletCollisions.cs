using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class BulletCollisions : NetworkBehaviour
{

    private const string WALL = "Wall";

    private const string BULLET_BARRIER = "BulletBarrier";

    private Bullet bullet;

    void Awake()
    {
        bullet = GetComponent<Bullet>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag(WALL) || collision.CompareTag(BULLET_BARRIER))
        {
            HandleWallCollision();
        }
        int id = collision.gameObject.GetInstanceID();
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null && enemy.isActiveAndEnabled)
        {
            BulletServerManager.Instance.ReleaseBullet(bullet);
            enemy.TakeDamage();
           
        }
    }

    private void HandleWallCollision()
    {
        BulletServerManager.Instance.ReleaseBullet(bullet);
    }


}

using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class EnemyBulletCollision : NetworkBehaviour
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
    }

    private void HandleWallCollision()
    {
        BulletServerManager.Instance.ReleaseBullet(bullet);
    }


}

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BulletServerManager : NetworkBehaviour
{
    public static BulletServerManager Instance { get; private set; }

    private readonly Dictionary<int, Bullet> activeVisuals = new();

    [SerializeField]
    private GameObject bulletServerPoolPrefab;

    [SerializeField]
    private GameObject shrapnelServerPoolPrefab;

    [SerializeField]
    private GameObject enemyBulletServerPoolPrefab;
    [SerializeField]
    private GameObject rocketBulletServerPoolPrefab;

    private BulletServerPool bulletServerPool;

    private BulletServerPool shrapnelServerPool;

    private BulletServerPool enemyBulletServerPool;

    private BulletServerPool rocketServerPool;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<BulletServerManager.RequestBulletSyncMessage>(OnRequestBulletSync);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;

        }
        Instance = this;

        // Instantiate the pool prefab under this object
        GameObject poolInstance = Instantiate(bulletServerPoolPrefab, transform);
        GameObject sharpnelInstance = Instantiate(shrapnelServerPoolPrefab, transform);
        GameObject enemyBulletPoolInstance = Instantiate(enemyBulletServerPoolPrefab, transform);
        GameObject rocketBulletPoolInstance = Instantiate(rocketBulletServerPoolPrefab, transform);
        bulletServerPool = poolInstance.GetComponent<BulletServerPool>();
        shrapnelServerPool = sharpnelInstance.GetComponent<BulletServerPool>();
        enemyBulletServerPool = enemyBulletPoolInstance.GetComponent<BulletServerPool>();
        rocketServerPool = rocketBulletPoolInstance.GetComponent<BulletServerPool>();

    }



    public void RegisterBullet(int netId, Bullet vb)
    {
        if (!activeVisuals.ContainsKey(netId))
            activeVisuals.Add(netId, vb);
    }

    public Bullet GetBullet(BulletType bulletType)
    {
        Bullet answer = null;
        switch (bulletType)
        {
            case BulletType.Basic :
                {
                    answer = bulletServerPool.Get();
                    break;
                }
            case BulletType.Shrapnel :
                {
                    answer = shrapnelServerPool.Get();
                    break;
                }
            case BulletType.EnemyBullet :
                {
                    answer = enemyBulletServerPool.Get();
                    break;
                }
            case BulletType.Rocket :
                {
                    answer = rocketServerPool.Get();
                    break;
                }
            default : break;
        }
        if(answer != null)
        {
            RegisterBullet(answer.gameObject.GetInstanceID(), answer);
        }
        return answer;
        
    }

    public Bullet GetBullet()
    {
        return GetBullet(BulletType.Basic);
    }

    public void ReleaseBullet(Bullet bullet)
    {
        Debug.Log("releasing bullet of type: " + bullet.GetBulletType);
        int instanceId = bullet.gameObject.GetInstanceID();
        DestroyBullet(bullet.GetBulletType, instanceId);
    }




    public Bullet GetBulletById(int netId)
    {
        activeVisuals.TryGetValue(netId, out var vb);
        return vb;
    }

    public Bullet GetPooledBullet()
    {
        return bulletServerPool.Get();
    }

    public void DestroyBullet(BulletType bulletType, int bulletId)
    {
        RpcDestroyVisualBullet(bulletId);
        if (activeVisuals.TryGetValue(bulletId, out var vb))
        {
            activeVisuals.Remove(bulletId);
        }
        else
        {
            return;
        }

        switch (bulletType){
            case BulletType.Basic :
                {
                    ReleaseBulletFromPool(bulletServerPool, vb);
                    break;
                }
            case BulletType.Shrapnel :
            {
                ReleaseBulletFromPool(shrapnelServerPool, vb);
                break;
            }
            case BulletType.EnemyBullet :
            {
                ReleaseBulletFromPool(enemyBulletServerPool, vb);
                break;
            }
            case BulletType.Rocket :
            {
                ReleaseBulletFromPool(rocketServerPool, vb);
                break;
            }
            default: break;
        }
    }

    void DestroyBullet(int bulletId)
    {
       
        DestroyBullet(BulletType.Basic, bulletId);
    }



    private void ReleaseBulletFromPool(BulletServerPool serverPool, Bullet b)
    {
        serverPool.Release(b);
    }

    [ClientRpc]
    void RpcDestroyVisualBullet(int bulletNetId)
    {
        VisualBulletManager.Instance.DestroyVisualBullet(bulletNetId);
    }

    [ClientRpc]
    public void RpcSpawnVisual(BulletType bulletType, Vector3 position, Vector2 dir, int bulletId)
    {
        VisualBullet vb = VisualBulletManager.Instance.GetPooledBullet(bulletType);
        // Position and orient it
        vb.transform.position = position;
        vb.transform.up = dir;

        // Reactivate and launch it
        vb.gameObject.SetActive(true);
        vb.Launch(dir);

        // Register it for later destruction
        VisualBulletManager.Instance.RegisterBullet(bulletId, vb);
    }

    [Server]
    public void SpawnEnemyBulletOnServer(Vector3 position, Vector2 direction)
    {
        Bullet bullet = GetBullet(BulletType.EnemyBullet); // from pool
        bullet.ResetState();
        bullet.Initialize(direction);
        bullet.transform.position = position;
        bullet.transform.up = direction;
        bullet.gameObject.SetActive(true);

        int bulletId = bullet.gameObject.GetInstanceID();

        RpcSpawnVisual(BulletType.EnemyBullet, position, direction, bulletId);
    }

    [Server]
    public void SpawnShrapnelOnServer(Vector3 position)
    {
        int count = 15;                // number of pieces
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;

            // Rotated direction (using Up as forward)
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;

            // Get shrapnel bullet from pool
            Bullet shrap = GetBullet(BulletType.Shrapnel);
            shrap.ResetState();
            shrap.Initialize(direction);
            shrap.transform.position = position;
            shrap.transform.up = direction;
            shrap.gameObject.SetActive(true);

            int bulletId = shrap.gameObject.GetInstanceID();

            // Spawn visual on clients
            RpcSpawnVisual(BulletType.Shrapnel, position, direction, bulletId);
        }
    }

    [Server]
    public void SpawnBulletOnServer(Vector3 position, Vector2 direction, BulletType bulletType = BulletType.Basic)
    {

        Bullet bullet = GetBullet(bulletType); // from pool
        bullet.ResetState();
        bullet.Initialize(direction);
        bullet.transform.position = position;
        bullet.transform.up = direction;
        bullet.gameObject.SetActive(true);

        int bulletId = bullet.gameObject.GetInstanceID();

        RpcSpawnVisual(bulletType, position, direction, bulletId);
    }

    private void OnRequestBulletSync(NetworkConnectionToClient conn, RequestBulletSyncMessage msg)
    {
        foreach (var kvp in activeVisuals)
        {
            Bullet bullet = kvp.Value;
            TargetSpawnVisual(conn, bullet.transform.position, bullet.transform.up, bullet.gameObject.GetInstanceID());
        }
    }

    [TargetRpc]
    private void TargetSpawnVisual(NetworkConnection conn, Vector3 position, Vector2 direction, int bulletId)
    {
        VisualBullet vb = VisualBulletManager.Instance.GetPooledBullet();

        vb.transform.position = position;
        vb.transform.up = direction;
        vb.gameObject.SetActive(true);
        vb.Launch(direction);

        VisualBulletManager.Instance.RegisterBullet(bulletId, vb);
    }


    
    public struct RequestBulletSyncMessage : NetworkMessage { }
}

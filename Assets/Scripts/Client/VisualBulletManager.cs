using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VisualBulletManager : MonoBehaviour
{
    public static VisualBulletManager Instance { get; private set; }

    private readonly Dictionary<int, VisualBullet> activeVisuals = new();

    [SerializeField]
    private GameObject visualManagerPoolPrefab;

    [SerializeField]
    private GameObject visualShrapnelPoolPrefab;

    [SerializeField]
    private GameObject visualEnemyPoolPrefab;

    [SerializeField]
    private GameObject visualRocketPoolPrefab;

    private BulletVisualPool bulletVisualPool;

    private BulletVisualPool shrapnelVisualPool;

    private BulletVisualPool enemyBulletVisualPool;

    private BulletVisualPool rocketVisualPool;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameObject poolInstance = Instantiate(visualManagerPoolPrefab, transform);
        GameObject shrapnelInstance = Instantiate(visualShrapnelPoolPrefab, transform);
        GameObject enemyBulletPoolInstance = Instantiate(visualEnemyPoolPrefab, transform);
        GameObject rocketBulletPoolInstance = Instantiate(visualRocketPoolPrefab, transform);
        bulletVisualPool = poolInstance.GetComponent<BulletVisualPool>();
        shrapnelVisualPool = shrapnelInstance.GetComponent<BulletVisualPool>();
        enemyBulletVisualPool = enemyBulletPoolInstance.GetComponent<BulletVisualPool>();
        rocketVisualPool = rocketBulletPoolInstance.GetComponent<BulletVisualPool>();
        DontDestroyOnLoad(gameObject);
    }

    public void ReleaseAll()
    {
        var keys = new List<int>(activeVisuals.Keys);
        foreach (var key in keys)
        {
            DestroyVisualBullet(key);
        }
        activeVisuals.Clear();
    }

    public void RegisterBullet(int netId, VisualBullet vb)
    {
        if (!activeVisuals.ContainsKey(netId))
            activeVisuals.Add(netId, vb);
    }

    public void DestroyVisualBullet(int netId)
    {
        if (activeVisuals.TryGetValue(netId, out var vb))
        {
            //Destroy(vb.gameObject);
            //bulletVisualPool.Release(vb);
            activeVisuals.Remove(netId);
            DestroyVisualBullet(vb.GetBulletType, vb);
        }
    }

    public void DestroyVisualBullet(BulletType bulletType, VisualBullet vb)
    {
        switch (bulletType)
        {
            case BulletType.Basic :
                {
                    DestroyVisualBullet(bulletVisualPool, vb);
                    break;
                }
            case BulletType.Shrapnel :
                {
                    DestroyVisualBullet(shrapnelVisualPool, vb);
                    break;
                }
            case BulletType.EnemyBullet :
                {
                    DestroyVisualBullet(enemyBulletVisualPool, vb);
                    break;
                }
            case BulletType.Rocket :
                {
                    DestroyVisualBullet(rocketVisualPool, vb);
                    break;
                }
            default:
                break;
        }
    }

    public void DestroyVisualBullet(BulletVisualPool pool, VisualBullet vb)
    {
        pool.Release(vb);
    }

    public VisualBullet GetBulletById(int netId)
    {
        activeVisuals.TryGetValue(netId, out var vb);
        return vb;
    }

    public VisualBullet GetPooledBullet()
    {
        return GetPooledBullet(BulletType.Basic);
    }

    public VisualBullet GetPooledBullet(BulletType bulletType)
    {
        switch (bulletType)
        {
            case BulletType.Basic : return bulletVisualPool.Get();
            case BulletType.Shrapnel : return shrapnelVisualPool.Get();
            case BulletType.EnemyBullet : return enemyBulletVisualPool.Get();
            case BulletType.Rocket : return rocketVisualPool.Get();
            default : return bulletVisualPool.Get();

        }
    }

    public void RequestServerSync()
    {
        if (NetworkClient.active)
        {
            NetworkClient.Send(new BulletServerManager.RequestBulletSyncMessage());
        }
    }
}

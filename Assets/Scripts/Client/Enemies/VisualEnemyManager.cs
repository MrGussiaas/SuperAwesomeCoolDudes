using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class VisualEnemyManager : MonoBehaviour
{

    [SerializeField]
    private GameObject visualEnemyPoolPrefab;

    [SerializeField]
    private GameObject visualEnemyFastPoolPrefab;

    [SerializeField]
    private GameObject visualEnemyShrapnelPoolPrefab;

    [SerializeField]
    private GameObject visualEnemyTankPoolPrefab;

    public static VisualEnemyManager Instance { get; private set; }

    private readonly Dictionary<int, VisualEnemy> activeVisuals = new();


    private VisualEnemyPool visualEnemyPool;

    private VisualEnemyPool visualEnemyFastPool;

    private VisualEnemyPool visualEnemyShrapnelPool;

    private VisualEnemyPool visualEnemyTankPool;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameObject poolInstance = Instantiate(visualEnemyPoolPrefab, transform);
        GameObject poolInstanceFast = Instantiate(visualEnemyFastPoolPrefab, transform);
        GameObject poolInstanceShrapnel = Instantiate(visualEnemyShrapnelPoolPrefab, transform);
        GameObject poolInstanceTank = Instantiate(visualEnemyTankPoolPrefab, transform);
        visualEnemyPool = poolInstance.GetComponent<VisualEnemyPool>();
        visualEnemyFastPool = poolInstanceFast.GetComponent<VisualEnemyPool>();
        visualEnemyShrapnelPool = poolInstanceShrapnel.GetComponent<VisualEnemyPool>();
        visualEnemyTankPool = poolInstanceTank.GetComponent<VisualEnemyPool>();
        DontDestroyOnLoad(gameObject);
    }

    public void ReleaseAll()
    {
        var keys = new List<int>(activeVisuals.Keys);
        foreach (var key in keys)
        {
            DestroyVisualEnemy(key);
        }
        activeVisuals.Clear();
    }


    public void RegisterEnemy(int netId, VisualEnemy vb)
    {
        if (!activeVisuals.ContainsKey(netId))
            activeVisuals.Add(netId, vb);
    }

    public void DestroyVisualEnemy(int netId)
    {
        if(activeVisuals.TryGetValue(netId, out var vb)){
            Debug.Log("releasing enemy of type: " + vb.GetEnemyType);
            DestroyVisualEnemy(vb.GetEnemyType,  netId);
            
        }
            
    }

    public void DestroyVisualEnemy(EnemyType enemyType, int netId)
    {
        if(!activeVisuals.TryGetValue(netId, out var vb)) return;
        switch (enemyType)
        {
            case EnemyType.Slow :
                {
                    PerformRelease(visualEnemyPool, vb);
                    activeVisuals.Remove(netId);
                    break;
                }
            case EnemyType.Fast :
                {
                    PerformRelease(visualEnemyFastPool, vb);
                    activeVisuals.Remove(netId);
                    break;
                }
            case EnemyType.Shrapnel :
                {
                    PerformRelease(visualEnemyShrapnelPool, vb);
                    activeVisuals.Remove(netId);
                    break;
                }
            case EnemyType.TankEnemy :
                {
                    PerformRelease(visualEnemyTankPool, vb);
                    activeVisuals.Remove(netId);
                    break;
                }
            default : break;
        }
    }


    public VisualEnemy GetEnemyById(int netId)
    {
        activeVisuals.TryGetValue(netId, out var vb);
        return vb;
    }

    public VisualEnemy GetPooledEnemy()
    {
        return GetPooledEnemy(EnemyType.Slow);
    }

    public VisualEnemy GetPooledEnemy(EnemyType type)
    {
        switch (type){
        
            case EnemyType.Slow:
                return visualEnemyPool.Get();
                break;
            case EnemyType.Fast:
                return visualEnemyFastPool.Get();
                break;
            case EnemyType.Shrapnel:
                return visualEnemyShrapnelPool.Get();
                break;
            case EnemyType.TankEnemy:
                return visualEnemyTankPool.Get();
                break;
            default :
                return visualEnemyPool.Get();
                break;
        }
        
    }

    public void PerformRelease(VisualEnemyPool enemyPool, VisualEnemy visualEnemy)
    {
        enemyPool.Release(visualEnemy);
    }

    public void RequestServerSync()
    {
        if (NetworkClient.active)
        {
            NetworkClient.Send(new EnemyServerSpawner.RequestEnemySyncMessage());
        }
    }

    
}

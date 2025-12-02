using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyServerSpawner : NetworkBehaviour
{

    private readonly Dictionary<int, Enemy> activeEnemies = new();

    [SerializeField]
    private GameObject enemyServerPoolPrefab;

    [SerializeField]
    private Vector3 initalWayPoint;
    public Vector3 InitialWayPoint { get { return initalWayPoint; } set { initalWayPoint = value; } }

    [SerializeField]
    private float spawnRadius = 5f;

    private int activeEnemyCount = 0;

    private ServerEnemyPool serverEnemyPool;

    public EnemyType GetEnemyType()
    {
        return enemyServerPoolPrefab.GetComponent<ServerEnemyPool>().GetSpawnerType();
    }

    private Vector3 ConvertInitialWayPointToLocalPosition()
    {
        Debug.Log("initialPoint: " + transform.position + " " + initalWayPoint);
        return transform.position + initalWayPoint;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkServer.RegisterHandler<RequestEnemySyncMessage>(OnRequestEnemySync);
    }

    public void initialSpawn(int spawnerId)
    {
        //NetworkServer.RegisterHandler<RequestEnemySyncMessage>(OnRequestEnemySync);

        // Instantiate the pool prefab under this object
        GameObject poolInstance = Instantiate(enemyServerPoolPrefab, transform);
        serverEnemyPool = poolInstance.GetComponent<ServerEnemyPool>();
        StartCoroutine(SlightDelayForSpawn(spawnerId));
    }

    public void SpawnEnemyOnServer(int spawnerId, int enemyCount)
    {
        GameObject poolInstance = Instantiate(enemyServerPoolPrefab, transform);
        serverEnemyPool = poolInstance.GetComponent<ServerEnemyPool>();
        StartCoroutine(SlightDelayForSpawn(spawnerId, enemyCount));
    }

    private IEnumerator SlightDelayForSpawn(int spawnerId = -1, int enemyCount = 1)
    {
        yield return null;
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randomSpawnPos = GetRandomPointInCircle();
            Vector2 randomDirection = Vector3.up;
            SpawnEnemyOnServer(spawnerId, randomSpawnPos, randomDirection);

            // Optional: small delay between spawns so they don't all appear at once
            //yield return new WaitForSeconds(0.1f);
        }
        
    }

    public void RegisterEnemy(int netId, Enemy vb)
    {
        if (!activeEnemies.ContainsKey(netId))
            activeEnemies.Add(netId, vb);
    }

    public Enemy GetEnemy()
    {
        Enemy enemy = serverEnemyPool.Get();
        RegisterEnemy(enemy.gameObject.GetInstanceID(), enemy);
        return enemy;
    }

    public void ReleaseEnemy(Enemy enemy)
    {
        int instanceId = enemy.gameObject.GetInstanceID();
        DestroyEnemy(instanceId);
        RpcDestroyVisualEnemy(instanceId);
    }


    public Enemy GetEnemyById(int netId)
    {
        activeEnemies.TryGetValue(netId, out var vb);
        return vb;
    }

    public Enemy GetPooledEnemy()
    {
        return serverEnemyPool.Get();
    }

    private Vector3 GetRandomPointInCircle()
    {
        // Pick a random point inside a unit circle (returns x,y)
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;

        // Convert to world space (2D on XY plane)
        Vector3 spawnPos = new Vector3(transform.position.x + randomPoint.x,
                                    transform.position.y + randomPoint.y,
                                    transform.position.z);

        return spawnPos;
    }

    void DestroyEnemy(int enemyId)
    {
        if(activeEnemyCount == 0)
        {
            return;
        }
        RpcDestroyVisualEnemy(enemyId);
        activeEnemyCount--;
        if (activeEnemies.TryGetValue(enemyId, out var vb))
        {
            //Destroy(vb.gameObject);
            serverEnemyPool.Release(vb);
            activeEnemies.Remove(enemyId);
        }
        if(activeEnemyCount <= 0)
        {
            //StartCoroutine(SlightDelayForSpawn(gameObject.GetInstanceID()));
        }
    }

    [ClientRpc]
    void RpcDestroyVisualEnemy(int enemyId)
    {
        VisualEnemyManager.Instance.DestroyVisualEnemy(enemyId);
    }

    [ClientRpc]
    public void RpcUpdateEnemyVisual(int id, Vector3 dir)
    {
        var vb = VisualEnemyManager.Instance.GetEnemyById(id);
        if (vb != null)
            vb.SetTargetDirection(dir);
    }


 

    [ClientRpc]
    public void RpcSpawnVisual(EnemyType enemyType, Vector3 position, Vector2 dir, int bulletId)
    {
        if(VisualEnemyManager.Instance.GetEnemyById(bulletId) != null)
        {
            VisualEnemyManager.Instance.GetEnemyById(bulletId).transform.position = position;
            VisualEnemyManager.Instance.GetEnemyById(bulletId).transform.up = dir;
            VisualEnemyManager.Instance.GetEnemyById(bulletId).gameObject.SetActive(true);
        }

         Debug.Log("RPC visual spawn. Manager null? " + (VisualEnemyManager.Instance == null));
        VisualEnemy vb = VisualEnemyManager.Instance.GetPooledEnemy(enemyType);

        // Position and orient it
        vb.transform.position = position;
        vb.transform.up = dir;

        // Reactivate and launch it
        vb.gameObject.SetActive(true);

        // Register it for later destruction
        VisualEnemyManager.Instance.RegisterEnemy(bulletId, vb);
    }

    [Server]
    public void SpawnEnemyOnServer(int spawnerId, Vector3 position, Vector2 direction)
    {
        Debug.Log("SpawnEnemyOnServer time");
        activeEnemyCount++;
        Enemy enemy = GetEnemy(); // from pool


        enemy.transform.position = position;
        enemy.transform.up = direction;
        enemy.gameObject.SetActive(true);
        enemy.SpawnerId = spawnerId;
        int enemyId = enemy.gameObject.GetInstanceID();


        
        //enemy.Initialize(ConvertInitialWayPointToLocalPosition());
        enemy.Initialize(initalWayPoint);
        enemy.ResetState();
        RpcSpawnVisual(enemy.GetEnemyType, position, direction, enemyId);
        

    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        int segments = 36; // more segments = smoother circle
        float angleStep = 360f / segments;
        Vector3 center = transform.position;

        Vector3 lastPoint = center + new Vector3(spawnRadius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * spawnRadius, Mathf.Sin(angle) * spawnRadius, 0f);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(ConvertInitialWayPointToLocalPosition(), new Vector3(.5f, .5f, .5f));
    }
    

    private void OnRequestEnemySync(NetworkConnectionToClient conn, RequestEnemySyncMessage msg)
    {
        foreach (var kvp in activeEnemies)
        {
            var enemy = kvp.Value;
            
            TargetSpawnVisual(conn, enemy.GetEnemyType, enemy.transform.position, enemy.transform.up, enemy.gameObject.GetInstanceID());
        }
    }

    [TargetRpc]
    void TargetSpawnVisual(NetworkConnection target, EnemyType enemyType, Vector3 position, Vector2 dir, int id)
    {
        VisualEnemy vb = VisualEnemyManager.Instance.GetPooledEnemy(enemyType);
        vb.transform.position = position;
        vb.transform.up = dir;
        vb.gameObject.SetActive(true);
        VisualEnemyManager.Instance.RegisterEnemy(id, vb);
    }

    [ClientRpc]
    public void RpcStartEnemyRotation(int enemyId, Vector2 dir)
    {

        var visual = VisualEnemyManager.Instance.GetEnemyById(enemyId);
        if (visual != null)
            visual.RotateTo(dir);
    }

    [ClientRpc]
    public void RpcFinishEnemyRotation(int enemyId, Vector2 dir)
    {
        var visual = VisualEnemyManager.Instance.GetEnemyById(enemyId);
        if (visual != null)
            visual.FinishRotation(dir);
    }

    [ClientRpc]
    public void RpcStartEnemyMove(int enemyId, float distance)
    {
        var visual = VisualEnemyManager.Instance.GetEnemyById(enemyId);
        if (visual != null)
            visual.MoveForward(distance);
    }

    [ClientRpc]
    public void RpcFinishEnemyMove(int enemyId, Vector3 finalPos)
    {
        var visual = VisualEnemyManager.Instance.GetEnemyById(enemyId);
        if (visual != null)
            visual.FinishMovement(finalPos);
    }

    [ClientRpc]
    public void RpcFinishEnemyAim(int enemyId, Vector3 finalDir)
    {
        var visual = VisualEnemyManager.Instance.GetEnemyById(enemyId);
        if (visual != null)
            visual.FinishAim(finalDir);
    }

    [ClientRpc]
    public void RpcStartEnemyAim(int enemyId, Vector3 aimDirection)
    {
        var visual = VisualEnemyManager.Instance.GetEnemyById(enemyId);
        if (visual != null)
            visual.StartAim(aimDirection);
    }

    public void SyncVisualsToClient(NetworkConnection conn)
    {
        foreach (var kvp in activeEnemies)
        {
            Enemy enemy = kvp.Value;
            if (enemy == null)
                continue;
            TargetSpawnVisual( conn, enemy.GetEnemyType, enemy.transform.position, enemy.transform.up, enemy.gameObject.GetInstanceID());
            
        }
    }


    public struct RequestEnemySyncMessage : NetworkMessage { }
}

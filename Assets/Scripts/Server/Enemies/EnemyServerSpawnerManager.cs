using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mirror;
using Mirror.Examples.Billiards;
using UnityEngine;

public class EnemyServerSpawnerManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> spawnerPrefabs;

    public static EnemyServerSpawnerManager Instance { get; private set; }

    private Dictionary<int, EnemyServerSpawner> enemySpawners = new();

    [SerializeField] private Vector3 northSpawnPos;
    [SerializeField] private Vector3 eastSpawnPos;
    [SerializeField] private Vector3 southSpawnPos;
    [SerializeField] private Vector3 westSpawnPos;

    [SerializeField] private Vector3 northWayPointOffset;
    [SerializeField] private Vector3 eastWayPointOffset;
    [SerializeField] private Vector3 southWayPointOffset;
    [SerializeField] private Vector3 westWayPointOffset;

    private Dictionary<EnemyType, List<EnemyServerSpawner>> spawnerRegistry = new();

    private void registerSpawner(EnemyType enemyType, EnemyServerSpawner enemyServerSpawner)
    {
        if (spawnerRegistry.TryGetValue(enemyType, out var list))
        {
            list.Add(enemyServerSpawner);
        }
        else
        {
            spawnerRegistry[enemyType] = new List<EnemyServerSpawner> { enemyServerSpawner };
        }
    }

    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IEnumerable<EnemyServerSpawner> GetAllSpawners()
    {
        return enemySpawners.Values;
    }

    private int GetNextAvailableSpawner(int spawnersUsed, int currentSpawner)
    {
        int index = currentSpawner % 4;

        for (int i = 0; i < 4; i++)
        {
            int bit = 1 << index;

            if ((spawnersUsed & bit) == 0)
            {
                return index;
            }

            index = (index + 1) % 4;
        }
        return -1;
    }

    public void SpawnWave(WaveSlot slot)
    {
        int spawnerCount = slot.spawnersToUse;
        int spawnerOne = 1;
        int spawnerTwo = 2;
        int spawnerThree = 4;
        int spawnerFour = 8;
        int usedSpawners = 0;
        int enemiesPerSpawner =  slot.enemyCount  / spawnerCount;
        int remainder = slot.enemyCount % spawnerCount;
        for(int i = 0, n = spawnerCount; i < n; i++)
        {
            int value = Random.Range(0, 4);
            int spawnerIndex = GetNextAvailableSpawner(usedSpawners, value);

            int bit = 1 << spawnerIndex;
            usedSpawners |= bit;
            if(spawnerRegistry.TryGetValue(slot.enemyType, out var listOfSpawners)){
                EnemyServerSpawner spawnerToUse = listOfSpawners[spawnerIndex];
                spawnerToUse.SpawnEnemyOnServer(spawnerToUse.gameObject.GetInstanceID(), enemiesPerSpawner + remainder);
                GameEvents.OnOpenGate?.Invoke((Direction)spawnerIndex);
                GameEvents.OnWaveSpawn?.Invoke((Direction)spawnerIndex, enemiesPerSpawner + remainder);
                remainder = 0;
            }
            

        }
        Debug.Log("The slot is of type: " + slot.enemyType + " of count: " + slot.enemyCount + " for spawner count: " + slot.spawnersToUse );

    }

    public void SpawnAllSpawners()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        Vector3 managerPos = transform.position;
        for (int i = 0; i < spawnerPrefabs.Count; i++)
        {
            GameObject obj = Instantiate(spawnerPrefabs[i]);
            EnemyServerSpawner spawner = obj.GetComponent<EnemyServerSpawner>();
            Vector3 spawnOffset = managerPos + (i % 4) switch
            {
                0 => northSpawnPos,
                1 => eastSpawnPos,
                2 => southSpawnPos,
                3 => westSpawnPos,
                _ => Vector3.zero
            };
            obj.transform.position = spawnOffset;

            Vector3 initialWayPoint = (i % 4) switch
            {
                0 => northWayPointOffset,
                1 => eastWayPointOffset,
                2 => southWayPointOffset,
                3 => westWayPointOffset,
                _ => Vector3.zero
            };
            
            if (spawner != null)
            {
                enemySpawners.Add(obj.GetInstanceID(), spawner);
                
                spawner.InitialWayPoint = initialWayPoint + spawnOffset;
                NetworkServer.Spawn(obj);
                registerSpawner(spawner.GetEnemyType(), spawner);
                //spawner.initialSpawn(obj.GetInstanceID());
                
            }
            
            //NetworkServer.Spawn(obj);

            // Spawn the networked object for clients
            
        }
        StartCoroutine(DelayRoomLoad());
    }

    IEnumerator DelayRoomLoad()
    {
        yield return new WaitForSeconds(1f);
        GameEvents.OnActivateRoom?.Invoke(1);
        GameEvents.OnRoomLoad?.Invoke();
        
    }

    public void ReleaseEnemy(Enemy enemy)
    {

        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;
        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.ReleaseEnemy(enemy); // or whatever your release logic is
        }
        GameEvents.OnEnemyEliminated?.Invoke();
    }

    public void StartRotation(Enemy enemy, Vector3 initialWayPoint)
    {
        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;

        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.RpcStartEnemyRotation(enemy.gameObject.GetInstanceID(), initialWayPoint); // or whatever your release logic is
        }
    }

    public void FinishRotation(Enemy enemy, Vector3 initialWayPoint)
    {
        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;

        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.RpcFinishEnemyRotation(enemy.gameObject.GetInstanceID(), initialWayPoint); // or whatever your release logic is
        }
    }

    public void StartEnemyMove(Enemy enemy, float distance)
    {
        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;

        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.RpcStartEnemyMove(enemy.gameObject.GetInstanceID(), distance); // or whatever your release logic is
        }
    }

    public void FinishEnemyMove(Enemy enemy, Vector3 finalPoint)
    {
        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;

        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.RpcFinishEnemyMove(enemy.gameObject.GetInstanceID(), finalPoint); // or whatever your release logic is
        }
    }

    public void StartEnemyAim(Enemy enemy, Vector3 aimDirection)
    {
        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;

        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.RpcStartEnemyAim(enemy.gameObject.GetInstanceID(), aimDirection); // or whatever your release logic is
        }
    }

    public void FinishEnemyAim(Enemy enemy, Vector3 aimDirection)
    {
        if (enemy == null)
            return;

        // Get the spawner ID from the enemy
        int spawnerId = enemy.SpawnerId;

        // Try to find the matching spawner in the dictionary
        if (enemySpawners.TryGetValue(spawnerId, out EnemyServerSpawner spawner))
        {
            spawner.RpcFinishEnemyAim(enemy.gameObject.GetInstanceID(), aimDirection); // or whatever your release logic is
        }
    }

    /*public void SyncAllVisuals()
    {
        // Go through every connected client
        Debug.Log("Attempting to syncAllVisuals()");
            // Go through each spawner
            foreach (EnemyServerSpawner spawner in enemySpawners.Values)
            {
                spawner.SyncVisualsToClient();
            }
        
    }*/

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Make sure we can see them clearly in the editor
        Gizmos.color = Color.green;

        Vector3 managerPos = transform.position;

        // Draw spheres at the four spawn positions
        Vector3 northPos = managerPos + northSpawnPos;
        Vector3 eastPos  = managerPos + eastSpawnPos;
        Vector3 southPos = managerPos + southSpawnPos;
        Vector3 westPos = managerPos + westSpawnPos;

        Vector3 northPosInitial = northPos + northWayPointOffset;
        Vector3 eastPosInitial = eastPos + eastWayPointOffset;
        Vector3 southPosInitial = southPos + southWayPointOffset;
        Vector3 westPosInitial = westPos + westWayPointOffset;

        float gizmoSize = 0.5f;

        // Draw the spheres
        Gizmos.DrawWireCube(northPos, new Vector3(gizmoSize, gizmoSize, gizmoSize));
        Gizmos.DrawWireCube(eastPos, new Vector3(gizmoSize, gizmoSize, gizmoSize));
        Gizmos.DrawWireCube(southPos, new Vector3(gizmoSize, gizmoSize, gizmoSize));
        Gizmos.DrawWireCube(westPos, new Vector3(gizmoSize, gizmoSize, gizmoSize));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(northPosInitial, new Vector3(gizmoSize / 2, gizmoSize / 2, gizmoSize / 2));
        Gizmos.DrawWireCube(eastPosInitial, new Vector3(gizmoSize / 2, gizmoSize / 2, gizmoSize / 2));
        Gizmos.DrawWireCube(southPosInitial, new Vector3(gizmoSize / 2, gizmoSize / 2, gizmoSize / 2));
        Gizmos.DrawWireCube(westPosInitial, new Vector3(gizmoSize / 2, gizmoSize / 2, gizmoSize / 2));

        // Optionally, draw lines connecting to the manager
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(managerPos, northPos);
        Gizmos.DrawLine(managerPos, eastPos);
        Gizmos.DrawLine(managerPos, southPos);
        Gizmos.DrawLine(managerPos, westPos);


        Gizmos.color = Color.red;
        for (int i = 0; i < spawnerPrefabs.Count; i++)
        {
            Vector3 spawnOffset = managerPos + (i % 4) switch
            {
                0 => northSpawnPos,
                1 => eastSpawnPos,
                2 => southSpawnPos,
                3 => westSpawnPos,
                _ => Vector3.zero
            };

            Vector3 initialWayPoint = spawnOffset +  (i % 4) switch
            {
                0 => northWayPointOffset,
                1 => eastWayPointOffset,
                2 => southWayPointOffset,
                3 => westWayPointOffset,
                _ => Vector3.zero
            };
            Gizmos.DrawWireCube(initialWayPoint, new Vector3(gizmoSize / 2, gizmoSize / 2, gizmoSize / 2));


            // Spawn the networked object for clients
            
        }

        

        // Draw labels for clarity in editor
    #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(northPos, "North");
        UnityEditor.Handles.Label(eastPos, "East");
        UnityEditor.Handles.Label(southPos, "South");
        UnityEditor.Handles.Label(westPos, "West");
    #endif
    }
    #endif

    
}

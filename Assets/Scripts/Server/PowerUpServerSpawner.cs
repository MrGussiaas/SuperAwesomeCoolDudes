using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.BouncyCastle.OpenSsl;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUpServerSpawner : NetworkBehaviour
{

    [SerializeField]
    private  List<GameObject> spawnablePowerUps = new();

    [SerializeField]
    private int spawnDelay = 5;

    private Coroutine spawnerLoop;

    public static PowerUpServerSpawner Instance { get; private set; }

    private Dictionary<AbilitiesEnum, Queue<PowerUp>> powerUpRegistry = new();

    [SerializeField]
    private float spawnHeight = 5f;
    [SerializeField]
    private float spawnWidth = 5f;

    private void InitVars()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        foreach(GameObject gameObject in spawnablePowerUps)
        {
            GameObject powerUpPrefabInstance = Instantiate(gameObject);
            PowerUp powerUp = powerUpPrefabInstance.GetComponent<PowerUp>();
            
           if( powerUpRegistry.TryGetValue(powerUp.PowerUpType, out var pu)){
                
                powerUp.gameObject.SetActive(false);
                pu.Enqueue(powerUp);
            }
            else
            {
                Queue<PowerUp> powerUpQueue = new Queue<PowerUp>();
                powerUp.gameObject.SetActive(false);
                powerUpQueue.Enqueue(powerUp);
                powerUpRegistry.Add(powerUp.PowerUpType, powerUpQueue);
            }
            
        }
    }

    public void UpdateLocation(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameEvents.OnActivateRoom += StartSpawnerTimer;
        GameEvents.PowerUpRemovedFromRoom += DeSpawnPowerUp;
        InitVars();
    }


    private Vector3 GetRandomPointInRectangle()
    {
        // Pick a random x/y inside rectangle centered at this transform
        float halfWidth = spawnWidth / 2f;
        float halfHeight = spawnHeight / 2f;

        float randomX = Random.Range(-halfWidth, halfWidth);
        float randomY = Random.Range(-halfHeight, halfHeight);

        // Convert to world space
        Vector3 spawnPos = new Vector3(
            transform.position.x + randomX,
            transform.position.y + randomY,
            transform.position.z
        );

        return spawnPos;
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnWidth, spawnHeight, 0f));
        Gizmos.color = Color.red;
    }

    private void SpawnPowerUp(PowerUp powerUp)
    {
        Vector3 spawnPosition = GetRandomPointInRectangle();
        powerUp.transform.position = spawnPosition;
        powerUp.gameObject.SetActive(true);
        powerUp.InitializePowerUP();
        NetworkServer.Spawn(powerUp.gameObject);
    }

    private void DeSpawnPowerUp(PowerUp powerUp)
    {
        if(powerUpRegistry.TryGetValue(powerUp.PowerUpType, out var q))
            {
                if(q == null)
                {
                    return;
                }
                powerUp.CancelPowerUp();
                powerUp.gameObject.SetActive(false);
                q.Enqueue(powerUp);
                NetworkServer.UnSpawn(powerUp.gameObject);
            }
    }

    private IEnumerator PowerUpSpawnLoop()
    {
        Debug.Log("Power up spawner loop begun");
        while(true){
            yield return new WaitForSeconds(spawnDelay);
            int powerUpCounts = powerUpRegistry.Count;
            int keySlot = Random.Range(0, powerUpCounts);
            if(powerUpRegistry.TryGetValue((AbilitiesEnum)keySlot, out var q))
            {
                
                if(q == null || q.Count <= 0)
                {
                    continue;
                }
                
                PowerUp powerUp = q.Dequeue();
                Debug.Log("Spawning power up of type: " + powerUp.PowerUpType);
                SpawnPowerUp(powerUp);
            }
        }
    }

    private void StartSpawnerTimer(int roomId, Direction direction)
    {
        if (!isServer)
        {
            return;
        }
        if(spawnerLoop != null)
        {
            StopCoroutine(spawnerLoop);
            spawnerLoop = null;
        } 
               
        spawnerLoop = StartCoroutine(PowerUpSpawnLoop());
    }



    public override void OnStopServer()
    {
        GameEvents.OnActivateRoom -= StartSpawnerTimer;
        GameEvents.PowerUpRemovedFromRoom -= DeSpawnPowerUp;
    }
    
}

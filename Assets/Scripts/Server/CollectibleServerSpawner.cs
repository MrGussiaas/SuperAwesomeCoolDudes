using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class CollectibleServerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject dogcoinPoolPrefab;
    [SerializeField] private GameObject catcoinPoolPrefab;
    [SerializeField] private GameObject frogcoinPoolPrefab;
    [SerializeField] private GameObject goatCheeseNftPoolPrefab;

    public static CollectibleServerSpawner Instance { get; private set; }

    [SerializeField]
    private float spawnHeight = 5f;
    [SerializeField]
    private float spawnWidth = 5f;
    [SerializeField]
    private int spawnDelay = 5;
    [SerializeField]
    private Vector3 positionOffSet;

    private CollectiblePool dogCoinPool;

    private CollectiblePool catCoinPool;
    private CollectiblePool frogCoinPool;
    private CollectiblePool goatCheeseNftPool;


    private Coroutine spawnerLoop;
    // Start is called before the first frame update

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameEvents.OnActivateRoom += StartCollectibleTimer;
        GameEvents.CollectibleRemovedFromRoom += DeSpawnCollectible;
        InitVars();
    }

    public void UpdateLocation(Vector3 newLocation)
    {
        transform.position = newLocation + positionOffSet;
    }

    public override void OnStopServer()
    {
        GameEvents.OnActivateRoom -= StartCollectibleTimer;
        GameEvents.CollectibleRemovedFromRoom -= DeSpawnCollectible;
    }

    private void StartCollectibleTimer(int roomId, Direction direction)
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
               
        spawnerLoop = StartCoroutine(CollectibleSpawnLoop());
    }

    private void DeSpawnCollectible(Collectible collectible)
    {
        NetworkServer.UnSpawn(collectible.gameObject);
        int collectibleType = (int)collectible.CollectibleType;
        CollectiblePool pool = GetPoolFromInt(collectibleType);
        pool.Release(collectible);
        collectible.gameObject.SetActive(false);
        
    }

    private void SpawnCollectible(Collectible collectible)
    {
        Vector3 spawnPosition = GetRandomPointInRectangle();
        collectible.transform.position = spawnPosition;
        collectible.gameObject.SetActive(true);
        collectible.InitializeCollectible();
        Debug.Log("spawning collectible of type: " + collectible.CollectibleType);
        NetworkServer.Spawn(collectible.gameObject);
    }

    private CollectiblePool GetPoolFromInt(int poolIndex)
    {
        switch (poolIndex) {
            case 0 : return  dogCoinPool;
            case 1 : return catCoinPool;
            case 2 : return frogCoinPool;
            default: return goatCheeseNftPool;
        }
    }

    private Vector3 GetRandomOffsetInRadius(float radius)
    {
        Vector2 offset2D = Random.insideUnitCircle * radius;
        return new Vector3(offset2D.x, offset2D.y, 0f);
    }

    private IEnumerator CollectibleSpawnLoop()
    {
        while(true){
            yield return new WaitForSeconds(spawnDelay);

            Vector3 centerPoint = GetRandomPointInRectangle();

            int spawnCount = Random.Range(
                5,
                10 + 1
            );

            int collectibleCounts = (int)CollectibleEnum.CHEESE_NFT + 1;
            int keySlot = Random.Range(0, collectibleCounts);
            CollectiblePool poolToUse = GetPoolFromInt(keySlot);

            for (int i = 0; i < spawnCount; i++)
            {
                Collectible collectible = poolToUse.Get();

                Vector3 offset = GetRandomOffsetInRadius(.5f);
                collectible.transform.position = centerPoint + offset;

                collectible.gameObject.SetActive(true);

                NetworkServer.Spawn(collectible.gameObject);
                collectible.InitializeCollectible();
            }
        }
    }

    private void InitVars()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameObject dogPoolInstance = Instantiate(dogcoinPoolPrefab, transform);
        GameObject catPoolInstance = Instantiate(catcoinPoolPrefab, transform);
        GameObject frogPoolInstance = Instantiate(frogcoinPoolPrefab, transform);
        GameObject goatCheeseNftInstance = Instantiate(goatCheeseNftPoolPrefab, transform);
        dogCoinPool = dogPoolInstance.GetComponent<CollectiblePool>();
        catCoinPool = catPoolInstance.GetComponent<CollectiblePool>();
        frogCoinPool = frogPoolInstance.GetComponent<CollectiblePool>();
        goatCheeseNftPool = goatCheeseNftInstance.GetComponent<CollectiblePool>();
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
            transform.position.x + positionOffSet.x + randomX,
            transform.position.y+positionOffSet.y + randomY,
            transform.position.z
        );

        return spawnPos;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + positionOffSet, new Vector3(spawnWidth, spawnHeight, 0f));
    }
}

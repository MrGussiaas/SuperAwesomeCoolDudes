using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ServerEnemyPool : MonoBehaviour
{
    [SerializeField] private GameObject enemySlowPrefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<Enemy> _pool;

    public EnemyType GetSpawnerType()
    {
        return enemySlowPrefab.GetComponent<Enemy>().GetEnemyType;
    }

    private void Awake()
    {

        if (enemySlowPrefab == null)
        {

            return;
        }

        _pool = new UnityEngine.Pool.ObjectPool<Enemy>(
            createFunc: () =>
            {
                GameObject serverEnemy = Instantiate(enemySlowPrefab);
                serverEnemy.SetActive(false);
                return serverEnemy.GetComponent<Enemy>();
            },
            actionOnGet: serverEnemy =>
            {
                serverEnemy.gameObject.SetActive(true);
            },
            actionOnRelease: serverEnemy =>
            {
                serverEnemy.gameObject.SetActive(false);
            },
            actionOnDestroy: serverEnemy =>
            {
                Destroy(serverEnemy.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public Enemy Get()
    {
        return _pool.Get();
    }

    public void Release(Enemy serverEnemy)
    {
        _pool.Release(serverEnemy);
    }
}

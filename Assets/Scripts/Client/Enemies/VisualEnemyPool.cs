using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VisualEnemyPool : MonoBehaviour
{
    [SerializeField] private GameObject enemySlowPrefab;

    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<VisualEnemy> _pool;

    private void Awake()
    {
        _pool = new UnityEngine.Pool.ObjectPool<VisualEnemy>(
            createFunc: () =>
            {
                GameObject visual = Instantiate(enemySlowPrefab);
                visual.SetActive(false);
                return visual.GetComponent<VisualEnemy>();
            },
            actionOnGet: visualEnemy =>
            {
                visualEnemy.gameObject.SetActive(true);
            },
            actionOnRelease: visualEnemy =>
            {
                Debug.Log("deactivating visual enemy");
                visualEnemy.gameObject.SetActive(false);
            },
            actionOnDestroy: visualEnemy =>
            {
                Destroy(visualEnemy.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public VisualEnemy Get()
    {
        return _pool.Get();
    }

    public void Release(VisualEnemy visualEnemy)
    {
        _pool.Release(visualEnemy);
    }
}

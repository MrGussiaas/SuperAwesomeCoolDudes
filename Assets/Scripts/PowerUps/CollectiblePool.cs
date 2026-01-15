using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CollectiblePool : MonoBehaviour
{

    [SerializeField] private GameObject collectiblePrefab;

    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<Collectible> _pool;
    // Start is called before the first frame update
    private void Awake()
    {
        _pool = new UnityEngine.Pool.ObjectPool<Collectible>(
            createFunc: () =>
            {
                GameObject collectible = Instantiate(collectiblePrefab);
                collectible.SetActive(false);
                return collectible.GetComponent<Collectible>();
            },
            actionOnGet: collectible =>
            {
                collectible.gameObject.SetActive(true);
            },
            actionOnRelease: collectible =>
            {
                collectible.gameObject.SetActive(false);
            },
            actionOnDestroy: collectible =>
            {
                Destroy(collectible.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public Collectible Get()
    {
        return _pool.Get();
    }

    public void Release(Collectible collectible)
    {
        _pool.Release(collectible);
    }
}

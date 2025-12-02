using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletServerPool : MonoBehaviour
{


    [SerializeField] private GameObject bulletServerPrefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<Bullet> _pool;

    private void Awake()
    {
        _pool = new UnityEngine.Pool.ObjectPool<Bullet>(
            createFunc: () =>
            {
                GameObject bullet = Instantiate(bulletServerPrefab);
                bullet.SetActive(false);
                return bullet.GetComponent<Bullet>();
            },
            actionOnGet: bullet =>
            {
                bullet.gameObject.SetActive(true);
            },
            actionOnRelease: bullet =>
            {
                bullet.gameObject.SetActive(false);
            },
            actionOnDestroy: bullet =>
            {
                Destroy(bullet.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public Bullet Get()
    {
        return _pool.Get();
    }



    public void Release(Bullet bullet)
    {
        _pool.Release(bullet);
    }
}
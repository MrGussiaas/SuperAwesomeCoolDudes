using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletVisualPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletVisualPrefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<VisualBullet> _pool;

    private void Awake()
    {
        _pool = new UnityEngine.Pool.ObjectPool<VisualBullet>(
            createFunc: () =>
            {
                GameObject visual = Instantiate(bulletVisualPrefab);
                visual.SetActive(false);
                return visual.GetComponent<VisualBullet>();
            },
            actionOnGet: visualBullet =>
            {
                visualBullet.gameObject.SetActive(true);
            },
            actionOnRelease: visualBullet =>
            {
                visualBullet.gameObject.SetActive(false);
            },
            actionOnDestroy: visualBullet =>
            {
                Destroy(visualBullet.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public VisualBullet Get()
    {
        return _pool.Get();
    }

    public void Release(VisualBullet visualBullet)
    {
        _pool.Release(visualBullet);
    }
}
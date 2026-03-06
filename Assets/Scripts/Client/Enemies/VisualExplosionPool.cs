using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VisualExplosionPool : MonoBehaviour
{

    [SerializeField] private GameObject visualExplosionPrefab;

    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<IVisualExplosion> _pool;

    private void Awake()
    {
        _pool = new UnityEngine.Pool.ObjectPool<IVisualExplosion>(
            createFunc: () =>
            {
                GameObject visual = Instantiate(visualExplosionPrefab);
                visual.SetActive(false);
                return visual.GetComponent<IVisualExplosion>();
            },
            actionOnGet: visualExplosion =>
            {
                var mb = (MonoBehaviour)visualExplosion;
                mb.gameObject.SetActive(true);
            },
            actionOnRelease: visualExplosion =>
            {
                var mb = (MonoBehaviour)visualExplosion;
                mb.gameObject.SetActive(false);
            },
            actionOnDestroy: visualExplosion =>
            {
                var mb = (MonoBehaviour)visualExplosion;
                Destroy(mb.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public IVisualExplosion Get()
    {
        return _pool.Get();
    }

    public void Release(IVisualExplosion visualEnemy)
    {
        _pool.Release(visualEnemy);
    }
}

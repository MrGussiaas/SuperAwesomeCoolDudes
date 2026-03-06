using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualExplosionManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static VisualExplosionManager Instance { get; private set; }

    [SerializeField]
    private GameObject visualExplosionPoolPrefab;

    [SerializeField]
    private GameObject playerBloodSpurtPoolPrefab;


    private VisualExplosionPool explosionVisualPool;

    private VisualExplosionPool explosionVisualBloodPool;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameObject poolInstance = Instantiate(visualExplosionPoolPrefab, transform);
        GameObject poolPlayerBloodInstance = Instantiate(playerBloodSpurtPoolPrefab, transform);

        explosionVisualPool = poolInstance.GetComponent<VisualExplosionPool>();
        explosionVisualBloodPool = poolPlayerBloodInstance.GetComponent<VisualExplosionPool>();

        DontDestroyOnLoad(gameObject);
    }

    public IVisualExplosion GetPooledExplosion()
    {
        return explosionVisualPool.Get();
    }

    public void ReleasePooledExplosion(IVisualExplosion explosionToRelease)
    {
        explosionVisualPool.Release(explosionToRelease);
    }

    public IVisualExplosion GetPooledPlayerBlood()
    {
        return explosionVisualBloodPool.Get();

    }

    public void ReleasePooledPlayerBlood(IVisualExplosion bloodToRelease)
    {
        explosionVisualBloodPool.Release(bloodToRelease);
    }

}

using System.Collections;
using System.Collections.Generic;
using Mirror.BouncyCastle.Tls;
using UnityEngine;

public class BloodSpurt : MonoBehaviour, IVisualExplosion
{

    private  ParticleSystem ps;

    public void EndExplosion()
    {
        
    }

    public void ExplosionMidpoint()
    {
        
    }

    public void StartExplosion(Vector3 startPoint)
    {
        ps.Play();
    }

    // Start is called before the first frame update
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

}

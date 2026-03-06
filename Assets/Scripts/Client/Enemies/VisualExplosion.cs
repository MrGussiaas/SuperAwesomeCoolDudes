using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualExplosion : MonoBehaviour,  IVisualExplosion
{

    private const string MAIN_EXPLOSION ="MainExplosion"; 

    private Animator anim;

    private ParticleSystem ps;

    private SpriteRenderer sr;

    public void EndExplosion()
    {
        sr.enabled = false;
        StartCoroutine(DelayRelease());
    }

    public void ExplosionMidpoint()
    {
        ps.Play();
    }



    public void StartExplosion(Vector3 startPoint)
    {
        sr.enabled = true;  
        transform.position = startPoint;
        transform.gameObject.SetActive(true);
        anim.Play(MAIN_EXPLOSION);

    }

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        ps = GetComponent<ParticleSystem>();
        sr = GetComponent<SpriteRenderer>();
    }

    private IEnumerator DelayRelease()
    {
        yield return new WaitForSeconds(.5f);
        VisualExplosionManager.Instance.ReleasePooledExplosion(this);
        gameObject.SetActive(false);

    }


}

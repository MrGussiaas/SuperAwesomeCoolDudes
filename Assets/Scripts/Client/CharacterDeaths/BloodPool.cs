using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPool : MonoBehaviour, IVisualExplosion
{


    private List<IVisualExplosion> childSpurts = new();
    private SpriteRenderer sr;

    private Animator anim;

    private const string MAIN_EXPLOSION = "MainExplosion";

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<IVisualExplosion>(out var explosion))
            {
                childSpurts.Add(explosion);
            }
        }
    }

    public void EndExplosion()
    {
        anim.enabled = false;
        foreach(IVisualExplosion spurt in childSpurts)
        {
            spurt.EndExplosion();
        }
        StartCoroutine(FadeOutBlood());
        
    }

    public void ExplosionMidpoint()
    {
        
        foreach(IVisualExplosion spurt in childSpurts)
        {
            spurt.ExplosionMidpoint();
        }
        
    }

    public void StartExplosion(Vector3 startPoint)
    {
        sr.enabled=true;
        transform.position = startPoint;
        anim.enabled = true;
        anim.Play(MAIN_EXPLOSION);

        foreach(IVisualExplosion spurt in childSpurts)
        {
            spurt.StartExplosion(startPoint);
        }
    }

    private IEnumerator FadeOutBlood()
    {
        Vector3 finalScale = new Vector3(1.5f, 1, 1);
        Vector3 startScale = transform.localScale;
        Color startColor = sr.color;
        float startAlpha = 1;
        float endAlpha = 0;

        float duration = 5f;
        float elapsed = 0;
        while(elapsed <= duration)
        {
            yield return null;
            float tween = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, finalScale, tween);
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, tween);
            Color fadedColor = new Color(startColor.r, startColor.g, startColor.b, newAlpha );
            elapsed += Time.deltaTime;
            sr.color = fadedColor;
        }
        VisualExplosionManager.Instance.ReleasePooledPlayerBlood(this);
    }


}

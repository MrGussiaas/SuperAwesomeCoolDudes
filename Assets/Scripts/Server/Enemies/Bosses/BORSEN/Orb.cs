using System;
using UnityEngine;

public class Orb  : MonoBehaviour, IDamagable
{
    private enum OrbStates {DEACTIVATED, ACTIVATED, DESTROYED, PLACED, IN_TRANSIT}
    private OrbStates orbState = OrbStates.DEACTIVATED;
    private SpriteRenderer sr;
    private Vector3 startPosition;
    private const float ANIMATION_FRAMES = 120;
    private const int ANIMATION_FPS = 60;

    private const int BASE_HEALTH = 30;
    private int currentHealth = BASE_HEALTH;

    private static float ANIMATION_TIME = (ANIMATION_FRAMES / ANIMATION_FPS) / Borsen.ANIMATION_SPEED;
    private float lerpSecondsElapsed;

    private BoxCollider2D bc;

    public event Action OnOrbExploded;


    [SerializeField]
    private Vector3 restingPosition;
    // Start is called before the first frame update
    void Awake()
    {
        startPosition = transform.localPosition;
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(orbState == OrbStates.DEACTIVATED || orbState == OrbStates.PLACED || orbState == OrbStates.ACTIVATED)
        {
            return;
        }
        lerpSecondsElapsed += Time.deltaTime;
        Vector3 nextPoint = Vector3.Lerp(startPosition, restingPosition, lerpSecondsElapsed / ANIMATION_TIME);
        transform.localPosition = nextPoint;


    }

    public void GrabOrb()
    {
        orbState = OrbStates.IN_TRANSIT;
    }

    public void ActivateOrb()
    {
        lerpSecondsElapsed = 0;
        orbState = OrbStates.ACTIVATED;
        currentHealth = BASE_HEALTH;
        sr.enabled = true;
        bc.enabled = true;
    }

    public void BlowUpOrb()
    {
        
    }

    public bool IsActive()
    {
        return orbState == OrbStates.ACTIVATED || orbState == OrbStates.IN_TRANSIT;
    }

    public void DeactivateOrb()
    {
        lerpSecondsElapsed = 0;
        orbState = OrbStates.DEACTIVATED;
        transform.localPosition = startPosition;
        sr.enabled = false;
        bc.enabled = false;
    }

    public void SyncMidPoint()
    {
        orbState = OrbStates.PLACED;
        lerpSecondsElapsed = 0;
        transform.localPosition = restingPosition;
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.parent.localToWorldMatrix;
        Gizmos.DrawWireCube(restingPosition, Vector3.one * 0.1f);
    }

    public void TakeDamage()
    {
        currentHealth --;
        if(currentHealth == 0)
        {
            OnOrbExploded?.Invoke();
        }
    }

}

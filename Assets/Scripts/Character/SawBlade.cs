using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class SawBlade : MonoBehaviour
{
    public const float ROTATION_SPEED = 180f;

    private AbilityPickup abilityPickup;

    public const string ENEMY = "Enemy";

    private const int MAX_HEALTH = 2;

    private int health = MAX_HEALTH;

    private int indexId;
    public int IndexId {get {return indexId;} set {indexId = value;}}



    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(transform.parent.position, Vector3.forward, ROTATION_SPEED * Time.deltaTime);
    }

    public void Awake()
    {
        InitVars();
    }

    private void InitVars()
    {
        abilityPickup = GetComponentInParent<AbilityPickup>();
    }

    public void Initialize()
    {
        health = MAX_HEALTH;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(ENEMY))
        {
            return;
        }

        int id = collision.gameObject.GetInstanceID();
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null && enemy.isActiveAndEnabled)
        {
            health --;
            enemy.TakeDamage();
        }
        if(health <= 0)
        {
            transform.gameObject.SetActive(false);
            DeactivateBlade();
        }
    }

    private void DeactivateBlade()
    {
        abilityPickup.DeactivateBlade(indexId);
    }
}

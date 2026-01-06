using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGatePassThroughCounter : MonoBehaviour
{
    private int waveCount;

    public const string ENEMY = "Enemy";

    private GateHandler gateHandler;

    private Direction direction;

    public void ResetCounter()
    {
        waveCount = 0;
    }

    public void AddToWaveCount(int incCount)
    {
        waveCount += incCount;
    }

    private void InitVars()
    {
        gateHandler = GetComponentInParent<GateHandler>();
        direction = gateHandler.GetDirection;
    }

    private void Awake()
    {
        InitVars();
    }

     private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag(ENEMY))
        {
            return;
        }
        if (col.CompareTag(ENEMY))
        {
            waveCount--;
        }
        if(waveCount <= 0)
        {
            GameEvents.OnCloseGate?.Invoke(gateHandler.GetDirection);
            waveCount = 0;
        }
    }

    public void UpdateEnemyCount(Direction dir, int count)
    {
        if(dir == direction)
        {
            waveCount += count;
        }
    }

    public void OnEnable()
    {
        GameEvents.OnWaveSpawn += UpdateEnemyCount;
    }

    public void OnDisable()
    {
        GameEvents.OnWaveSpawn -= UpdateEnemyCount;
    }

}

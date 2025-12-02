using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveOrchestrator : MonoBehaviour
{
    [SerializeField]
    private List<EnemyWave> enemyWaves;

    private int currentWave = 0;

    int currentEnemyWaveCount = 0;

    public void OnEnable()
    {
        GameEvents.OnRoomLoad += DoInitialSpawn;
        GameEvents.OnEnemyEliminated += DoEnemyEliminated;
    }

    public void OnDisable()
    {
        GameEvents.OnRoomLoad -= DoInitialSpawn;
        GameEvents.OnEnemyEliminated -= DoEnemyEliminated;
    }

    private void DoInitialSpawn()
    {
        EnemyWave firstWave = enemyWaves[currentWave];
        currentEnemyWaveCount = firstWave.GetWaveTotal();
        for(int i = 0, n = firstWave.WaveSlots.Count; i < n; i++)
        {
           WaveSlot enemyWaveSlot =  firstWave.WaveSlots[i];
           EnemyServerSpawnerManager.Instance.SpawnWave(enemyWaveSlot);
        }
    }

    private void DoEnemyEliminated()
    {
        currentEnemyWaveCount--;
        Debug.Log("enemyWaveCount:  " + currentEnemyWaveCount);
        if(currentEnemyWaveCount <= 0)
        {
            currentWave ++;
            if(currentWave < enemyWaves.Count)
            {
                DoInitialSpawn();
            }
            else
            {
                Debug.Log("This wave is done.");
            }
        }
    }
}

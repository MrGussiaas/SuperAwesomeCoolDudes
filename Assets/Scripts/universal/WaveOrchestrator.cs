using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveOrchestrator : MonoBehaviour
{
    [SerializeField]
    private List<EnemyWave> enemyWaves;

    private RoomController roomController;

    private int currentWave = 0;

    private const string ROOM_EXIT = "RoomExit";

    int currentEnemyWaveCount = 0;

    public void Awake()
    {
        InitVars();
    }

    private void InitVars()
    {
        roomController = GetComponentInParent<RoomController>();
    }

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
        if(enemyWaves.Count == 0)
        {
            roomController.RoomCleared();
            return;
        }
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
        if(currentEnemyWaveCount <= 0)
        {
            currentWave ++;
            if(currentWave < enemyWaves.Count)
            {
                DoInitialSpawn();
            }
            else
            {
                roomController.RoomCleared();
            }
        }
    }
}

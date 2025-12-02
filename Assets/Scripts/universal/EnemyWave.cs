using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

[Serializable]
public class EnemyWave
{

    [SerializeField]
    private List<WaveSlot> waveSlots;

    public List<WaveSlot> WaveSlots {get  {return waveSlots;}}

    public int GetWaveTotal()
    {
        int sumOf = 0;
        foreach(WaveSlot slot in waveSlots){
            sumOf += slot.enemyCount;
        }
        return sumOf;
    } 

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyCommand
{
    public int enemyId;
    public int spawnerId;
    public EnemyCommandType commandType;

    public Vector3 position;
    public Vector3 direction;

    public float distance;
  
}

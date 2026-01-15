using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents
{
    public static Action OnRoomLoad;

    public static Action OnEnemyEliminated;

    public static Action<Direction, GateOpenReason> OnOpenGate;

    public static Action<Direction, GateOpenReason> OnCloseGate;

    public static Action<Direction, int> OnWaveSpawn;

    public static Action<int, Direction> OnActivateRoom;

    public static Action<PowerUp> PowerUpRemovedFromRoom;

    public static Action<Collectible> CollectibleRemovedFromRoom;

}

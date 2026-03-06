using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyCommandType : byte
{
    MOVE_FORWARD,
    MOVE_FORWARD_NO_DIR,
    FINISH_MOVEMENT,
    FINISH_MOVEMENT_CANCELLED,
    START_ROTATION,
    FINISH_ROTATION,
    DESTROY
}

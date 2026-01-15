using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public void DoWallBump(Vector3 bumpedPosition, Vector2 contactVector);
}

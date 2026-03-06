using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVisualExplosion 
{
    public void StartExplosion(Vector3 startPoint);

    public void ExplosionMidpoint();

    public void EndExplosion();

}

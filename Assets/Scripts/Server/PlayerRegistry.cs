using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerRegistry : NetworkBehaviour
{
    public static PlayerRegistry Instance;

    private readonly List<Transform> players = new();

    void Awake()
    {
        Instance = this;
    }

    [Server]
    public void Register(GameObject player)
    {
        players.Add(player.transform);
    }

    [Server]
    public void Unregister(GameObject player)
    {
        players.Remove(player.transform);
    }

    public Transform GetNearest(Vector3 position)
    {
        Transform nearest = null;
        float best = float.MaxValue;

        foreach (var p in players)
        {
            float d = (p.position - position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                nearest = p;
            }
        }

        return nearest;
    }
}


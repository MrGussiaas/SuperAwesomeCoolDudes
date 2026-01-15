using UnityEngine;

[System.Serializable]
public struct EnemyMovementSnapshot
{
    public int enemyId;       // Unique ID to identify the enemy
    public int spawnerId;
    public Vector3 position;  // Final or target position
    public Vector3 direction; // Optional: direction or rotation
    public float distance;    // Optional: distance moved for visual interpolation
}
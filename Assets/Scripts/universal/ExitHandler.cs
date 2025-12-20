using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;

public class ExitHandler : NetworkBehaviour
{
    [SerializeField]
    private Direction exitDirection;

    [SerializeField]
    private int nextRoomNumber;

    [SerializeField]
    private Vector3 playerEntranceSpawn;
    public Vector3 PlayerEntranceSpawn {get {return playerEntranceSpawn;}}

    [SerializeField]
    private Vector3 playerEntranceWayPoint;
    public Vector3 PlayerEntranceWayPoint {get {return playerEntranceWayPoint;}}

    private const string PLAYER = "Player";

    public Direction ExitDirection {get{return exitDirection;}}
    [ServerCallback]
    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag(PLAYER))
        {
            return;
        }

        Direction directionToLeaveOpen = Direction.NULL;
        switch (exitDirection)
        {
            case  Direction.East :   {
                    directionToLeaveOpen = Direction.West;
                    break;
                }

            case  Direction.West :   {
                    directionToLeaveOpen = Direction.East;
                    break;
                }
            case  Direction.North :   {
                    directionToLeaveOpen = Direction.South;
                    break;
                }
            case  Direction.South :   {
                    directionToLeaveOpen = Direction.North;
                    break;
                }

        }
        Debug.Log("Activating nextRoom: " + nextRoomNumber + " " + directionToLeaveOpen);
        GameEvents.OnActivateRoom(nextRoomNumber, directionToLeaveOpen);
    }

    private Vector3 computeSpawnOffset(Vector3 relativePosition)
    {
        return transform.position + relativePosition;
    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 spawnStart = computeSpawnOffset(playerEntranceSpawn);
        Vector3 spawnEnd = computeSpawnOffset(playerEntranceWayPoint);
        Gizmos.DrawWireCube(spawnStart, new Vector3(.25f,.25f,.25f));
        Gizmos.DrawWireCube(spawnEnd, new Vector3(.25f,.25f,.25f));
        #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(spawnStart, "Start");
            UnityEditor.Handles.Label(spawnEnd, "End");
        #endif
    }
    
}



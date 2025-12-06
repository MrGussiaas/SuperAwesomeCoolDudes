using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHandler : MonoBehaviour
{
    [SerializeField]
    private Direction exitDirection;

    [SerializeField]
    private int nextRoomNumber;

    private const string PLAYER = "Player";

    public Direction ExitDirection {get{return exitDirection;}}
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

        GameEvents.OnActivateRoom(nextRoomNumber, directionToLeaveOpen);
    }
    
}



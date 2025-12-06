using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomController : NetworkBehaviour
{
    [SerializeField]
    private int roomId;

    [SerializeField]
    private Dictionary<Direction, GateHandler> gateHandlers = new();

    [SerializeField]
    private List<Direction> exitsToOpen;

    private List<ExitHandler> roomExits = new();

    [SyncVar(hook = nameof(OnActiveRoomChanged))]
    private bool isActiveRoom = false;

    private WaveOrchestrator waveOrchestrator;

    private PanCamera panCamera;

     public void Awake()
    {
        initVars();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
    private void initVars()
    {
        GateHandler[] gates = GetComponentsInChildren<GateHandler>(true);
        ExitHandler[] exits = GetComponentsInChildren<ExitHandler>(true);
        waveOrchestrator = GetComponentInChildren<WaveOrchestrator>(true);
        panCamera = Camera.main.GetComponent<PanCamera>();

        foreach (var gate in gates)
        {
            gateHandlers[gate.GetDirection] = gate;
        }
        foreach (var exit in exits)
        {
            roomExits.Add(exit);
        }
    }

    public void ActivateRoom(int newRoomId, Direction enteredFrom)
    {
        ServerActivateRoom(newRoomId, enteredFrom);
    }

    [Server]
    private void ServerActivateRoom(int newRoomId, Direction enteredFrom)
    {
        isActiveRoom = (newRoomId == roomId);

        waveOrchestrator.gameObject.SetActive(isActiveRoom);
        SetGateStatus(isActiveRoom, enteredFrom);
        //HandleGateOpen(enteredFrom);
        if(isActiveRoom){
            DoCameraPanRPC();
            Debug.Log("attempting rpc gate open: " + enteredFrom);
            RpcOpenGate(enteredFrom);
            EnemyServerSpawnerManager.Instance.UpdateLocation(transform.position);
            PowerUpServerSpawner.Instance.UpdateLocation(transform.position);
            
        }
        
    }

    private void SetGateStatus(bool status, Direction directionToLeaveOpen)
    {
        Debug.Log("Setting gate statuses but shoule leave open: " + directionToLeaveOpen);
        foreach (var kvp in gateHandlers)
        {
            GateHandler gate = kvp.Value;
            // Enable room gates if room is active
            gate.gameObject.SetActive(status);

        }
    }

    private void OnActiveRoomChanged(bool oldValue, bool newValue)
    {
        Debug.Log("UH OH!!!!!!!!");
        SetGateStatus(newValue, Direction.NULL);
    }

    [Server]
    private void HandleGateOpen(Direction direction)
    {
        if (!isActiveRoom)
        {
            return;
        }
        if (gateHandlers.TryGetValue(direction, out var handler))
        {
            handler.gameObject.SetActive(true);
            handler.OpenGate();
        }
        //RpcOpenGate(direction);
    }

    [Server]
    private void HandleGateClose(Direction direction)
    {
        if (!isActiveRoom)
        {
            return;
        }
        RpcCloseGate(direction);
    }

    public void RoomCleared()
    {
        for(int i = 0, n = roomExits.Count; i < n; i++)
        {
            if (!exitsToOpen.Contains(roomExits[i].ExitDirection))
            {
                continue;
            }
            roomExits[i].gameObject.SetActive(true);
            ActivateExit(i);
            RpcOpenGate(roomExits[i].ExitDirection);
        }
    }

    [ClientRpc]
    private void DoCameraPanRPC()
    {
        panCamera.PanCameraTo(transform.position);
    }

    [ClientRpc]
    private void ActivateExit(int exitIndex)
    {
        roomExits[exitIndex].gameObject.SetActive(true);
    }

    [ClientRpc]
    private void RpcOpenGate(Direction direction)
    {
        
        if (gateHandlers.TryGetValue(direction, out var handler))
        {
            handler.OpenGate();
        }
    }

    [ClientRpc]
    private void RpcCloseGate(Direction direction)
    {
        if (gateHandlers.TryGetValue(direction, out var handler))
        {
            handler.CloseGate();
        }
    }

    public void OnEnable()
    {
        GameEvents.OnActivateRoom += ActivateRoom;
        GameEvents.OnOpenGate += HandleGateOpen;
        GameEvents.OnCloseGate += HandleGateClose;
    }

    public void OnDisable()
    {
        GameEvents.OnActivateRoom -= ActivateRoom;
        GameEvents.OnOpenGate -= HandleGateOpen;
        GameEvents.OnCloseGate -= HandleGateClose;
    }
}

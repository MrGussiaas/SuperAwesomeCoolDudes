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

    [SyncVar(hook = nameof(OnActiveRoomChanged))]
    private bool isActiveRoom = false;

    public void Awake()
    {
         initVars();
    }

    private void initVars()
    {
        GateHandler[] gates = GetComponentsInChildren<GateHandler>(true);

        foreach (var gate in gates)
        {
            gateHandlers[gate.GetDirection] = gate;
        }
    }
    [Server]
    private void ActivateRoom(int newRoomId)
    {
        isActiveRoom = (newRoomId == roomId);
    }

    private void OnActiveRoomChanged(bool oldValue, bool newValue)
    {
        foreach (var kvp in gateHandlers)
        {
            GateHandler gate = kvp.Value;
            // Enable room gates if room is active
            gate.gameObject.SetActive(newValue);
        }
    }

    [Server]
    private void HandleGateOpen(Direction direction)
    {
        RpcOpenGate(direction);
    }

    [Server]
    private void HandleGateClose(Direction direction)
    {
        RpcCloseGate(direction);
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

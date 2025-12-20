using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomController : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

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

    public static RoomController ActiveRoom { get; private set; }

    private PanCamera panCamera;

    private bool isTransitioning = false;

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

    public void RespawnPlayer(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement?.EnableMovememnt(false);
        StartCoroutine(LerpPlayersToStart(new GameObject[]{player}, Direction.West));
    }

    [ClientRpc]
    private void KillPlayerVisual(GameObject player)
    {

    }

    public void ActivateRoom(int newRoomId, Direction enteredFrom)
    {
        ServerActivateRoom(newRoomId, enteredFrom);
    }

    private GameObject[] getPlayers()
    {
        return GameObject.FindGameObjectsWithTag(PLAYER_TAG);
    }

    private void spawnPlayersTo(GameObject[] players, Vector3 newPosition)
    {
        foreach(GameObject player in players)
        {
            player.transform.position = newPosition;
        } 
    }

    private Direction InvertDirection(Direction direction)
    {
        if(direction == Direction.East)
        {
            return Direction.West;
        }
        if(direction == Direction.West)
        {
            return Direction.East;
        }
        if(direction == Direction.North)
        {
            return Direction.South;
        }
        return Direction.North;

    }

    [Server]
    public void SyncLateJoinPlayer(GameObject playerObj)
    {
        Debug.Log("Syncing late player");
        if (!isActiveRoom)
            return;
        NetworkIdentity ni = playerObj.GetComponent<NetworkIdentity>();
        if (ni == null)
            return;
 
        TargetLateJoinSetup(ni.connectionToClient,transform.position);
        StartCoroutine(LerpPlayersToStart(new GameObject[]{playerObj}, Direction.West));
    }

    [TargetRpc]
    private void TargetLateJoinSetup( NetworkConnection target, Vector3 roomCenter)
    {
        //PanCamera cam = Camera.main.GetComponent<PanCamera>();
        panCamera.transform.position = new Vector3(roomCenter.x, roomCenter.y, panCamera.transform.position.z);
    }

    private IEnumerator LerpPlayersToStart(GameObject[] players,  Direction directionEnteringFrom)
    {
        
        HandleGateOpen(directionEnteringFrom);
        DisablePlayerControls(false, players);
        Vector3 teleportPosition = getEntranceSpawnForPlayer(directionEnteringFrom);
        Vector3 wayPointPosition = getEntranceFinishForPlayer(directionEnteringFrom);
        spawnPlayersTo(players, teleportPosition);
        TriggerEntranceAnimation(directionEnteringFrom, players);
        Debug.Log("Lerping late player to: " + wayPointPosition);
        yield return StartCoroutine(LerpPlayersTo(wayPointPosition, players));
        spawnPlayersTo(players, wayPointPosition);
        HandleGateClose(directionEnteringFrom);
        yield return null;
        DisablePlayerControls(true, players);
        
        yield return null;
    }

    private IEnumerator DoRoomTransition(Direction directionEnteringFrom)
    {
       // Direction realDirection = InvertDirection(directionEnteringFrom);
        Debug.Log("Do Room transition started  " + directionEnteringFrom);
        isTransitioning = true;
        EnemyServerSpawnerManager.Instance.UpdateLocation(transform.position);
        PowerUpServerSpawner.Instance.UpdateLocation(transform.position);
        HandleGateOpen(directionEnteringFrom);
        GameObject[] players = getPlayers();
        DisablePlayerControls(true, players);
        Vector3 teleportPosition = getEntranceSpawnForPlayer(directionEnteringFrom);
        Vector3 wayPointPosition = getEntranceFinishForPlayer(directionEnteringFrom);
        DoCameraPanRPC();
        yield return new WaitForSeconds(.1f);
        spawnPlayersTo(players, teleportPosition);
        TriggerEntranceAnimation(directionEnteringFrom, players);
        yield return StartCoroutine(LerpPlayersTo(wayPointPosition, players));
        spawnPlayersTo(players, wayPointPosition);
        yield return null;
        HandleGateClose(directionEnteringFrom);
        yield return null;
        GameEvents.OnRoomLoad?.Invoke();
        isTransitioning = false;
    }

    private void DeactivateExits()
    {
        foreach(ExitHandler exit in roomExits)
        {
            exit.gameObject.SetActive(false);
        }
    }

    private IEnumerator LerpPlayersTo(Vector3 endPosition, GameObject[] players)
    {
        float duration = .2f;
        if (players == null || players.Length == 0)
            yield break;

        // Record the starting positions
        Vector3[] startPositions = new Vector3[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            startPositions[i] = players[i].transform.position;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration); 

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    players[i].transform.position = Vector3.Lerp(startPositions[i], endPosition, t);
                }
            }

            yield return null; 
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
                players[i].transform.position = endPosition;
        } 
    }

    private void DisablePlayerControls(bool enable, GameObject[] players)
    {
        Debug.Log("Disabling player controls haha!");
        foreach(GameObject player in players)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement?.EnableMovememnt(enable);
        } 
    }

    private void TriggerEntranceAnimation(Direction directionEnteringFrom, GameObject[] players)
    {
        string trigger = "WalkEast";
        bool flipSprite = false;
        if(directionEnteringFrom == Direction.North)
        {
            trigger = "WalkSouth";
        }
        else if (directionEnteringFrom == Direction.South)
        {
            trigger = "WalkNorth";
        }
        else if(directionEnteringFrom == Direction.East)
        {
            flipSprite = true;
        }
        foreach(GameObject player in players)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement?.RpcPlayAnimation(trigger, flipSprite);
        } 

    }

    private void ServerActivateRoom(int newRoomId, Direction enteredFrom)
    {
        isActiveRoom = (newRoomId == roomId);
        if (isActiveRoom){
            ActiveRoom = this;
        }
        else
        {
            DeactivateExits();
        }

        waveOrchestrator.gameObject.SetActive(isActiveRoom);
        SetGateStatus(isActiveRoom, enteredFrom);
        HandleGateOpen(enteredFrom);
        if(isActiveRoom && !isTransitioning){
            
            StartCoroutine(DoRoomTransition(enteredFrom));

        }
        
    }

    private void SetGateStatus(bool status, Direction directionToLeaveOpen)
    {
        Debug.Log("Setting gate statuses but shoule leave open: " + directionToLeaveOpen);
        foreach (var kvp in gateHandlers)
        {
            GateHandler gate = kvp.Value;
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
        
        RpcOpenGate(direction);
    }

    [Server]
    private void HandleGateClose(Direction direction)
    {
        if (!isActiveRoom)
        {
            return;
        }
        if (gateHandlers.TryGetValue(direction, out var handler))
        {
            handler.CloseGate();
        }
        RpcCloseGate(direction);
    }

    private Vector3 getEntranceSpawnForPlayer(Direction direction)
    {
        for(int i = 0, n = roomExits.Count; i < n; i++)
        {
            if (roomExits[i].ExitDirection == direction)
            {
                return roomExits[i].transform.TransformPoint(roomExits[i].PlayerEntranceSpawn);
            }
        }   
        return Vector3.zero;
    }

    private Vector3 getEntranceFinishForPlayer(Direction direction)
    {
        for(int i = 0, n = roomExits.Count; i < n; i++)
        {
            if (roomExits[i].ExitDirection == direction)
            {
                return roomExits[i].transform.TransformPoint(roomExits[i].PlayerEntranceWayPoint);
            }
        }   
        return Vector3.zero;
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
            if (gateHandlers.TryGetValue(roomExits[i].ExitDirection, out var handler))
            {
                handler.OpenGate();
            }
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

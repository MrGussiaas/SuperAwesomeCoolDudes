using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        EnemyServerSpawnerManager.Instance.SpawnAllSpawners();
        NetworkServer.RegisterHandler<EnemyServerSpawner.RequestEnemySyncMessage>(OnEnemySyncRequest);
    }

    private void OnEnemySyncRequest(NetworkConnectionToClient conn, EnemyServerSpawner.RequestEnemySyncMessage msg)
    {
        // Go through all spawners and ask each to sync
        foreach (var spawner in EnemyServerSpawnerManager.Instance.GetAllSpawners())
            spawner.SyncVisualsToClient(conn);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
    }



    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // Notify all enemies that a new player exists
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.OnPlayerConnected();
        }
        GameObject newPlayer = conn.identity.gameObject;

        // Late join sync
        RoomController.ActiveRoom?.SyncLateJoinPlayer(newPlayer);
    }
    
    public override void OnStopClient()
    {
        base.OnStopClient();

        // Release all client-side visuals
        if (VisualEnemyManager.Instance != null)
            VisualEnemyManager.Instance.ReleaseAll();

        if (VisualBulletManager.Instance != null)
            VisualBulletManager.Instance.ReleaseAll();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Only the client side should request sync from the server.
        // A Host has BOTH server and client active, so we must guard this:

    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        if (!NetworkServer.active)
        {
            Debug.Log("SENDING RequestEnemySyncMessage");
            NetworkClient.Send(new EnemyServerSpawner.RequestEnemySyncMessage());
            NetworkClient.Send(new BulletServerManager.RequestBulletSyncMessage());
        }
    }
}

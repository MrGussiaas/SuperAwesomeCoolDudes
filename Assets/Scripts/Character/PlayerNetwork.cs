using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Start is called before the first frame update
    public override void OnStartServer()
    {
        PlayerRegistry.Instance.Register(gameObject);
    }

    public override void OnStopServer()
    {
        PlayerRegistry.Instance.Unregister(gameObject);
    }
}

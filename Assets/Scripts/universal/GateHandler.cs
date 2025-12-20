using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GateHandler : NetworkBehaviour
{
    [SerializeField]
    private Direction direction;

    public Direction GetDirection {get {return direction;}}

    private BoxCollider2D collider;

    private SpriteRenderer sr;

    private void InitVars()
    {
        if(sr == null){
            sr = GetComponent<SpriteRenderer>();
        }
        if(collider == null){
            collider = GetComponent<BoxCollider2D>();
        }
    }

    public void Awake()
    {
        InitVars();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        InitVars();
    }

    public void OpenGate()
    {
        sr.enabled = false;
        collider.enabled = false;
        
    }

    public void CloseGate()
    {
        sr.enabled = true;
        collider.enabled = true;
    }








}

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

    private Animator animator;

    private const string OPEN_GATE = "OpenGate";
    private const string CLOSE_GATE = "CloseGate";

    private HashSet<GateOpenReason> openReasons = new();

    private void InitVars()
    {
        if(sr == null){
            sr = GetComponent<SpriteRenderer>();
        }
        if(collider == null){
            collider = GetComponent<BoxCollider2D>();
        }
        if(animator == null)
        {
            animator = GetComponent<Animator>();
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

    private void OpenGateInternal()
    {
        if(sr != null){
            sr.enabled = false;
        }
        if(animator != null)
        {
            animator.SetTrigger(OPEN_GATE);
        }

        
    }

    public void RequestGateOpen(GateOpenReason reason)
    {
        openReasons.Add(reason);
        if(reason == GateOpenReason.ArenaCleared){
            collider.enabled = false;
        }
        OpenGateInternal();
    }

    public void RequestGateClose(GateOpenReason reason)
    {
        openReasons.Remove(reason);
        if(openReasons.Count == 0)
        {
            CloseGateInternal();
        }
    }


    private void CloseGateInternal()
    {
        if(sr != null){
            sr.enabled = true;
        }
        if(animator != null)
        {
            animator.SetTrigger(CLOSE_GATE);
        }
        collider.enabled = true;
    }








}

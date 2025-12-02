using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateHandler : MonoBehaviour
{
    [SerializeField]
    private Direction direction;

    public Direction GetDirection {get {return direction;}}

    private BoxCollider2D collider;

    private SpriteRenderer sr;

    private void InitVars()
    {
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
    }

    public void Awake()
    {
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

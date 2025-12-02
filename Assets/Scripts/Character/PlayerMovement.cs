using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private InputHandler inputHandler;

    private static int MOVE_SPEED = 5;

    private Vector2 moveDirection;

    private Rigidbody2D rb;
    void Awake()
    {
        initInternals();
    }
    
    private void initInternals()
    {
        inputHandler = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        int upDirection = inputHandler.UpHeld ? 1 : (inputHandler.DownHeld ? -1 : 0);
        int rightDirection = inputHandler.RightHeld ? 1 : (inputHandler.LeftHeld ? -1 : 0);
        moveDirection = new Vector3(rightDirection, upDirection, 0).normalized;
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Vector2 newPos = rb.position + moveDirection * MOVE_SPEED * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}

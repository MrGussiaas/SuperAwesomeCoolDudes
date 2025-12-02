using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{

    public bool UpHeld { get; private set; }
    public bool DownHeld { get; private set; }
    public bool LeftHeld { get; private set; }
    public bool RightHeld { get; private set; }

    public bool ShootHeld { get; private set; }

    public Vector2 MouseWorldPosition { get; private set; }

    private PlayerControls controls;

    private Camera mainCamera;

    void Awake()
    {
        controls = new PlayerControls();
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        controls.PlayerMovement.WalkUp.performed += onWalkUpPerformed;
        controls.PlayerMovement.WalkUp.canceled  += onWalkUpCancelled;

        controls.PlayerMovement.WalkDown.performed  += onWalkDownPerformed;
        controls.PlayerMovement.WalkDown.canceled  += onWalkDownCancelled;

        controls.PlayerMovement.WalkRight.started += onWalkRightPerformed;
        controls.PlayerMovement.WalkRight.canceled += onWalkRightCancelled;
        
        controls.PlayerMovement.WalkLeft.started += onWalkLeftPerformed;
        controls.PlayerMovement.WalkLeft.canceled += onWalkLeftCancelled;

        controls.PlayerMovement.Shoot.started += onShootPerformed;
        controls.PlayerMovement.Shoot.canceled += onShootCancelled;

        controls.PlayerMovement.Aim.performed += setAimPoint;

        controls.PlayerMovement.Pause.performed += DoPause;

        controls.PlayerMovement.Enable();
    }

    void OnDisable()
    {
        controls.PlayerMovement.WalkUp.performed -= onWalkUpPerformed;
        controls.PlayerMovement.WalkUp.canceled -= onWalkUpCancelled;

        controls.PlayerMovement.WalkDown.performed -= onWalkDownPerformed;
        controls.PlayerMovement.WalkDown.canceled -= onWalkDownCancelled;

        controls.PlayerMovement.WalkRight.started -= onWalkRightPerformed;
        controls.PlayerMovement.WalkRight.canceled -= onWalkRightCancelled;

        controls.PlayerMovement.WalkLeft.started -= onWalkLeftPerformed;
        controls.PlayerMovement.WalkLeft.canceled -= onWalkLeftCancelled;

        controls.PlayerMovement.Shoot.started -= onShootPerformed;
        controls.PlayerMovement.Shoot.canceled -= onShootCancelled;

        controls.PlayerMovement.Aim.performed -= setAimPoint;

        controls.PlayerMovement.Pause.started -= DoPause;

        controls.PlayerMovement.Disable();
    }

    private void DoPause(InputAction.CallbackContext context)
    {
        Time.timeScale = Time.timeScale < 1 ? 1 : .1f;
    }

    private void setAimPoint(InputAction.CallbackContext context)
    {
        Vector2 mouseScreenPos = context.ReadValue<Vector2>();

        if (mainCamera != null)
        {
            MouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        }

    }

    private void onShootPerformed(InputAction.CallbackContext context)
    {
        ShootHeld = true;
    }

    private void onShootCancelled(InputAction.CallbackContext context)
    {
        ShootHeld = false;
    }

    private void onWalkUpPerformed(InputAction.CallbackContext context)
    {
        UpHeld = true;
    }

    private void onWalkUpCancelled(InputAction.CallbackContext context)
    {
        UpHeld = false;
    }

    private void onWalkRightPerformed(InputAction.CallbackContext context)
    {
        RightHeld = true;
    }

    private void onWalkRightCancelled(InputAction.CallbackContext context)
    {
        RightHeld = false;
    }

    private void onWalkLeftPerformed(InputAction.CallbackContext context)
    {
        LeftHeld = true;
    }

    private void onWalkLeftCancelled(InputAction.CallbackContext context)
    {
        LeftHeld = false;
    }

    private void onWalkDownPerformed(InputAction.CallbackContext context)
    {
        DownHeld = true;
    }

    private void onWalkDownCancelled(InputAction.CallbackContext context)
    {
        DownHeld = false;
    }
    
}


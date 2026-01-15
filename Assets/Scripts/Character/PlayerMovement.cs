using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    private const string WALK_SOUTH = "WalkSouth";

    private const string WALK_SOUTH_22 = "WalkSouth22";

    
    private const string WALK_SOUTH_EAST = "WalkSouthEast";

    private const string WALK_SOUTH_67 = "WalkSouth67";
    private const string WALK_EAST_112 = "WalkEast112";
    private const string WALK_EAST = "WalkEast";
    private const string WALK_NORTH_EAST = "WalkNorthEast";

     private const string WALK_EAST_157 = "WalkNorthEast157";
    private const string WALK_NORTH = "WalkNorth";

    private const string SHOOT_SOUTH = "ShootSouth";
    private const string SHOOT_SOUTH_22 = "ShootSouth22";
    private const string SHOOT_SOUTH_EAST = "ShootSouthEast";
    private const string SHOOT_SOUTH_67 = "ShootSouth67";
    private const string SHOOT_EAST = "ShootEast";
    private const string SHOOT_EAST_112 = "ShootEast112";
    private const string SHOOT_NORTH_EAST = "ShootNorthEast";
    private const string SHOOT_EAST_157 = "ShootEast157";
    private const string SHOOT_NORTH = "ShootNorth";

    private const string GUN = "Gun";

    private const string GO_IDLE = "GoIdle";

    private InputHandler inputHandler;

    private Shoot gunAim;

    private static int MOVE_SPEED = 5;

    private Vector2 moveDirection;

    private SpriteRenderer sr;

    private Animator anim;

    private Rigidbody2D rb;

    private Vector3 NORTH = new Vector3(0,1,0);
    private Vector3 NORTH_EAST = new Vector3(1,1,0);
    private Vector3 NORTH_EAST_157 = new Vector3(1,.75f,0);
    private Vector3 NORTH_WEST_157 = new Vector3(-1,.75f,0);
    private Vector3 EAST = new Vector3(1,0,0);
    private Vector3 EAST_112 = new Vector3(1,.5f,0);
    private Vector3 WEST_112 = new Vector3(-1,.5f,0);
    private Vector3 SOUTH_EAST = new Vector3(1,-1,0);

    private Vector3 SOUTH_64 = new Vector3(.75f,-1,0);
    private Vector3 SOUTH_WEST_64 = new Vector3(-.75f,-1,0);

    private Vector3 SOUTH = new Vector3(0,-1,0);

    private Vector3 SOUTH_22 = new Vector3(.25f,-1,0);
    private Vector3 SOUTH_WEST_22 = new Vector3(-.25f,-1,0);
    private Vector3 SOUTH_WEST = new Vector3(-1,-1,0);
    private Vector3 WEST =  new Vector3(-1,0,0);
    private Vector3 NORTH_WEST = new Vector3(-1,1,0);

    [SyncVar]
    private bool movementEnabled = true;


    void Awake()
    {
        initInternals();
    }

    public void EnableMovememnt(bool enableMovemment)
    {
        movementEnabled = enableMovemment;
        gunAim.enabled = enableMovemment;
    }
    
    private void initInternals()
    {
        inputHandler = GetComponent<InputHandler>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        for(int i = 0, n = transform.childCount; i < n; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (obj.CompareTag(GUN))
            {
                gunAim = obj.GetComponent<Shoot>();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        int upDirection = inputHandler.UpHeld ? 1 : (inputHandler.DownHeld ? -1 : 0);
        int rightDirection = inputHandler.RightHeld ? 1 : (inputHandler.LeftHeld ? -1 : 0);
        if (!isLocalPlayer || !movementEnabled)
        {
            return;
        }

        Vector3 newDirection = new Vector3(rightDirection, upDirection, 0);
        
        if (inputHandler.ShootHeld && newDirection == Vector3.zero)
        {
            TriggerShootAnimation(GetDirection8(gunAim.GetAimDirection()));
        }
        else if(inputHandler.ShootHeld && newDirection != Vector3.zero)
        {
           TriggerWalkAnimation(GetDirection8(gunAim.GetAimDirection()));
        }
        else
        {
            TriggerWalkAnimation(newDirection);
        }
        CmdSetMoveDirection(newDirection.normalized);
    }

    [Command]
    void CmdSetMoveDirection(Vector2 dir)
    {
        moveDirection = dir;
    }

    Vector3 GetDirection8(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return Vector3.zero;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        if (angle >= 260f)
        {
            if (angle <= 280f)
                return SOUTH;

            if (angle < 302f)
                return SOUTH_22;

            if (angle < 325f)
                return SOUTH_EAST;

            if (angle < 344f)
                return SOUTH_64;

            return EAST; // 344 → 360
        }

 
         if (angle < 10f){
            return EAST_112;
        }
         if (angle < 30f){
            return NORTH_EAST;
        }

        if(angle < 35f)
        {
            return NORTH_EAST;
        }
        if (angle < 60f){
            return NORTH_EAST_157;
        }
        if (angle < 90){
            return NORTH;
        }
        if (angle < 110f){
            return NORTH;
        }
        if (angle < 135){
            return NORTH_WEST_157;
        }
        if (angle < 170){
            return NORTH_WEST;
        }
        if (angle < 190){
            return WEST_112;
        }

        if (angle < 225){
            return SOUTH_WEST;
        }
        if (angle < 300){
            return SOUTH_WEST_22;
        }









        return NORTH; // 88 → 90+

        if (angle < 191.25f) return WEST;
        if (angle < 213.75f) return SOUTH_WEST;
        if (angle < 236.25f) return SOUTH_64;
        if (angle < 258.75f) return SOUTH;
        if (angle < 281.25f) return SOUTH_22;
        if (angle < 303.75f) return SOUTH_EAST;
        if (angle < 326.25f) return SOUTH_64;

        return EAST;
    }
    
    private void TriggerShootAnimation(Vector3 aimDirection)
    {
        if(aimDirection == Vector3.zero)
        {
            return;
        }
        sr.flipX = aimDirection.x < 0;
        Vector3 absVector = new Vector3(Mathf.Abs(aimDirection.x), aimDirection.y, Mathf.Abs(aimDirection.z));
        if(absVector == NORTH)
        {
            anim.SetTrigger(SHOOT_NORTH);
            CmdSendAnimationUpdate(SHOOT_NORTH, sr.flipX);
        }
        else if(absVector == EAST_112)
        {
            anim.SetTrigger(SHOOT_EAST_112);
            CmdSendAnimationUpdate(SHOOT_EAST_112, sr.flipX);

        }
        else if(absVector == NORTH_EAST_157)
        {
            anim.SetTrigger(SHOOT_EAST_157);
            CmdSendAnimationUpdate(SHOOT_EAST_157, sr.flipX);

        }
        else if(absVector == NORTH_EAST)
        {
            anim.SetTrigger(SHOOT_NORTH_EAST);
            CmdSendAnimationUpdate(SHOOT_NORTH_EAST, sr.flipX);
        }
        else if(absVector == EAST)
        {
            anim.SetTrigger(SHOOT_EAST);
            CmdSendAnimationUpdate(SHOOT_EAST, sr.flipX);
        }
        else if(absVector == SOUTH_EAST)
        {
            anim.SetTrigger(SHOOT_SOUTH_EAST);
            CmdSendAnimationUpdate(SHOOT_SOUTH_EAST, sr.flipX);
        }
        else if(absVector == SOUTH_22)
        {
            anim.SetTrigger(SHOOT_SOUTH_22);
            CmdSendAnimationUpdate(SHOOT_SOUTH_22, sr.flipX);
        }
        else if(absVector == SOUTH_64)
        {
            anim.SetTrigger(SHOOT_SOUTH_67);
            CmdSendAnimationUpdate(SHOOT_SOUTH_67, sr.flipX);
        }

        else if(absVector == SOUTH)
        {
            anim.SetTrigger(SHOOT_SOUTH);
            CmdSendAnimationUpdate(SHOOT_SOUTH, sr.flipX);
        }
    }

    private void TriggerWalkAnimation(Vector3 newDirection)
    {
        if(newDirection == Vector3.zero)
        {
            anim.SetTrigger(GO_IDLE);
            CmdSendAnimationUpdate(GO_IDLE, sr.flipX);
            return;
        }
        sr.flipX = newDirection.x < 0;
        Vector3 absVector = new Vector3(Mathf.Abs(newDirection.x), newDirection.y, Mathf.Abs(newDirection.z));
        if(absVector == NORTH)
        {
            anim.SetTrigger(WALK_NORTH);
            CmdSendAnimationUpdate(WALK_NORTH, sr.flipX);
        }
        else if(absVector == NORTH_EAST_157)
        {
            anim.SetTrigger(WALK_EAST_157);
            CmdSendAnimationUpdate(WALK_EAST_157, sr.flipX);
        }
        else if(absVector == EAST_112)
        {
            anim.SetTrigger(WALK_EAST_112);
            CmdSendAnimationUpdate(WALK_EAST_112, sr.flipX);
        }
        else if(absVector == NORTH_EAST)
        {
            anim.SetTrigger(WALK_NORTH_EAST);
            CmdSendAnimationUpdate(WALK_NORTH_EAST, sr.flipX);
        }
        else if(absVector == EAST)
        {
            anim.SetTrigger(WALK_EAST);
            CmdSendAnimationUpdate(WALK_EAST, sr.flipX);
        }
        else if(absVector == SOUTH_64)
        {
            anim.SetTrigger(WALK_SOUTH_67);
            CmdSendAnimationUpdate(WALK_SOUTH_67, sr.flipX);
        }
        else if(absVector == SOUTH_EAST)
        {
            anim.SetTrigger(WALK_SOUTH_EAST);
            CmdSendAnimationUpdate(WALK_SOUTH_EAST, sr.flipX);
        }
        else if(absVector == SOUTH_22)
        {
            anim.SetTrigger(WALK_SOUTH_22);
            CmdSendAnimationUpdate(WALK_SOUTH_22, sr.flipX);
        }
        else if(absVector == SOUTH)
        {
            anim.SetTrigger(WALK_SOUTH);
            CmdSendAnimationUpdate(WALK_SOUTH, sr.flipX);
        }
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (!movementEnabled)
        {
            return;
        }
        Vector2 newPos = rb.position + moveDirection * MOVE_SPEED * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    [Command]
    void CmdSendAnimationUpdate(string trigger, bool flipSprite)
    {
        // Server calls RPC
        RpcPlayAnimation(trigger, flipSprite);
    }

    [ClientRpc]
    public void RpcPlayAnimation(string trigger, bool flipSprite)
    {
        sr.flipX = flipSprite;
        anim.SetTrigger(trigger);
    }
}

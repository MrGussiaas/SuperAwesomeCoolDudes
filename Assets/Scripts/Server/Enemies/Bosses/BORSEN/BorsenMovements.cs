using System.Collections;
using Mirror;
using UnityEngine;

public class BorsenMovements : NetworkBehaviour
{

    private enum BorsenStates {IDLE, MOVING, SHOOTING, GRABBING_ORB, PLACING_ORB, DAMAGED, LASER_SWEEP_LEFT, LASER_SWEEP_RIGHT}

    private BorsenStates currentState = BorsenStates.IDLE;

    private Vector2 movementDirection;

    private Vector2 previousMoveDirection = Vector2.right;

    private Vector3 movementStart;

    private Vector3 movementEnd;

    private Animator anim;

    private float movementTime = 0;
    private float elapsedTime = 0;

    private int movementCounter = 0;

    private const float MAX_ANGLE_OFFSET = 10f;

    private const float MOVEMENT_SPEED_FORWARD = 1.5f;

    private float movementSpeedBackward = .5f;

    private const string GRAB_LEFT_ORB_BEGIN = "GrabLeftOrbBegin";

    private const string GRAB_RIGHT_ORB_BEGIN = "GrabRightOrbBegin";

     private const string LASER_SWEEP_RIGHT= "StartLaserSweepRightToLeft";
     private const string LASER_SWEEP_LEFT= "StartLaserSweepLeftToRightStart";

    private static string LEFT_ORB = "LeftOrb";
    private static string RIGHT_ORB = "RightOrb";

    private const string TAKE_DAMAGE= "TakeDamage";
     private const string TAKE_DAMAGE_ANIMATION= "Damaged";

     private Orb leftOrb;
    private Orb rightOrb;

    

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        foreach (Transform child in transform){
            if (child.CompareTag(RIGHT_ORB))
            {
                rightOrb = child.GetComponent<Orb>();
            }
            if (child.CompareTag(LEFT_ORB))
            {
                leftOrb = child.GetComponent<Orb>();
            }
           
        }
    }

    private void SetUpMovementStateVars()
    {
        currentState = BorsenStates.MOVING;
        
        elapsedTime = 0;
        float movementDistance = (Random.value * .15f) + .1f;
        movementStart = transform.position;
        movementEnd = movementStart + (Vector3)(movementDirection * movementDistance);
        movementTime = movementDistance / MOVEMENT_SPEED_FORWARD;
        movementDirection = ComputeNewDirection();
        if(movementDirection == Vector2.zero)
        {
           currentState = BorsenStates.IDLE; 
        }
        
    }

    private void SetUpGrabbingOrbStateVars()
    {
        currentState = BorsenStates.GRABBING_ORB;
        float diceRoll = Random.value;
        if (diceRoll <.5f){
            anim.SetTrigger(GRAB_LEFT_ORB_BEGIN);
            FireAnimationTriggerToClients(GRAB_LEFT_ORB_BEGIN);
        }
        else{  
            anim.SetTrigger(GRAB_RIGHT_ORB_BEGIN);
            FireAnimationTriggerToClients(GRAB_RIGHT_ORB_BEGIN);
        }
    }

    
    public void OrbPlacement()
    {
        if (isServer)
        {
            Debug.Log("Server Orb Placement");
        }
        else
        {
            return;
        }
        currentState = BorsenStates.IDLE;
        elapsedTime = 0;
        movementCounter = 0;
    }

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        if(currentState == BorsenStates.IDLE)
        {
            float diceRoll = Random.value;
            if(diceRoll <= .65f)
            {
                SetUpMovementStateVars();
            }
            else
            {
                SetUpGrabbingOrbStateVars();
            }
        }
        else if(currentState == BorsenStates.GRABBING_ORB)
        {
            
        }
        else if(currentState == BorsenStates.MOVING)
        {
            DoNextMovement();
        }
        
    }

    private void DoNextMovement()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime > movementTime)
        {
            currentState = BorsenStates.IDLE;
            elapsedTime = 0;
            movementCounter ++;
            return;
        }
        Vector3 nextPosition = Vector3.Lerp(movementStart, movementEnd, elapsedTime / movementTime);
        transform.position = nextPosition;
    }

    private void DoNextOrbPlacement()
    {
        
    }

    private Vector2 ComputeNewDirection()
    {
        
        Transform closestPlayer = PlayerRegistry.Instance.GetNearest(transform.position);
        if(closestPlayer == null)
        {
            return Vector2.zero;
        }
        Vector2 toPlayer = (closestPlayer.position - transform.position);
        Vector2 cardinalVector = Vector2.zero;
        if(movementCounter % 2 == 0)
        {
            cardinalVector = -previousMoveDirection;
        }
        else if(Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
        {
           cardinalVector = toPlayer.x < 0 ? Vector2.left : Vector2.right; 
        }
        else
        {
            cardinalVector = toPlayer.y < 0 ? Vector2.down : Vector2.up;
        }
        float angle = Random.Range(-MAX_ANGLE_OFFSET, MAX_ANGLE_OFFSET);
        Vector2 cheatedDir = Quaternion.Euler(0,0,angle) * cardinalVector;
        previousMoveDirection = cheatedDir;
        return cheatedDir.normalized;
    }

    [ClientRpc]
    private void FireAnimationTriggerToClients(string triggerName)
    {
        if (isServer)
        {
            return;
        }
        anim.SetTrigger(triggerName);
    }

    [ClientRpc]
    private void FireAnimationTriggerResetToClients(string triggerName){
         if (isServer)
        {
            return;
        }
        anim.ResetTrigger(triggerName);
        
    }
    [ClientRpc]
    private void FireAnimationPlayToClients(string animationName)
    {
        if (isServer)
        {
            return;
        }
        anim.Play(animationName);
    }

    private void RightOrbDamage()
    {
        if (!isServer)
        {
            return;
        }
        Debug.Log("Borsen reset");
        anim.ResetTrigger(GRAB_RIGHT_ORB_BEGIN);
        FireAnimationTriggerResetToClients(GRAB_RIGHT_ORB_BEGIN);
        currentState = BorsenStates.DAMAGED;
        anim.Play(TAKE_DAMAGE_ANIMATION);
        FireAnimationPlayToClients(TAKE_DAMAGE_ANIMATION);
        elapsedTime = 0;
        movementCounter = 0;
        StartCoroutine(BorsenDamageRoutine(false));
    }

    private void LeftOrbDamage()
    {
        if (!isServer)
        {
            return;
        }
        Debug.Log("Borsen reset");
        anim.ResetTrigger(GRAB_LEFT_ORB_BEGIN);
        FireAnimationTriggerResetToClients(GRAB_LEFT_ORB_BEGIN);
        currentState = BorsenStates.DAMAGED;
        anim.Play(TAKE_DAMAGE_ANIMATION);
        FireAnimationPlayToClients(TAKE_DAMAGE_ANIMATION);
        elapsedTime = 0;
        movementCounter = 0;
        StartCoroutine(BorsenDamageRoutine(true));
    }

    private void CompleteLaserSweep()
    {
        currentState = BorsenStates.IDLE;
    }


    private IEnumerator BorsenDamageRoutine(bool left)
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("re-setting Borsen to IDLE");
        currentState = left ? BorsenStates.LASER_SWEEP_LEFT : BorsenStates.LASER_SWEEP_RIGHT;
        string animationTrigger = left ? LASER_SWEEP_LEFT : LASER_SWEEP_RIGHT;
        anim.SetTrigger(animationTrigger);
        FireAnimationTriggerToClients(animationTrigger);
        elapsedTime = 0;
        movementCounter = 0;
        
    }

    private void OnEnable()
    {
        rightOrb.OnOrbExploded += RightOrbDamage;
        leftOrb.OnOrbExploded += LeftOrbDamage;
    }

    private void OnDisable()
    {
        rightOrb.OnOrbExploded -= RightOrbDamage;
        leftOrb.OnOrbExploded -= LeftOrbDamage;
    }
}

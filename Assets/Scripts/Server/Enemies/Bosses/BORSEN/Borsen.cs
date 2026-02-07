
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Borsen : NetworkBehaviour
{
    private List<BorsenEye> borsenEyes = new();

    private Animator animator;
    private Orb leftOrb;
    private Orb rightOrb;
    private static string LEFT_EYE = "BorsenEyes1";
    private static string LEFT_ORB = "LeftOrb";
    private static string RIGHT_ORB = "RightOrb";

    public const float ANIMATION_SPEED = .5f;

    [ServerCallback]
    public void EndSweep()
    {
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.EndSweep();
        }
        EndSweepOnClient();
    }

    [ClientRpc]
    private void EndSweepOnClient()
    {
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.EndSweep();
        } 
    }

    [ServerCallback]
    public void StartLeftToRightSweep()
    {
        
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.BeginLeftSweep();
        }
        SendLeftEyeSweepToClients();
    }

    [ServerCallback]
    public void StartRightToLeftSweep()
    {
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.BeginRightSweep();
        }
        SendRightEyeSweepToClients();
    }

    [ClientRpc]
    private void SendLeftEyeSweepToClients()
    {
        if (isServer)
        {
            return;
        }
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.BeginLeftSweep();
        }
    }

    [ClientRpc]
    private void SendRightEyeSweepToClients()
    {
        if (isServer)
        {
            return;
        }
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.BeginRightSweep();
        }
    }

    public void SyncSweepMid()
    {
        Debug.Log("Sync eye midpoint");
        foreach(BorsenEye eye in borsenEyes)
        {
            eye.SyncMidPoint();
        }
    }

   private void ActivateLeftOrb()
    {
        if (!isServer)
        {
            return;
        }
        leftOrb.ActivateOrb();
        SendLeftOrbActivationToClient();
    }
    public void ActivateRightOrb()
    {
        if (!isServer)
        {
            return;
        }
        rightOrb.ActivateOrb();
        SendRightOrbActivationToClient();
    }

    [ClientRpc]
    private void SyncMidPointEyesClient()
    {
      foreach(BorsenEye eye in borsenEyes)
        {
            eye.SyncMidPoint();
        }
    }

    [ClientRpc]
    private void SendLeftOrbActivationToClient()
    {
        leftOrb.ActivateOrb();
    }

    [ClientRpc]
    private void SendRightOrbActivationToClient()
    {
        rightOrb.ActivateOrb();
    }

    [ClientRpc]
    private void SendRightOrbDeActivationToClient()
    {
        rightOrb.DeactivateOrb();
    }

    [ClientRpc]
    private void SendLeftOrbDeActivationToClient()
    {
        leftOrb.DeactivateOrb();
    }

    [ServerCallback]
    public void PlaceActiveOrb()
    {

        if (rightOrb.IsActive())
        {
            BulletServerManager.Instance.SpawnBorsenProjectilesOnServer(rightOrb.transform.position);
            rightOrb.DeactivateOrb();
            SendRightOrbDeActivationToClient();
        }
        if (leftOrb.IsActive())
        {
            BulletServerManager.Instance.SpawnBorsenProjectilesOnServer(leftOrb.transform.position);
            leftOrb.DeactivateOrb();
            SendLeftOrbDeActivationToClient();
        }
        
    }




    public void GrabOrb()
    {
        if (rightOrb.IsActive())
        {
            rightOrb.GrabOrb();
        }
        if (leftOrb.IsActive())
        {
            leftOrb.GrabOrb();
        }
    }

    public void SyncOrbMidPoint()
    {
        if (rightOrb.IsActive())
        {
            rightOrb.SyncMidPoint();
        }
        if (leftOrb.IsActive())
        {
            leftOrb.SyncMidPoint();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = ANIMATION_SPEED;
        foreach (Transform child in transform){
            if (child.CompareTag(LEFT_EYE))
            {
                 BorsenEye eye = child.GetComponent<BorsenEye>();
                 borsenEyes.Add(eye);
            }
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

    private void TakeRightDamage()
    {
        Debug.Log("Borsen official right damage here");
        rightOrb.DeactivateOrb();
        SendRightOrbDeActivationToClient();
    }

    private void TakeLeftDamage()
    {
        Debug.Log("Borsen official left damage here");
        leftOrb.DeactivateOrb();
        SendLeftOrbDeActivationToClient();
    }

    private void OnEnable()
    {
        rightOrb.OnOrbExploded += TakeRightDamage;
        leftOrb.OnOrbExploded += TakeLeftDamage;
    }

    private void OnDisable()
    {
        rightOrb.OnOrbExploded -= TakeRightDamage;
        leftOrb.OnOrbExploded -= TakeLeftDamage;
    }
}




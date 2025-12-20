using Mirror;
using UnityEngine;

public class Shoot : NetworkBehaviour
{
    private InputHandler inputHandler;

    private const int BULLET_SPEED = 10;

    private float timeSinceLastShot = 100;

    private AbilityPickup abilityPickup;

    [SerializeField] private GameObject serverBulletPrefab;
    

    void Awake()
    {
        inputHandler = GetComponentInParent<InputHandler>();
        abilityPickup = GetComponentInParent<AbilityPickup>();
    }

    public Vector3 GetAimDirection()
    {
        Vector3 mousePos3D = (Vector3)inputHandler.MouseWorldPosition;
        return (mousePos3D - transform.position).normalized; 
    }

    void Update()
    {
        if (!isLocalPlayer) return; // Only handle input for the local player
        timeSinceLastShot += Time.deltaTime;
        if (inputHandler.ShootHeld && timeSinceLastShot >= .1f)
        {
            Debug.Log("Shooting with the following ability: " + abilityPickup.SpreadShotAmmo);
            Vector3 shootDir = GetAimDirection();
            if(abilityPickup.SpreadShotAmmo > 0)
            {
                CmdShoot(transform.position, Rotate2D(shootDir, 15), BulletType.Basic, AbilitiesEnum.SPREAD_SHOT);
                CmdShoot(transform.position, Rotate2D(shootDir, -15), BulletType.Basic, AbilitiesEnum.NA);
                CmdShoot(transform.position, shootDir, BulletType.Basic, AbilitiesEnum.NA);
                //abilityPickup.ShootSpreadShot();

            }
            else if(abilityPickup.RocketAmmo > 0)
            {
                //abilityPickup.ShootRocket();
                CmdShoot(transform.position, shootDir, BulletType.Rocket, AbilitiesEnum.ROCKET_SHOT);
            }
            else
            {
                CmdShoot(transform.position, shootDir, BulletType.Basic, AbilitiesEnum.NA);
            }
            timeSinceLastShot = 0;
        }
    }



    private Vector3 Rotate2D(Vector3 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }
    

    [Command]
    void CmdShoot(Vector3 position, Vector3 direction, BulletType bulletType, AbilitiesEnum abilityType)
    {
        if(abilityType == AbilitiesEnum.ROCKET_SHOT)
        {
            abilityPickup.ShootRocket();
        }
        if(abilityType == AbilitiesEnum.SPREAD_SHOT)
        {
            abilityPickup.ShootSpreadShot();
        }
        BulletServerManager.Instance.SpawnBulletOnServer(position, direction, bulletType);
    }


}

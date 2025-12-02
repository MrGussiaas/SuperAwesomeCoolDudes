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

    void Update()
    {
        if (!isLocalPlayer) return; // Only handle input for the local player
        timeSinceLastShot += Time.deltaTime;
        if (inputHandler.ShootHeld)
        {
            Debug.Log("Shooting with the following ability: " + abilityPickup.SpreadShotAmmo);
            Vector3 mousePos3D = (Vector3)inputHandler.MouseWorldPosition;
            Vector3 shootDir = (mousePos3D - transform.position).normalized;
            
            if(abilityPickup.SpreadShotAmmo > 0)
            {
                CmdShoot(transform.position, Rotate2D(shootDir, 15), BulletType.Basic);
                CmdShoot(transform.position, Rotate2D(shootDir, -15), BulletType.Basic);
                CmdShoot(transform.position, shootDir, BulletType.Basic);
                abilityPickup.ShootSpreadShot();

            }
            else if(abilityPickup.RocketAmmo > 0)
            {
                abilityPickup.ShootRocket();
                CmdShoot(transform.position, shootDir, BulletType.Rocket);
            }
            else
            {
                CmdShoot(transform.position, shootDir, BulletType.Basic);
            }
            timeSinceLastShot = 0;
        }
    }

    private Vector3 Rotate2D(Vector3 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }
    

    [Command]
    void CmdShoot(Vector3 position, Vector3 direction, BulletType bulletType)
    {
        BulletServerManager.Instance.SpawnBulletOnServer(position, direction, bulletType);
    }


}

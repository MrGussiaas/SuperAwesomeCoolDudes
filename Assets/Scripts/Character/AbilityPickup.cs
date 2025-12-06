using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AbilityPickup : NetworkBehaviour
{
    private int spreadShotAmmo;

    private int rocketAmmo;

    [SerializeField]
    private GameObject sawBladePrefab;

    private List<SawBlade> sawBlades = new();

    private const int BLADE_COUNT = 5;

    private const float DIST_FROM_PARENT = .75f;

    

    public int SpreadShotAmmo {get {return spreadShotAmmo;}}

    public int RocketAmmo {get {return rocketAmmo;}}

    public void DoPickeup(AbilitiesEnum powerUp, int ammoAmount)
    {
        switch (powerUp){
            case AbilitiesEnum.SPREAD_SHOT :
                spreadShotAmmo = ammoAmount;
                break;
            case AbilitiesEnum.ROCKET_SHOT :
                rocketAmmo = ammoAmount;
                break;
            case AbilitiesEnum.SAW_BLADE :
                InitializeSawBlades();
                break;
        }
    }

    public void Awake()
    {
       InitVars(); 
    }

    private void InitVars()
    {
        for(int i = 0, n = BLADE_COUNT; i < n; i++)
        {
            GameObject sawBladeInstance = Instantiate(sawBladePrefab, transform);
            sawBlades.Add(sawBladeInstance.GetComponent<SawBlade>());
            sawBladeInstance.SetActive(false);
        }
        //InitializeSawBlades();
    }

    private void InitializeSawBlades()
    {
        float degreeStep = 360f / BLADE_COUNT;

        for (int i = 0; i < BLADE_COUNT; i++)
        {
            SawBlade blade = sawBlades[i];

            float angleDegrees = degreeStep * i;
            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(
                Mathf.Cos(angleRadians),
                Mathf.Sin(angleRadians)
            ) * DIST_FROM_PARENT;

            blade.transform.localPosition = offset;

            blade.transform.localRotation = Quaternion.Euler(0, 0, angleDegrees);
            blade.Initialize();
            blade.gameObject.SetActive(true);
            blade.IndexId = i;
            if(isServer){
                ActivateBlade(i, blade.transform.localRotation, blade.transform.localPosition);
            }
        }

    }

    public void ShootSpreadShot()
    {
        spreadShotAmmo--;
    }

    public void ShootRocket()
    {
        rocketAmmo--;
    }

    [ClientRpc]
    public void DeactivateBlade(int bladeIndex)
    {
        if(bladeIndex < 0 || bladeIndex >= sawBlades.Count)
        {
            return;
        }
        sawBlades[bladeIndex].gameObject.SetActive(false);
    }

    [ClientRpc]
    public void ActivateBlade(int bladeIndex, Quaternion rotation, Vector3 position)
    {
        if(bladeIndex < 0 || bladeIndex >= sawBlades.Count)
        {
            return;
        }
        SawBlade blade = sawBlades[bladeIndex];
        blade.transform.localPosition = position;
        blade.transform.localRotation = rotation;
        sawBlades[bladeIndex].gameObject.SetActive(true);
    }
}

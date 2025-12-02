using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    private AbilitiesEnum powerUpType;

    public AbilitiesEnum PowerUpType {get {return powerUpType;}}

    [SerializeField]
    private int initialAmmoAmount;
    public int IntitaliAmmoAmount {get {return initialAmmoAmount;}}

}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    [SerializeField]
    private AbilitiesEnum powerUpType;

    public AbilitiesEnum PowerUpType {get {return powerUpType;}}

    [SerializeField]
    private int initialAmmoAmount;
    public int IntitaliAmmoAmount {get {return initialAmmoAmount;}}

}

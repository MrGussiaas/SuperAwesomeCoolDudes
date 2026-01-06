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

    private SpriteRenderer sr;

    private Coroutine startCountDown;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void InitializePowerUP()
    {
        if(startCountDown != null)
        {
            StopCoroutine(startCountDown);
        }
        startCountDown = StartCoroutine(StartCountDown());
    }

    public void CancelPowerUp()
    {
        if(startCountDown != null)
        {
            StopCoroutine(startCountDown);
        }
    }

    private IEnumerator StartCountDown()
    {
        Color opaqueColor = sr.color;
        Color transparentColor = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        yield return new WaitForSeconds(2.5f);
        float elapsedTime = 0;
        int ctr = 0;
        while (elapsedTime <= 2.5f)
        {
            elapsedTime += Time.deltaTime;
            ctr++;
            sr.color = ctr % 2 == 0 ? opaqueColor : transparentColor;
            yield return null;
        }
        sr.color = opaqueColor;
        GameEvents.PowerUpRemovedFromRoom(this);
    }



}

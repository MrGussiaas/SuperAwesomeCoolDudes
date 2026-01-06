using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerStates : NetworkBehaviour
{
    private bool isInvincible = false;
    private SpriteRenderer sr;

    private const float I_FRAME_TIME = 1f;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool IsInvincible {
        get {return isInvincible;} 
        set {
                isInvincible = value;
                if(isInvincible){
                    StartCoroutine(StartInvincibleCoRoutine());
                    StartClientBlink();
                }
            }
        }

    private IEnumerator StartInvincibleCoRoutine()
    {
        yield return new WaitForSeconds(I_FRAME_TIME);
        isInvincible = false;
    }

    private IEnumerator DoInvincibleBlink()
    {
        float elapsedTime = 0;
        Color originalColor = sr.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        Color opaqueColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        int counter = 0;
        while(elapsedTime <= I_FRAME_TIME)
        {
            sr.color = counter % 2 == 0 ? transparentColor : opaqueColor;
            counter ++;
            elapsedTime += Time.deltaTime;
            yield return null;

        }
        sr.color = opaqueColor;
        
    }
    [ClientRpc]
    private void StartClientBlink()
    {
       StartCoroutine(DoInvincibleBlink()); 
    }
}

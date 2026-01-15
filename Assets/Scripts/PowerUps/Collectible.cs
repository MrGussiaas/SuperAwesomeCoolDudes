using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Collectible : NetworkBehaviour
{
    [SerializeField]
    private CollectibleEnum collectibleType;
    public CollectibleEnum CollectibleType {get{return collectibleType;}}

    private SpriteRenderer sr;

    private WaitForSeconds waitForSecondsSem;

    private const int MASTER_DELAY = 5;

    public void Awake()
    {
        InitVars();
    }

    private void InitVars()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    [ClientRpc]
    private void RpcStartBlinking(float activeSeconds, float waitForSecondsSemm)
    {
        StartCoroutine(StartCountDown(activeSeconds, waitForSecondsSemm));
    }

    public void InitializeCollectible()
    {
        float activeSeconds = MASTER_DELAY + Random.value;
        waitForSecondsSem = new WaitForSeconds(activeSeconds / 2);
        Debug.Log("Initializing the collectible isServer: " + isServer);
        // server runs coroutine locally
        if (isServer)
        {
            StartCoroutine(StartCountDown(activeSeconds, activeSeconds / 2));
            RpcStartBlinking(activeSeconds, activeSeconds / 2);  // tell clients to do the same
        }
    }

    private IEnumerator StartCountDown(float secondsActive, float  waitForSecondsSemm)
    {
        Color opaqueColor = sr.color;
        Color transparentColor = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        float halfSeconds = secondsActive;
        yield return new WaitForSeconds(waitForSecondsSemm);
        float elapsedTime = 0;
        int ctr = 0;
        while (elapsedTime <= halfSeconds)
        {
            elapsedTime += Time.deltaTime;
            ctr++;
            sr.color = ctr % 2 == 0 ? opaqueColor : transparentColor;
            yield return null;
        }
        sr.color = opaqueColor;
        GameEvents.CollectibleRemovedFromRoom(this);
    }
}

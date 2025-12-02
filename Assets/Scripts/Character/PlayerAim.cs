using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerAim : NetworkBehaviour
{

    private GameObject gun;

    private const string GUN = "Gun";

    private InputHandler inputHandler;

    // Start is called before the first frame update
    void Awake()
    {
        initProperties();
    }
    
    private void initProperties()
    {
        inputHandler = GetComponent<InputHandler>();
        for(int i = 0, n = transform.childCount; i < n; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.CompareTag(GUN))
            {
                gun = child;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        Vector3 mousePos = inputHandler.MouseWorldPosition;

        Vector3 direction = mousePos - gun.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        gun.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEnemyTank : VisualEnemyBasicSlow
{

    private const string TURRET = "Turret";

    private VisualEnemyTurret turret;

    private void initVars()
    {
        for(int i = 0, n = transform.childCount; i < n; i++)
        {
            GameObject gObj = transform.GetChild(i).gameObject;
            if (gObj.CompareTag(TURRET))
            {
                turret = gObj.GetComponent<VisualEnemyTurret>();
            }
        }
    }

    private void Awake()
    {
        initVars();
    }

    public override void StartAim(Vector3 dir)
    {
        Debug.Log("Starting aim");
        base.StartAim(dir);
        turret.BeginAim(dir);
    }

    public override void FinishAim(Vector3 dir)
    {
        Debug.Log("Finish aim here");
        base.FinishAim(dir);
        turret.CompleteAim(dir);
    }
}

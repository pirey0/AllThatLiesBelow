using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingLog : MineableObject
{

    [SerializeField] string statement;

    [Zenject.Inject] ProgressionHandler progressionHandler;
    [Zenject.Inject] PlayerStatementsHandler playerStatements;

    static float lastStatementCooldown = 0; //Dirty

    public override void Damage(float v)
    {
        if (progressionHandler.CurrentDay == 0)
        {
            if (Time.time - lastStatementCooldown > 6)
            {
                playerStatements.Say(statement, 5);
                lastStatementCooldown = Time.time;
            }
        }
        else
        {
            base.Damage(v);
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLauncher : Weapon, IPlayerWeapon
{
    public override void Tick()
    {
        Vector3 mousePosition = Helper.MouseData().position;
        Aim(mousePosition);
        base.Tick();
    }

    public void TryTriggerPrimaryAttack(float triggerTime)
    {
        RequestPrimaryAttack(triggerTime);
    }

    public void TryTriggerSecondaryAttack(float triggerTime)
    {
        RequestSecondaryAttack(triggerTime);
    }
}

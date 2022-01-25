using UnityEngine;

public interface IPlayerWeapon
{
    void Tick();
    void TryTriggerPrimaryAttack(float attackTime);
    void TryTriggerSecondaryAttack(float attackTime);
}

using System;
using UnityEngine;

public class Weapon: MonoBehaviour
{
    [SerializeField] private GameObject target;

    [Tooltip("If true, player can shoot both primary and secondary attack at the same time. Totally untested")]
    [SerializeField] private bool canShootBoth;

    [Header("Primary Attack")]
    [SerializeField] private ProjectilePattern primaryAttackPattern;
    [SerializeField] private ProjectileType primaryAttackProjectileType;
    [SerializeField] private FiringMode primaryAttackFiringMode;
    private bool executePrimaryAttack;

    [Header("Secondary Attack")]
    [SerializeField] private ProjectilePattern secondaryAttackPattern;
    [SerializeField] private ProjectileType secondaryAttackProjectileType;
    [SerializeField] private FiringMode secondaryAttackFiringMode;
    private bool executeSecondaryAttack;

    public virtual void Tick()
    {
        bool exclusivePrimary = executePrimaryAttack && !executeSecondaryAttack;
        bool exclusiveSecondary = executeSecondaryAttack && !executePrimaryAttack;

        if (!canShootBoth && (executeSecondaryAttack && executePrimaryAttack))
        {
            secondaryAttackFiringMode.IsSequenceDone(true);
            primaryAttackFiringMode.IsSequenceDone(true);
            executePrimaryAttack = false;
            executeSecondaryAttack = false;
            return;
        }
        
        if (exclusivePrimary)
        {
            if (primaryAttackFiringMode.CanShoot())
                PrimaryAttack();
            executePrimaryAttack = !primaryAttackFiringMode.IsSequenceDone();
        }

        if (exclusiveSecondary)
        {
            if (secondaryAttackFiringMode.CanShoot())
                SecondaryAttack();
            executeSecondaryAttack = !secondaryAttackFiringMode.IsSequenceDone();
        }
    }

    public void RequestPrimaryAttack(float triggerTime = 0.0f)
    {
        executePrimaryAttack = primaryAttackFiringMode.CanStartSequence(triggerTime);
    }

    public void RequestSecondaryAttack(float triggerTime = 0.0f)
    {
        executeSecondaryAttack = secondaryAttackFiringMode.CanStartSequence(triggerTime);
    }

    public virtual void PrimaryAttack()
    {
        primaryAttackPattern.Execute(this.transform, primaryAttackProjectileType.ProjectileId, target);
    }

    public virtual void SecondaryAttack()
    {
        secondaryAttackPattern.Execute(this.transform, secondaryAttackProjectileType.ProjectileId, target);
    }

    protected void Aim(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetTarget(GameObject target) => this.target = target;
    public float GetPrimaryRof => primaryAttackFiringMode.rateOfFire;
}

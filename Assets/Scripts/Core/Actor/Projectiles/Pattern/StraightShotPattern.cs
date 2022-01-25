using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightShotPattern : ProjectilePattern
{
    [SerializeField] private float projectileSpeedBonus = 20.0f;

    public override void Execute(Transform origin, int projectileTypeId, GameObject target = null)
    {
        GameObject spawnedProjectile = ProjectilePool.Instance.GetPooledObject(projectileTypeId);
        LaunchProjectile(origin.transform.position, origin.right * projectileSpeedBonus, spawnedProjectile);
    }
}

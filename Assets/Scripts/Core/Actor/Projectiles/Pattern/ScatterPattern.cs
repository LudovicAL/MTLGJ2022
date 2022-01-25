using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterPattern : ProjectilePattern
{
    [Range(2, 360)]
    [SerializeField] private int scatterShotCount = 2;
    [Range(5.0f, 180.0f)]
    [SerializeField] private float scatterShotAngle = 45.0f;
    [SerializeField] private float projectileSpeedBonus = 20.0f;

    public override void Execute(Transform origin, int projectileTypeId, GameObject target = null)
    {
        float rad = scatterShotAngle * Mathf.Deg2Rad;
        float radShot = rad * 2 / (scatterShotCount - 1);

        GameObject[] spawnedProjectiles = ProjectilePool.Instance.GetPooledObject(projectileTypeId, scatterShotCount);

        for (int i = 0; i < scatterShotCount; i++)
        {
            float currentRad = rad - radShot * i;
            Vector3 offset = new Vector3(Mathf.Cos(currentRad), Mathf.Sin(currentRad), 0.0f);


            Vector3 direction = (origin.right * offset.x + origin.up * offset.y) * projectileSpeedBonus;

            if (i < spawnedProjectiles.Length)
                LaunchProjectile(origin.position, direction, spawnedProjectiles[i]);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiperPattern : ProjectilePattern
{
    [Range(2, 360)]
    [SerializeField] private int wiperIncrement = 40;
    [Range(5.0f, 180.0f)]
    [SerializeField] private float wiperMaxAngle = 45.0f;
    [SerializeField] private float projectileSpeedBonus = 20.0f;

    private int currentIncrement;

    public override void Execute(Transform origin, int projectileTypeId, GameObject target = null)
    {
        //WIP do not use;

        float rad = wiperMaxAngle * Mathf.Deg2Rad;
        float radShot = rad * 2 / (wiperIncrement - 1);

        GameObject spawnedProjectile = ProjectilePool.Instance.GetPooledObject(projectileTypeId);

        float currentRad = radShot * currentIncrement;
        Vector3 offset = new Vector3(Mathf.Cos(currentRad), Mathf.Sin(currentRad), 0.0f);

        currentIncrement = (currentRad >= rad) ? currentIncrement - 1 : currentIncrement + 1;
        Vector3 direction = (origin.right * offset.x + origin.up * offset.y) * projectileSpeedBonus;
        LaunchProjectile(origin.position, direction, spawnedProjectile);
        
        /*for (int i = 0; i < scatterShotCount; i++)
        {
            float currentRad = rad - radShot * i;
            Vector3 offset = new Vector3(Mathf.Cos(currentRad), Mathf.Sin(currentRad), 0.0f);


            Vector3 direction = (origin.right * offset.x + origin.up * offset.y) * projectileSpeedBonus;

            if (i < spawnedProjectiles.Length)
                LaunchProjectile(origin.position, direction, spawnedProjectiles[i]);
        }*/
    }
}

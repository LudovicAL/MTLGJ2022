using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletType : ProjectileType
{
    private int projectileId = -1;
    public override int ProjectileId {
        get {
            if (projectileId == -1)
                return this.gameObject.GetInstanceID();
            else
                return projectileId;
        }
        set {
            projectileId = value;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Sorry I was doing this somewhere else! :D CHRISC
        //CleanUp();
        //SpawnEffect();
    }

    private void CleanUp()
    {
        this.transform.gameObject.SetActive(false);
    }

    private void SpawnEffect()
    {

    }
}

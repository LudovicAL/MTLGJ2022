using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipWeaponSystem : MonoBehaviour
{
    private SpaceShipAI _spaceShipAI;
    private Rigidbody2D _body;
    private Weapon weapon;
    private float lastShotTime;

    private void Awake()
    {
        _spaceShipAI = GetComponentInParent<SpaceShipAI>();
        _body = GetComponentInParent<Rigidbody2D>();
        weapon = GetComponentInChildren<Weapon>();
        weapon.SetTarget(GameObject.FindGameObjectWithTag("Player"));
    }

    private void Update()
    {
        //Just call primary attack when your condition is met
        if (_spaceShipAI.CanShoot() && Time.time > lastShotTime + (1 / (weapon.GetPrimaryRof / 60.0f)))
        {
            lastShotTime = Time.time;
            weapon.PrimaryAttack();
            _body.AddForce(-_spaceShipAI._aimDirection * 5, ForceMode2D.Impulse);
        }
            
    }
}

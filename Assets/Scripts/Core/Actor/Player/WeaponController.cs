using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : Singleton<WeaponController>
{
    public List<GameObject> availableWeapons = new List<GameObject>();

    [SerializeField] private WeaponUI weaponUI; 
    private IPlayerWeapon currentWeapon;

    public void Awake()
    {
        weaponUI = FindObjectOfType<WeaponUI>();

        if (availableWeapons.Count > 0)
        {
            currentWeapon = availableWeapons[0].GetComponentInChildren<IPlayerWeapon>();
            for (int i = 0; i < availableWeapons.Count; i++)
                weaponUI.AddWeaponToUI(i);

            weaponUI.ChangeActiveWeapon(0);
        }
    }

    private void OnEnable()
    {
        weaponUI.activeWeaponChanged += OnActiveWeaponChanged;
    }

    private void OnDestroy()
    {
        weaponUI.activeWeaponChanged -= OnActiveWeaponChanged;
    }

    private void Update()
    {
        currentWeapon?.Tick();
    }

    private void OnActiveWeaponChanged(int i)
    {
        currentWeapon = availableWeapons[i].GetComponentInChildren<IPlayerWeapon>();
    }

    public void OnPrimaryFire(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
            currentWeapon?.TryTriggerPrimaryAttack((float)callbackContext.time);
    }

    public void OnSecondaryFire(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
            currentWeapon?.TryTriggerSecondaryAttack((float)callbackContext.time);
    }

    public void OnPickUpAddToWeapon(GameObject newWeapon)
    {
        availableWeapons.Add(newWeapon);
        weaponUI.AddWeaponToUI(availableWeapons.Count - 1);
    }
}

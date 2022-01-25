using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipGun : MonoBehaviour
{
    private SpaceShipAI _shipAI;

    // Start is called before the first frame update
    void Start()
    {
        _shipAI = transform.parent.GetComponent<SpaceShipAI>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = Mathf.Atan2(_shipAI._aimDirection.y, _shipAI._aimDirection.x);
        transform.localRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }
}

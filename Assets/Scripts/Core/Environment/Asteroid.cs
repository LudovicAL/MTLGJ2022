using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    Camera _mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPoint = _mainCamera.WorldToViewportPoint(transform.position);

        if (screenPoint.x < 0 || screenPoint.y < 0 || screenPoint.y > 1)
            Destroy(gameObject);
    }
}

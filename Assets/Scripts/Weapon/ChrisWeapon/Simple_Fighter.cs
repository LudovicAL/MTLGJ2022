using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;

public class Simple_Fighter : MonoBehaviour
{
    public GameObject m_MissilePrefab;

    private int m_MissileCount = 0;
    private int m_CheapTimer = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ++m_CheapTimer;

        if (m_CheapTimer >= 5)
        {
            m_CheapTimer = 0;
            if (m_MissileCount < 30)
            {
                Instantiate(m_MissilePrefab, transform.position, Quaternion.identity);
                ++m_MissileCount;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : Singleton<ProjectilePool>
{
    public Dictionary<int, List<GameObject>> pooledProjectiles = new Dictionary<int, List<GameObject>>();
    [SerializeField] private int amountToPool;
    public GameObject[] _availableProjectiles;

    private List<(int, int)> activeProjectiles = new List<(int, int)>();
    private List<GameObject> buffer = new List<GameObject>();

    private float lastCleanupTime;
    private float cleanupDelay = 1.0f;

    void Awake()
    {
        _availableProjectiles = Resources.LoadAll<GameObject>("Prefab/Projectiles");
    }

    private void Start()
    {
        foreach (GameObject availableProjectile in _availableProjectiles)
        {
            int instanceID = availableProjectile.GetInstanceID();
            pooledProjectiles.Add(instanceID, new List<GameObject>());

            GameObject tmp;
            for (int i = 0; i < amountToPool; i++)
            {
                tmp = Instantiate(availableProjectile, this.transform);
                tmp.SetActive(false);
                pooledProjectiles[instanceID].Add(tmp);
            }
        }
    }

    private void Update()
    {
        if (Time.time > lastCleanupTime + cleanupDelay) { 
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                (int projectileId, int index) = activeProjectiles[i];

                GameObject pooledpart = pooledProjectiles[projectileId][index];
                if (pooledpart.gameObject.activeInHierarchy)
                {
                    Vector3 screenPoint = Camera.main.WorldToViewportPoint(pooledpart.transform.position);

                    if (screenPoint.x > 1 || screenPoint.x < 0 || screenPoint.y < 0 || screenPoint.y > 1)
                    {
                        pooledpart.gameObject.SetActive(false);
                        activeProjectiles.RemoveAt(i);
                    }
                } else
                {
                    activeProjectiles.RemoveAt(i);
                }
            }
        }
    }

    public GameObject[] GetPooledObject(int partId, int amount)
    {
        if (partId <= 0)
            return null;

        int projectileFound = 0;
        buffer.Clear();

        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledProjectiles[partId][i].activeInHierarchy)
            {
                activeProjectiles.Add((partId, i));
                buffer.Add(pooledProjectiles[partId][i]);
                projectileFound++;
            }

            if (projectileFound == amount)
                return buffer.ToArray();
        }
        return buffer.ToArray();
    }

    public GameObject GetPooledObject(int partId)
    {
        if (partId <= 0)
            return null;

        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledProjectiles[partId][i].activeInHierarchy)
            {
                activeProjectiles.Add((partId, i));
                return pooledProjectiles[partId][i];
            }
        }
        
        return null;
    }
}

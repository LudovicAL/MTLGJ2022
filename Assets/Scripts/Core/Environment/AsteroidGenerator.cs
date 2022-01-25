using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGenerator : MonoBehaviour
{
    public GameObject _asteroidPrefab;
    public float _minVelocity = 5f;
    public float _maxVelocity = 20f;
    public int _angleRange = 30;
    public float _maxScale = 3f;
    public float _minTimeBetweenSpawns = 1f;
    public float _maxTimeBetweenSpawns = 3f;



    private float _timeBeforeNextSpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _timeBeforeNextSpawn -= Time.deltaTime;

        if (_timeBeforeNextSpawn <= 0f)
        {
            _timeBeforeNextSpawn = Random.Range(_minTimeBetweenSpawns, _maxTimeBetweenSpawns);
            GameObject asteroid = GameObject.Instantiate(_asteroidPrefab, transform.position, Quaternion.identity);
            asteroid.transform.localScale *= Random.Range(1, _maxScale);
            Rigidbody2D body = asteroid.GetComponent<Rigidbody2D>();
            Vector3 force = Vector3.left * Random.Range(_minVelocity, _maxVelocity);
            force = Quaternion.Euler(0, 0, Random.Range(-_angleRange, _angleRange)) * force;
            body.AddForce(force);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    public Transform playerTransform;
    public List<GameObject> asteroidPrefabs;
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 50f;
    public float checkInterval = 1f;
    public int maxAsteroids = 20;

    private List<GameObject> activeAsteroids = new List<GameObject>();

    void Start()
    {
        for(int i = 0; i < maxAsteroids; i++)
        {
            SpawnAsteroid();
        }
        InvokeRepeating("SpawnAsteroid", 0, checkInterval);
        InvokeRepeating("CheckAsteroids", 0, checkInterval);
    }

    void SpawnAsteroid()
    {
        if (activeAsteroids.Count >= maxAsteroids)
            return;

        Vector3 spawnPosition = Random.onUnitSphere * (Random.Range(minSpawnDistance, maxSpawnDistance)) + playerTransform.position;
        GameObject asteroidPrefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)];
        GameObject newAsteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
        activeAsteroids.Add(newAsteroid);
    }



    void CheckAsteroids()
    {
        List<GameObject> asteroidsToRemove = new List<GameObject>();
        foreach (GameObject asteroid in activeAsteroids)
        {
            if (Vector3.Distance(playerTransform.position, asteroid.transform.position) > maxSpawnDistance)
            {
                asteroidsToRemove.Add(asteroid);
            }
        }

        foreach (GameObject asteroid in asteroidsToRemove)
        {
            activeAsteroids.Remove(asteroid);
            Destroy(asteroid);
        }
    }
}

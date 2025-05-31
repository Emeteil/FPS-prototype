using UnityEngine;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public GameObject prefab;
        public int minSpawnCount;
        public int maxSpawnCount;
    }

    [SerializeField] private List<SpawnablePrefab> spawnablePrefabs = new List<SpawnablePrefab>();
    [SerializeField] private Vector3 spawnArea = new Vector3(10, 10, 10);
    [SerializeField] private float simulationTime = 3f;
    [SerializeField] private float simulationStep = 0.05f;
    [SerializeField] private bool physicsSimulation = false;

    private void Start()
    {
        SpawnPrefabs();
        
        if (physicsSimulation)
            PhysicsSimulation(simulationTime);
    }

    private void SpawnPrefabs()
    {
        foreach (var spawnable in spawnablePrefabs)
        {
            int spawnCount = Random.Range(spawnable.minSpawnCount, spawnable.maxSpawnCount + 1);
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition = new Vector3(
                    Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                    Random.Range(-spawnArea.y / 2, spawnArea.y / 2),
                    Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
                );

                Quaternion spawnRotation = Random.rotation;
                Instantiate(spawnable.prefab, spawnPosition, spawnRotation);
            }
        }
    }

    private void PhysicsSimulation(float _timeToSimulate)
    {
        if (_timeToSimulate < simulationStep) return;

        Physics.autoSimulation = false;

        float simulatedTime = 0f;

        while (simulatedTime < _timeToSimulate)
        {
            Physics.Simulate(simulationStep);
            simulatedTime += simulationStep;
        }

        Physics.autoSimulation = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}
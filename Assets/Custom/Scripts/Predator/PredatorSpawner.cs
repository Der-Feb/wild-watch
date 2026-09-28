using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredatorSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject predatorPrefab;
    public int maxPredators = 3;
    public float spawnInterval = 3f;
    public float spawnCheckRadius = 2f;

    [Header("Detection")]
    public LayerMask predatorLayer; // set to "predators"

    readonly List<GameObject> active = new List<GameObject>();
    float timer;

    void Update()
    {
        active.RemoveAll(p => p == null);
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;
        TrySpawn();
    }

    void TrySpawn()
    {
        if (active.Count >= maxPredators) return;
        if (Prey.All.Count == 0) return;
        if (Physics.CheckSphere(transform.position, spawnCheckRadius, predatorLayer)) return;

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning("[Spawner] Spawn point is not on the NavMesh.");
            return;
        }

        active.Add(Instantiate(predatorPrefab, hit.position, transform.rotation));
        Debug.Log($"[Spawner] Spawned {predatorPrefab.name} {active.Count}/{maxPredators}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnCheckRadius);
    }
}
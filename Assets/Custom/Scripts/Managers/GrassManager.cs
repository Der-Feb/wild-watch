using UnityEngine;
using System.Collections.Generic;

public class GrassManager : MonoBehaviour
{
    public static GrassManager Instance;

    [Header("References")]
    public Transform player;

    [Header("Settings")]
    public float visibleRadius = 100f;
    public float checkInterval = 0.25f; // how often to re-check distances (seconds)

    private List<Billboard> allGrass = new List<Billboard>();
    private float timer;
    private float sqrRadius;

    void Awake()
    {
        Instance = this;
        sqrRadius = visibleRadius * visibleRadius;
    }

    public void Register(Billboard grass)
    {
        allGrass.Add(grass);
    }

    public void Unregister(Billboard grass)
    {
        allGrass.Remove(grass);
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        Vector3 playerPos = player.position;

        foreach (var grass in allGrass)
        {
            if (grass == null) continue;

            float sqrDist = (grass.transform.position - playerPos).sqrMagnitude;
            bool shouldBeActive = sqrDist <= sqrRadius;

            if (grass.IsActive != shouldBeActive)
            {
                grass.SetActive(shouldBeActive);
            }
        }
    }
}
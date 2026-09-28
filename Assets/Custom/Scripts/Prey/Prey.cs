using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prey : MonoBehaviour
{
    public static readonly List<Prey> All = new List<Prey>();

    [Header("Identity")]
    public Species species = Species.Rabbit;

    [Header("Death — Knockback")]
    public float knockbackDistance = 1.5f;
    public float knockbackDuration = 0.25f;

    [Header("Death — Visuals")]
    public GameObject deathVfxPrefab;
    public float deathVfxLifetime = 3f;
    public float deathHoldTime = 1.5f;

    public bool IsDead { get; private set; }
    public PredatorAI Hunter { get; set; }
    public event Action<Prey> Killed;

    Animator animator;
    string displayNameCache;
    public string DisplayName => displayNameCache;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    void Awake()
    {
        var info = GetComponent<Interactable>();
        displayNameCache = info != null ? info.DisplayName : name;
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    public void Kill(Vector3 knockbackDirection = default)
    {
        if (IsDead) return;
        IsDead = true;
        All.Remove(this);
        Killed?.Invoke(this);
        StartCoroutine(DeathSequence(knockbackDirection));
    }

    IEnumerator DeathSequence(Vector3 knockbackDirection)
    {
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        if (HasParameter("isDead"))
            animator.SetBool("isDead", true);

        knockbackDirection.y = 0f;
        if (knockbackDirection.sqrMagnitude > 0.01f && knockbackDistance > 0f)
        {
            knockbackDirection.Normalize();
            Vector3 start = transform.position;
            Vector3 desired = start + knockbackDirection * knockbackDistance;

            Vector3 end = NavMesh.SamplePosition(desired, out NavMeshHit hit, knockbackDistance + 0.5f, NavMesh.AllAreas)
                ? hit.position
                : start;

            float t = 0f;
            while (t < knockbackDuration)
            {
                t += Time.deltaTime;
                float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / knockbackDuration), 2f);
                transform.position = Vector3.Lerp(start, end, eased);
                yield return null;
            }
        }

        if (deathVfxPrefab != null)
        {
            GameObject vfx = Instantiate(deathVfxPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);
            Destroy(vfx, deathVfxLifetime);
        }

        yield return new WaitForSeconds(deathHoldTime);
        Destroy(gameObject);
    }

    bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }
}
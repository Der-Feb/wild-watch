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

    [Header("Death Sequence")]
    public float hitStunDuration = 0.5f;
    public float deathHoldTime = 1.5f;

    [Header("Death — Visuals")]
    public GameObject deathVfxPrefab;
    public float deathVfxLifetime = 3f;

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

    /// <param name="attackerPosition">World position of whatever hit us — used only to
    /// orient the death reaction, never to move this object.</param>
    public void Kill(Vector3 attackerPosition = default)
    {
        if (IsDead) return;
        IsDead = true;
        All.Remove(this);
        Killed?.Invoke(this);
        StartCoroutine(DeathSequence(attackerPosition));
    }

    IEnumerator DeathSequence(Vector3 attackerPosition)
    {
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            agent.enabled = false;
        }

        // Face away from whatever hit us, so a single stumble/fall clip reads correctly
        // regardless of which direction the attack came from. Rotation only — no translation.
        float hitAngle = 0f; // 0 = hit from front, +90 = from the right, -90 = from the left, ±180 = from behind
        if (attackerPosition != default)
        {
            Vector3 toAttacker = attackerPosition - transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude > 0.001f)
            {
                toAttacker.Normalize();
                hitAngle = Vector3.SignedAngle(transform.forward, toAttacker, Vector3.up);
                transform.rotation = Quaternion.LookRotation(-toAttacker); // face away from attacker
            }
        }

        if (HasParameter("hitAngle"))
            animator.SetFloat("hitAngle", hitAngle); // available for Option B below, harmless if unused

        if (HasParameter("getHit"))
            animator.SetTrigger("getHit");

        if (deathVfxPrefab != null)
        {
            GameObject vfx = Instantiate(deathVfxPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);
            Destroy(vfx, deathVfxLifetime);
        }

        yield return new WaitForSeconds(hitStunDuration);

        if (HasParameter("isDead"))
            animator.SetBool("isDead", true);

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
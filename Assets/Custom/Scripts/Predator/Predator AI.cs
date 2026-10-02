using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PredatorAI : MonoBehaviour
{
    [Header("Diet")]
    public Species[] diet = { Species.Rabbit };

    [Header("Detection & Attack")]
    public float detectionRadius = 30f;
    public bool huntAnywhere = false;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float hitDelay = 0.4f;
    public float recoverTime = 0.8f;
    public float scanInterval = 0.5f;
    public float repathInterval = 0.1f;

    [Header("Approach (slows from chase to walk before biting)")]
    public float approachRange = 6f;
    public float approachExitRange = 8f; // must move this far away again to resume full chase speed
    [Range(0.1f, 1f)] public float approachSpeedMultiplier = 0.4f;

    [Header("Closing In (ignore agent-vs-agent avoidance so the bite can land)")]
    public float noAvoidanceRange = 3f;

    [Header("Rest / Tired")]
    public float restAfterKill = 2.5f;
    public float noPreyRecheckInterval = 2f;

    [Header("Animation Smoothing")]
    public float speedDampTime = 0.15f;

    [Header("Debug")]
    public bool debugLogs = true;
    public float debugLogInterval = 3f;

    [Header("References")]
    public Animator animator;

    protected NavMeshAgent agent;
    protected Prey target;
    protected float huntSpeed = 4f; // set by subclass in OnTargetAcquired

    Vector3 homePosition;
    ObstacleAvoidanceType defaultAvoidance;

    float lastAttackTime = -999f;
    float scanTimer;
    float repathTimer;
    float patrolWaitTimer;
    float nextDebugLog;
    float nextNearLog;
    bool patrolWaiting;
    bool isAttacking;
    bool isTired;
    bool restCoroutineRunning;
    bool isApproaching;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        defaultAvoidance = agent.obstacleAvoidanceType;
        agent.stoppingDistance = attackRange * 0.5f;
        agent.autoBraking = false;
        agent.acceleration = Mathf.Max(agent.acceleration, 20f);
    }

    protected virtual void Start()
    {
        homePosition = transform.position;
    }

    protected virtual void Update()
    {
        if (animator != null)
            animator.SetFloat("speed", agent.velocity.magnitude, speedDampTime, Time.deltaTime);

        if (isAttacking || isTired) return;
        if (!agent.isOnNavMesh) return;

        if (target == null || target.IsDead)
        {
            ReleaseTarget();

            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f)
            {
                scanTimer = scanInterval;
                AcquireTarget();
            }

            if (target == null)
            {
                if (!AnyPreyOfDietAlive())
                {
                    if (!restCoroutineRunning) StartCoroutine(RestUntilPreyReturns());
                }
                else
                {
                    Patrol();
                }
            }
            return;
        }

        float flat = FlatDistance(target.transform.position);

        if (flat <= approachRange) isApproaching = true;
        else if (flat >= approachExitRange) isApproaching = false;
        agent.speed = isApproaching ? huntSpeed * approachSpeedMultiplier : huntSpeed;

        agent.obstacleAvoidanceType = flat <= noAvoidanceRange
            ? ObstacleAvoidanceType.NoObstacleAvoidance
            : defaultAvoidance;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(target.transform.position);
        }

        bool inRange = flat <= attackRange;
        bool arrived = !agent.pathPending &&
                       agent.remainingDistance <= agent.stoppingDistance + 0.3f &&
                       flat <= attackRange * 1.5f;
        bool ready = Time.time >= lastAttackTime + attackCooldown;

        if ((inRange || arrived) && ready)
        {
            StartCoroutine(Attack());
        }
        else if (debugLogs && flat <= attackRange * 2.5f && Time.time >= nextNearLog)
        {
            nextNearLog = Time.time + 1f;
            Debug.Log($"[{name}] Near {target.DisplayName}: dist {flat:F2}m, remaining {agent.remainingDistance:F2}, " +
                      $"avoidance {agent.obstacleAvoidanceType}, speed {agent.speed:F2}, cooldownReady {ready}");
        }
    }

    bool AnyPreyOfDietAlive()
    {
        float maxSqr = huntAnywhere ? float.MaxValue : detectionRadius * detectionRadius;

        foreach (Prey p in Prey.All)
        {
            if (p.IsDead) continue;
            if (System.Array.IndexOf(diet, p.species) < 0) continue;

            float sqr = (p.transform.position - transform.position).sqrMagnitude;
            if (sqr <= maxSqr) return true;
        }
        return false;
    }

    IEnumerator RestUntilPreyReturns()
    {
        restCoroutineRunning = true;
        SetTired(true);
        agent.isStopped = true;
        agent.ResetPath();

        if (debugLogs)
            Debug.Log($"[{name}] No {string.Join("/", diet)} left in range. Resting.");

        while (!AnyPreyOfDietAlive())
            yield return new WaitForSeconds(noPreyRecheckInterval);

        if (debugLogs)
            Debug.Log($"[{name}] Prey available again. Waking up.");

        SetTired(false);
        agent.isStopped = false;
        restCoroutineRunning = false;
    }

    void SetTired(bool value)
    {
        isTired = value;
        if (animator != null)
        {
            animator.SetBool("isTired", value);
            if (debugLogs) Debug.Log($"[{name}] isTired set to {value}");
        }
    }

    // ---------- Overridable hooks ----------
    protected virtual bool IsValidPrey(Prey p)
    {
        if (p.IsDead) return false;
        if (p.Hunter != null && p.Hunter != this) return false;
        return System.Array.IndexOf(diet, p.species) >= 0;
    }

    protected virtual float ScoreTarget(Prey p, float sqrDist) => sqrDist;
    protected virtual void OnTargetAcquired(Prey p) { }
    protected virtual void OnTargetLost() { }
    protected virtual void OnKill(Prey p) { }
    // ---------------------------------------

    float FlatDistance(Vector3 point)
    {
        Vector3 d = point - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    void AcquireTarget()
    {
        float maxSqr = huntAnywhere ? float.MaxValue : detectionRadius * detectionRadius;
        float bestScore = float.MaxValue;
        Prey best = null;

        int total = Prey.All.Count, inDiet = 0, available = 0;
        float nearestAvailSqr = float.MaxValue;

        foreach (Prey p in Prey.All)
        {
            if (p.IsDead) continue;
            if (System.Array.IndexOf(diet, p.species) < 0) continue;
            inDiet++;

            if (!IsValidPrey(p)) continue;
            available++;

            float sqr = (p.transform.position - transform.position).sqrMagnitude;
            if (sqr < nearestAvailSqr) nearestAvailSqr = sqr;
            if (sqr > maxSqr) continue;

            float score = ScoreTarget(p, sqr);
            if (score < bestScore) { bestScore = score; best = p; }
        }

        if (best == null)
        {
            if (debugLogs && Time.time >= nextDebugLog)
            {
                nextDebugLog = Time.time + debugLogInterval;
                string nearest = available > 0 ? $"{Mathf.Sqrt(nearestAvailSqr):F1}m" : "n/a";
                Debug.Log($"[{name}] No target. registered: {total}, matching diet: {inDiet}, " +
                          $"available: {available}, nearest available: {nearest}.");
            }
            return;
        }

        target = best;
        target.Hunter = this;
        patrolWaiting = false;
        isApproaching = false;
        agent.isStopped = false;
        OnTargetAcquired(best);

        if (debugLogs)
            Debug.Log($"[{name}] Hunting {best.DisplayName} ({Mathf.Sqrt((best.transform.position - transform.position).sqrMagnitude):F1}m away)");
    }

    void Patrol()
    {
        if (patrolWaiting)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                patrolWaiting = false;
                PickPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && !agent.hasPath)
        {
            PickPatrolPoint();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            patrolWaiting = true;
            patrolWaitTimer = Random.Range(1f, 3f);
            agent.ResetPath();
        }
    }

    void PickPatrolPoint()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 r = Random.insideUnitCircle * 25f;
            Vector3 candidate = homePosition + new Vector3(r.x, 0f, r.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                return;
            }
        }

        patrolWaiting = true;
        patrolWaitTimer = 1f;
    }

    void ReleaseTarget()
    {
        if (target == null) return;
        if (target.Hunter == this) target.Hunter = null;
        target = null;
        isApproaching = false;
        agent.obstacleAvoidanceType = defaultAvoidance;
        OnTargetLost();
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);

        if (animator != null) animator.SetTrigger("attack");
        yield return new WaitForSeconds(hitDelay);

        bool killed = false;
        if (target != null && !target.IsDead &&
            FlatDistance(target.transform.position) <= attackRange * 1.8f)
        {
            Debug.Log($"<color=red>[KILL]</color> {name} killed {target.DisplayName}!");
            Prey victim = target;

            victim.Kill(transform.position);

            OnKill(victim);
            killed = true;
        }
        else if (debugLogs)
        {
            Debug.Log($"[{name}] Bite missed (target moved out of range).");
        }

        ReleaseTarget();
        agent.ResetPath();

        if (killed)
        {
            SetTired(true);
            yield return new WaitForSeconds(restAfterKill);
            SetTired(false);
        }
        else
        {
            yield return new WaitForSeconds(recoverTime);
        }

        agent.isStopped = false;
        isAttacking = false;
    }

    protected virtual void OnDestroy()
    {
        if (target != null && target.Hunter == this) target.Hunter = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;     Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;  Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, noAvoidanceRange);
    }
}
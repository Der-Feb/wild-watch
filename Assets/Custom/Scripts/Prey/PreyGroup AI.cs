using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Prey), typeof(NavMeshAgent))]
public class PreyGroupAI : MonoBehaviour
{
    public enum Role { Leader, Follower }

    [Header("Role")]
    public Role role = Role.Leader;

    [Header("Leader Settings")]
    public float wanderRadius = 5f;
    public Vector3 wanderCenter;
    public float moveSpeed = 1.5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    public float arriveThreshold = 0.4f;

    [Header("Follower Settings")]
    public Transform leader;
    public float followDistance = 1.2f;
    public Vector3 followOffset;
    public float followSpeed = 2f;
    public float stopDistance = 0.3f;
    public float catchUpDistance = 4f;
    public float catchUpMultiplier = 1.6f;
    public float repathInterval = 0.25f;

    [Header("Optional")]
    public Animator animator;

    Prey prey;
    NavMeshAgent agent;
    ObstacleAvoidanceType defaultAvoidance;
    float waitTimer;
    float repathTimer;
    bool waiting;

    void Awake()
    {
        prey = GetComponent<Prey>();
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        defaultAvoidance = agent.obstacleAvoidanceType;
    }

    void OnEnable()  { prey.Killed += HandleKilled; }
    void OnDisable() { prey.Killed -= HandleKilled; }

    void Start()
    {
        if (!agent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        if (role == Role.Leader)
        {
            if (wanderCenter == Vector3.zero) wanderCenter = transform.position;
            agent.speed = moveSpeed;
            PickNewWanderTarget();
        }
        else
        {
            agent.speed = followSpeed;
            if (leader == null) leader = FindLeaderInGroup();

            if (followOffset == Vector3.zero)
            {
                float angle = transform.GetSiblingIndex() * 1.2f;
                followOffset = new Vector3(Mathf.Sin(angle) * 1.2f, 0f, -Mathf.Cos(angle) * 0.5f);
            }
        }
    }

    void Update()
    {
        if (prey.IsDead || !agent.enabled || !agent.isOnNavMesh) return;

        // Stop dodging whatever predator is currently hunting us —
        // two agents both avoiding each other can never close the gap.
        agent.obstacleAvoidanceType = prey.Hunter != null
            ? ObstacleAvoidanceType.NoObstacleAvoidance
            : defaultAvoidance;

        if (role == Role.Leader) UpdateLeader();
        else UpdateFollower();

        if (animator != null)
            animator.SetBool("isMoving", agent.velocity.sqrMagnitude > 0.02f);
    }

    void UpdateLeader()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                PickNewWanderTarget();
            }
            return;
        }

        bool arrived = !agent.pathPending && agent.remainingDistance <= arriveThreshold;
        bool failed  = !agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid;

        if (arrived || failed)
        {
            waiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            agent.ResetPath();
        }
    }

    void PickNewWanderTarget()
    {
        agent.speed = moveSpeed;

        for (int i = 0; i < 6; i++)
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = wanderCenter + new Vector3(r.x, 0f, r.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        waiting = true;
        waitTimer = 1f;
    }

    void UpdateFollower()
    {
        if (leader == null)
        {
            leader = FindLeaderInGroup();
            return;
        }

        Vector3 worldOffset = leader.TransformDirection(followOffset);
        Vector3 desired = leader.position - leader.forward * followDistance + worldOffset;
        float dist = Vector3.Distance(transform.position, desired);

        if (dist <= stopDistance)
        {
            if (agent.hasPath) agent.ResetPath();
            return;
        }

        agent.speed = dist > catchUpDistance ? followSpeed * catchUpMultiplier : followSpeed;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    Transform FindLeaderInGroup()
    {
        if (transform.parent == null) return null;

        foreach (Transform sibling in transform.parent)
        {
            var ai = sibling.GetComponent<PreyGroupAI>();
            var p  = sibling.GetComponent<Prey>();
            if (ai != null && ai != this && ai.role == Role.Leader && p != null && !p.IsDead)
                return sibling;
        }
        return null;
    }

    void HandleKilled(Prey dead)
    {
        if (agent.enabled) { agent.ResetPath(); agent.enabled = false; }

        if (role != Role.Leader || transform.parent == null) return;

        PreyGroupAI successor = null;
        foreach (Transform sibling in transform.parent)
        {
            var ai = sibling.GetComponent<PreyGroupAI>();
            var p  = sibling.GetComponent<Prey>();
            if (ai == null || ai == this || p == null || p.IsDead) continue;
            successor = ai;
            break;
        }
        if (successor == null) return;

        successor.PromoteToLeader(wanderCenter, wanderRadius);

        foreach (Transform sibling in transform.parent)
        {
            var ai = sibling.GetComponent<PreyGroupAI>();
            if (ai != null && ai != this && ai != successor && ai.role == Role.Follower)
                ai.leader = successor.transform;
        }
    }

    public void PromoteToLeader(Vector3 center, float radius)
    {
        role = Role.Leader;
        wanderCenter = center;
        wanderRadius = radius;
        waiting = false;
        PickNewWanderTarget();
    }

    void OnDrawGizmosSelected()
    {
        if (role != Role.Leader) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? wanderCenter : transform.position, wanderRadius);
    }
}
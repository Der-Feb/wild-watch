using UnityEngine;

public class RabbitAI : MonoBehaviour
{
    public enum Role { Leader, Follower }

    [Header("Role")]
    public Role role = Role.Leader;

    [Header("Leader Settings (Big Rabbit)")]
    public float wanderRadius = 5f;
    public Vector3 wanderCenter; // Leave at (0,0,0) to auto-use starting position
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 8f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    public float arriveThreshold = 0.2f;

    [Header("Follower Settings (Small Rabbits)")]
    public Transform leader;              // Drag the Big rabbit here
    public float followDistance = 1.2f;   // How far behind the leader to stay
    public Vector3 followOffset;          // Small per-rabbit offset so they don't stack
    public float followSpeed = 2f;
    public float followRotationSpeed = 8f;
    public float stopDistance = 0.2f;    // Stop moving once this close, to avoid jitter

    [Header("Optional Animator Sync")]
    public Animator animator;             // Optional: drag in to drive an "isMoving" bool

    Vector3 targetPosition;
    float waitTimer;
    bool waiting;

    void Start()
    {
        if (role == Role.Leader)
        {
            if (wanderCenter == Vector3.zero)
                wanderCenter = transform.position;

            PickNewWanderTarget();
        }
        else if (role == Role.Follower)
        {
            if (leader == null)
                leader = FindLeaderInGroup();

            // Automatically assign a fan/flock offset based on hierarchy order
            int index = transform.GetSiblingIndex();
            float angle = index * 1.2f; // Spread them out evenly
            if (followOffset == Vector3.zero)
            {
                followOffset = new Vector3(Mathf.Sin(angle) * 1.2f, 0f, -Mathf.Cos(angle) * 0.5f);
            }
        }
    }

    Transform FindLeaderInGroup()
    {
        if (transform.parent == null) return null;

        foreach (Transform sibling in transform.parent)
        {
            RabbitAI siblingAI = sibling.GetComponent<RabbitAI>();
            if (siblingAI != null && siblingAI.role == Role.Leader)
                return sibling;
        }

        Debug.LogWarning($"{name}: No Leader found among siblings under '{transform.parent.name}'. Assign 'Leader' manually or check group hierarchy.", this);
        return null;
    }

    void Update()
    {
        bool isMoving;

        if (role == Role.Leader)
            isMoving = UpdateLeader();
        else
            isMoving = UpdateFollower();

        if (animator != null)
            animator.SetBool("isMoving", isMoving);
    }

    bool UpdateLeader()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                PickNewWanderTarget();
            }
            return false;
        }

        MoveTowards(targetPosition, moveSpeed, rotationSpeed);

        if (Vector3.Distance(transform.position, targetPosition) <= arriveThreshold)
        {
            waiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            return false;
        }

        return true;
    }

    void PickNewWanderTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        targetPosition = wanderCenter + new Vector3(randomPoint.x, 0f, randomPoint.y);
    }

    bool UpdateFollower()
    {
        if (leader == null) return false;

        // Transform the local offset into world space relative to the leader's current orientation
        Vector3 worldOffset = leader.TransformDirection(followOffset);
        Vector3 desiredPosition = leader.position - leader.forward * followDistance + worldOffset;
        
        float distance = Vector3.Distance(transform.position, desiredPosition);

        if (distance <= stopDistance)
            return false;

        MoveTowards(desiredPosition, followSpeed, followRotationSpeed);
        return true;
    }

    void MoveTowards(Vector3 destination, float speed, float rotSpeed)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        // 1. Smoothly rotate head/body toward the destination first
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        // 2. Only step forward in the direction the rabbit is currently facing
        // (prevents sliding sideways/backwards)
        float forwardStep = Mathf.Max(0f, Vector3.Dot(transform.forward, direction.normalized));
        transform.position += transform.forward * (speed * forwardStep * Time.deltaTime);
    }

    // Draws the invisible wander boundary in the Scene view only (never visible in-game)
    void OnDrawGizmosSelected()
    {
        if (role != Role.Leader) return;

        Vector3 center = Application.isPlaying ? wanderCenter : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
}
using UnityEngine;

public class WolfAI : PredatorAI
{
    [Header("Wolf Traits")]
    public float patrolSpeed = 1.3f;
    public float chaseSpeed = 6f;
    public float strayBonus = 0.5f;
    public float leaderPenalty = 1.5f;

    protected override void Awake()
    {
        base.Awake();
        agent.speed = patrolSpeed;
    }

    protected override float ScoreTarget(Prey p, float sqrDist)
    {
        var group = p.GetComponent<PreyGroupAI>();
        if (group == null) return sqrDist;

        if (group.role == PreyGroupAI.Role.Leader)
            return sqrDist * leaderPenalty;

        if (group.leader != null)
        {
            float stray = Vector3.Distance(p.transform.position, group.leader.position);
            return sqrDist - stray * stray * strayBonus;
        }
        return sqrDist;
    }

    protected override void OnTargetAcquired(Prey p) => huntSpeed = chaseSpeed;
    protected override void OnTargetLost()           => agent.speed = patrolSpeed;
}
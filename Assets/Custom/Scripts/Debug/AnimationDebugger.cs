using UnityEngine;
using UnityEngine.AI;

public class AnimatorDebugger : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public string speedParam = "speed";
    public bool logEveryFrame = true; // false = only log when the clip changes

    string lastClip;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (animator == null) return;

        var clips = animator.GetCurrentAnimatorClipInfo(0);
        string clipName = clips.Length > 0 ? clips[0].clip.name : "(none)";

        string next = "";
        if (animator.IsInTransition(0))
        {
            var n = animator.GetNextAnimatorClipInfo(0);
            if (n.Length > 0) next = $" -> {n[0].clip.name}";
        }

        float animSpeed = animator.GetFloat(speedParam); // warns if the parameter doesn't exist
        float agentSpeed = agent != null ? agent.velocity.magnitude : -1f;

        if (logEveryFrame || clipName != lastClip)
        {
            Debug.Log($"[{name}] clip: {clipName}{next} | animator {speedParam}: {animSpeed:F2} | agent velocity: {agentSpeed:F2}", this);
        }

        lastClip = clipName;
    }
}
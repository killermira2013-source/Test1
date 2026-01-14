using UnityEngine;
using UnityEngine.AI;

public class EnemyRagdoll : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Collider mainCollider;
    [SerializeField] private EnemyAI enemyAI;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    private void Awake()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        ToggleRagdoll(false);
    }

    public void ActivateRagdoll()
    {
        if (animator) animator.enabled = false;
        if (agent) agent.enabled = false;
        if (enemyAI) enemyAI.enabled = false;

        if (mainCollider) mainCollider.enabled = false;

        ToggleRagdoll(true);
    }

    private void ToggleRagdoll(bool isRagdoll)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !isRagdoll;
        }

        foreach (var col in ragdollColliders)
        {
            if (col != mainCollider)
            {
                col.enabled = isRagdoll;
            }
        }
    }

    public void ApplyForce(Vector3 forcePosition, Vector3 forceDirection)
    {
        Rigidbody closestBone = null;
        float minDistance = float.MaxValue;

        foreach (var rb in ragdollBodies)
        {
            float dist = Vector3.Distance(rb.position, forcePosition);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestBone = rb;
            }
        }

        if (closestBone != null)
        {
            closestBone.AddForce(forceDirection, ForceMode.Impulse);
        }
    }
}
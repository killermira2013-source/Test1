using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private EnemyRagdoll ragdoll;

    private void Start()
    {
        currentHealth = maxHealth;
        if (gameObject.CompareTag("Player"))
        {
            GameUI.Instance.UpdateHealth(currentHealth);
        }
        ragdoll = GetComponent<EnemyRagdoll>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} получил {amount} урона. Осталось: {currentHealth}");
        if (TryGetComponent(out EnemyAI enemyAI))
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player != null)
            {
                enemyAI.OnHit(player.position);
            }
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitForce)
    {
        TakeDamage(amount);

        if (gameObject.CompareTag("Player"))
        {
            GameUI.Instance.UpdateHealth(currentHealth);
        }

        if (currentHealth <= 0 && ragdoll != null)
        {
            ragdoll.ApplyForce(hitPoint, hitForce);
        }
    }

    private void Die()
    {
        if (gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            if (ragdoll != null)
            {
                ragdoll.ActivateRagdoll();
                Destroy(gameObject, 30f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float impactForce = 15f;

    [System.Serializable]
    public struct ImpactInfo
    {
        public string tag;
        public GameObject effect;
    }

    [Header("Ёффекты по “егам")]
    [SerializeField] private List<ImpactInfo> impactEffects;
    [SerializeField] private GameObject defaultHitEffect;

    private float damage;
    private TrailRenderer trail;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.enabled = false;
    }

    public void Setup(float _damage, Vector3 _shootDir, float _bulletSpeed, Vector3 _playerVelocity)
    {
        damage = _damage;

        if (rb != null)
        {
            rb.linearVelocity = (_shootDir * _bulletSpeed) + (_playerVelocity * 0.5f);
        }

        Destroy(gameObject, lifeTime);
        if (trail != null) Invoke(nameof(EnableTrail), 0.05f);
    }

    private void EnableTrail()
    {
        if (trail != null) trail.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Health targetHealth))
        {
            Vector3 forceDir = rb.linearVelocity.normalized;

            targetHealth.TakeDamage(damage, collision.contacts[0].point, forceDir * impactForce);
        }
        else if (collision.gameObject.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(damage);
        }

        GameObject effectToSpawn = defaultHitEffect;
        string hitTag = collision.gameObject.tag;

        foreach (var impact in impactEffects)
        {
            if (impact.tag == hitTag)
            {
                effectToSpawn = impact.effect;
                break;
            }
        }

        if (effectToSpawn != null)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 spawnPos = contact.point + contact.normal * 0.05f;
            Quaternion spawnRot = Quaternion.LookRotation(contact.normal);

            GameObject impact = Instantiate(effectToSpawn, spawnPos, spawnRot);
            impact.transform.localScale = Vector3.one;
            Destroy(impact, 2f);
        }

        Destroy(gameObject);
    }
}
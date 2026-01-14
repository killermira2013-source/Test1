using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum PatrolMode { Loop, PingPong }
    private enum State { Patrol, Chase, Attack, Reload }

    [Header("Состояние")]
    [SerializeField] private State currentState;

    [Header("Настройки Зрения")]
    [SerializeField] private Transform eyes;
    [SerializeField] private float sightDistance = 25f;
    [SerializeField] private float viewAngle = 120f;
    [SerializeField] private LayerMask viewMask;

    [Header("Настройки Боя")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Настройки Памяти")]
    [SerializeField] private float chaseMemoryTime = 10f;
    [SerializeField] private float chaseTimer;

    [Header("Настройки Осмотра")]
    [SerializeField] private float lookInterval = 2f;
    [SerializeField] private float lookTurnSpeed = 2f; 
    [SerializeField] private float lookAngleRange = 60f; 

    [Header("Патруль")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Loop;
    [SerializeField] private float patrolWaitTime = 3f;
    [SerializeField] private Transform[] patrolPoints;

    private int currentPatrolIndex = 0;
    private bool patrolForward = true;
    private bool isWaitingAtPoint = false;
    private float patrolTimer;

    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private float nextFireTime;
    private Collider playerCollider;
    private Transform playerCamera;

    private int currentAmmo;
    private float reloadTimer;
    private Vector3 lastKnownPosition;
    private Vector3 targetShootingPoint;

    private float lookTimer;
    private Quaternion targetLookRotation;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0f;

        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            playerCamera = player.GetComponentInChildren<Camera>()?.transform;
            if (playerCamera == null) playerCamera = Camera.main.transform;
        }

        currentAmmo = maxAmmo;
        lastKnownPosition = player.position;
        targetShootingPoint = player.position + Vector3.up * 1.5f;

        currentState = State.Patrol;
        GoToNextPatrolPoint();

        targetLookRotation = transform.rotation;
    }

    private void Update()
    {
        if (player == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        if (animator) animator.SetBool("IsMoving", isMoving);

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                if (CanSeePlayer()) WakeUpAggro();
                break;

            case State.Chase:
                ChaseLogic();
                break;

            case State.Attack:
                AttackLogic();
                break;

            case State.Reload:
                ReloadLogic();
                break;
        }
    }

    public void OnHit(Vector3 shooterPos)
    {
        lastKnownPosition = shooterPos;
        WakeUpAggro();
    }

    private void WakeUpAggro()
    {
        chaseTimer = chaseMemoryTime;
        isWaitingAtPoint = false;
        if (currentState != State.Reload && currentState != State.Attack)
        {
            currentState = State.Chase;
        }
    }

    private void LookAroundBehavior()
    {
        lookTimer -= Time.deltaTime;

        if (lookTimer <= 0)
        {
            lookTimer = lookInterval;

            float randomY = Random.Range(-lookAngleRange, lookAngleRange);

            targetLookRotation = Quaternion.Euler(0, transform.eulerAngles.y + randomY, 0);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRotation, Time.deltaTime * lookTurnSpeed);
    }

    private void PatrolLogic()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaitingAtPoint)
        {
            patrolTimer -= Time.deltaTime;

            LookAroundBehavior();

            if (patrolTimer <= 0)
            {
                isWaitingAtPoint = false;
                GoToNextPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isWaitingAtPoint = true;
            patrolTimer = patrolWaitTime;
            lookTimer = 0;
            agent.isStopped = true;
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;

        if (patrolMode == PatrolMode.Loop)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        else if (patrolMode == PatrolMode.PingPong)
        {
            if (patrolForward)
            {
                currentPatrolIndex++;
                if (currentPatrolIndex >= patrolPoints.Length - 1)
                {
                    currentPatrolIndex = patrolPoints.Length - 1;
                    patrolForward = false;
                }
            }
            else
            {
                currentPatrolIndex--;
                if (currentPatrolIndex <= 0)
                {
                    currentPatrolIndex = 0;
                    patrolForward = true;
                }
            }
        }

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void ChaseLogic()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer())
        {
            chaseTimer = chaseMemoryTime;

            if (distance <= attackRange)
            {
                currentState = State.Attack;
                agent.ResetPath();
            }
            else
            {
                agent.SetDestination(player.position);
            }
        }
        else
        {
            agent.SetDestination(lastKnownPosition);

            if (Vector3.Distance(transform.position, lastKnownPosition) < 2f)
            {
                agent.isStopped = true;

                LookAroundBehavior();

                chaseTimer -= Time.deltaTime;
            }
            else
            {
                agent.isStopped = false;
                chaseTimer = chaseMemoryTime;
            }

            if (chaseTimer <= 0)
            {
                currentState = State.Patrol;
                isWaitingAtPoint = false;
                GoToNextPatrolPoint();
            }
        }
    }
    private void AttackLogic()
    {
        if (currentAmmo <= 0) { StartReload(); return; }
        if (!CanSeePlayer()) { currentState = State.Chase; agent.SetDestination(lastKnownPosition); return; }
        if (Vector3.Distance(transform.position, player.position) > attackRange * 1.2f) { currentState = State.Chase; return; }

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);

        if (Time.time >= nextFireTime) { nextFireTime = Time.time + 1f / fireRate; Shoot(); }
    }

    private void StartReload()
    {
        currentState = State.Reload;
        reloadTimer = reloadTime;
        agent.ResetPath();
        if (TryFindCover(out Vector3 coverPos)) agent.SetDestination(coverPos);
    }

    private void ReloadLogic()
    {
        if (agent.pathPending || agent.remainingDistance > 0.5f) return;
        if (agent.velocity.sqrMagnitude > 0.1f) { agent.ResetPath(); return; }
        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0)
        {
            currentAmmo = maxAmmo;
            currentState = State.Chase;
            agent.SetDestination(lastKnownPosition);
            chaseTimer = chaseMemoryTime;
        }
    }

    private bool TryFindCover(out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 15f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
            {
                if (Physics.Raycast(lastKnownPosition + Vector3.up * 1.5f, (hit.position - lastKnownPosition).normalized, out RaycastHit rayHit, Vector3.Distance(lastKnownPosition, hit.position), viewMask))
                {
                    if (rayHit.transform.root != transform.root && rayHit.transform.root != player.root)
                    {
                        result = hit.position;
                        return true;
                    }
                }
            }
        }
        result = Vector3.zero;
        return false;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 2f) { lastKnownPosition = player.position; targetShootingPoint = player.position + Vector3.up * 1.2f; return true; }
        if (distance > sightDistance) return false;

        Vector3 centerPoint = (playerCollider != null) ? playerCollider.bounds.center : player.position + Vector3.up * 1.5f;
        Vector3[] checkPoints = new Vector3[4];
        checkPoints[0] = centerPoint;
        checkPoints[1] = player.position + Vector3.up * 1.7f;
        checkPoints[2] = (playerCamera != null) ? playerCamera.position : centerPoint + Vector3.up * 0.5f;
        checkPoints[3] = (playerCollider != null) ? centerPoint + player.transform.right * 0.3f : centerPoint;

        foreach (Vector3 target in checkPoints)
        {
            Vector3 dirToTarget = (target - eyes.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) > viewAngle / 2) continue;
            RaycastHit hit;
            if (Physics.Raycast(eyes.position, dirToTarget, out hit, sightDistance, viewMask))
            {
                if (hit.transform.root == player.root)
                {
                    lastKnownPosition = player.position;
                    targetShootingPoint = target;
                    return true;
                }
            }
        }
        return false;
    }

    private void Shoot()
    {
        currentAmmo--;
        if (animator) animator.SetTrigger("Shoot");
        Vector3 direction = (targetShootingPoint - firePoint.position).normalized;
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null) bulletScript.Setup(10f, direction, 30f, Vector3.zero);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
}
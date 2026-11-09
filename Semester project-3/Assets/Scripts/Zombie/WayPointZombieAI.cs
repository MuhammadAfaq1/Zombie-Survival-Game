
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WayPointZombieAI : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public Transform player;

    [Header("Random Patrol Settings")]
    public float patrolRadius = 10f;
    public float waypointWaitTime = 1f;
    public float waypointReachDistance = 0.5f;
    public int maxRandomAttempts = 10;

    [Header("Zombie Settings")]
    public float walkSpeed = 1.25f;
    public float chaseSpeed = 2f;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float attackCoolDown = 2f;
    public float attackDelay = 1.5f;
    public int attackDamage = 10;
    public int health = 100;

    [Header("Damage Animation")]
    public string damageTrigger = "takeDamage";

    [Header("Blood Screen Effect")]
    public GameObject bloodScreenEffect;

    public enum ZombieState
    {
        Walking,
        Chasing,
        Attacking,
        Dead
    }

    public ZombieState currentState = ZombieState.Walking;

    private CapsuleCollider capsuleCollider;
    private bool isAttacking;
    private bool waitingAtPoint;
    private float nextAttackTime;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
        else
            Debug.LogError("Player object not found!");

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        capsuleCollider = GetComponent<CapsuleCollider>();

        // CALL RESET HERE ON SPAWN
        ResetZombieState();

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is missing!");
            enabled = false;
            return;
        }

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("Zombie is not on the NavMesh!");
            enabled = false;
            return;
        }

        navMeshAgent.speed = walkSpeed;

        ChooseRandomDestination();
    }
   private void Update()
    {
        if (currentState == ZombieState.Dead)
            return;

        float distanceToPlayer = player != null
            ? Vector3.Distance(transform.position, player.position)
            : Mathf.Infinity;

        switch (currentState)
        {
            case ZombieState.Walking:

                Patrol();

                if (distanceToPlayer <= chaseRange)
                {
                    StopAllCoroutines();
                    waitingAtPoint = false;

                    currentState = ZombieState.Chasing;
                    navMeshAgent.isStopped = false;
                }

                break;

            case ZombieState.Chasing:

                if (distanceToPlayer <= attackRange)
                {
                    currentState = ZombieState.Attacking;
                    navMeshAgent.isStopped = true;

                    SetAnimationState(false, true);
                    StartAttack();
                }
                else if (distanceToPlayer > chaseRange)
                {
                    ReturnToPatrol();
                }
                else
                {
                    ChasePlayer();
                }

                break;

            case ZombieState.Attacking:

                if (player == null ||
                    distanceToPlayer > attackRange + 0.3f)
                {
                    currentState = ZombieState.Chasing;
                    navMeshAgent.isStopped = false;

                    SetAnimationState(true, false);
                }
                else
                {
                    FacePlayer();

                    if (!isAttacking && Time.time >= nextAttackTime)
                        StartAttack();
                }

                break;
        }
    }

    public void ResetZombieState()
    {
        currentState = ZombieState.Walking;
        health = 100; // Reset health to max
        isAttacking = false;
        waitingAtPoint = false;

        if (capsuleCollider != null)
            capsuleCollider.enabled = true;

        if (navMeshAgent != null)
            navMeshAgent.enabled = true;

        if (animator != null)
        {
            animator.SetBool("isDead", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
            animator.ResetTrigger(damageTrigger);
        }

        // Clean up any lingering blood marks from previous pooled instances
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.name == "BloodImpactMark")
            {
                Destroy(child.gameObject);
            }
        }
    }

 
    // RANDOM PATROL

    private void Patrol()
    {
        if (navMeshAgent.pathPending)
            return;

        if (!navMeshAgent.hasPath ||
            navMeshAgent.remainingDistance <= waypointReachDistance)
        {
            if (!waitingAtPoint)
                StartCoroutine(WaitAndChooseNextPoint());
        }
    }

    private void ChooseRandomDestination()
    {
        for (int i = 0; i < maxRandomAttempts; i++)
        {
            Vector3 randomDirection =
                Random.insideUnitSphere * patrolRadius;

            randomDirection += transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomDirection,
                out hit,
                patrolRadius,
                NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                bool validPath =
                    navMeshAgent.CalculatePath(hit.position, path);

                if (validPath &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    navMeshAgent.isStopped = false;

                    bool destinationSet =
                        navMeshAgent.SetDestination(hit.position);

                    if (destinationSet)
                    {
                        Debug.Log(
                            "NEW WAYPOINT GENERATED: " + hit.position
                        );

                        if (animator != null)
                            animator.SetBool("isWalking", true);

                        return;
                    }
                }
            }
        }

        Debug.LogWarning(
            "Could not find a reachable waypoint for this zombie!"
        );
    }

    private IEnumerator WaitAndChooseNextPoint()
    {
        waitingAtPoint = true;
        navMeshAgent.isStopped = true;

        SetAnimationState(false, false);

        yield return new WaitForSeconds(waypointWaitTime);

        waitingAtPoint = false;

        if (currentState == ZombieState.Walking)
            ChooseRandomDestination();
    }

    private void ReturnToPatrol()
    {
        StopAllCoroutines();

        isAttacking = false;
        waitingAtPoint = false;

        currentState = ZombieState.Walking;

        navMeshAgent.speed = walkSpeed;
        navMeshAgent.isStopped = false;

        ChooseRandomDestination();
    }

    // CHASING

    private void ChasePlayer()
    {
        if (player == null)
            return;

        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.isStopped = false;

        navMeshAgent.SetDestination(player.position);

        SetAnimationState(true, false);
    }

    // PLAYER ROTATION

    private void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }

    // ATTACKING

    private void StartAttack()
    {
        if (isAttacking)
            return;

        StartCoroutine(AttackWithDelay());
    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCoolDown;

        SetAnimationState(false, true);

        yield return new WaitForSeconds(attackDelay);

        if (currentState != ZombieState.Dead &&
            player != null &&
            Vector3.Distance(transform.position, player.position)
                <= attackRange + 0.3f)
        {
            PlayerMovement playerMovement =
                player.GetComponent<PlayerMovement>();

            if (playerMovement != null)
                playerMovement.TakeDamage(attackDamage);

            if (bloodScreenEffect != null)
            {
                GameObject effect = Instantiate(bloodScreenEffect);
                Destroy(effect, 0.5f);
            }
        }

        isAttacking = false;

        if (currentState == ZombieState.Attacking)
            SetAnimationState(false, true);
    }

    // ANIMATIONS

    private void SetAnimationState(bool walking, bool attacking)
    {
        if (animator == null)
            return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isAttacking", attacking);
    }

    // DAMAGE

    public void TakeDamage(int damageAmount)
    {
        if (currentState == ZombieState.Dead)
            return;

        health -= damageAmount;

        Debug.Log("Waypoint Zombie health: " + health);

        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }

        if (animator != null)
        {
            animator.ResetTrigger(damageTrigger);
            animator.SetTrigger(damageTrigger);
        }
    }

    // DEATH

    private void Die()
    {
        if (currentState == ZombieState.Dead)
            return;

        currentState = ZombieState.Dead;

        // Add score and count this zombie exactly once.
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(1);
          
        }
        else
        {
            Debug.LogWarning("GameManager instance not found!");
        }

        StopAllCoroutines();

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        if (capsuleCollider != null)
            capsuleCollider.enabled = false;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isDead", true);
            animator.ResetTrigger(damageTrigger);
        }

        Debug.Log("Waypoint Zombie died. +1 Score");
    }
 
  
}

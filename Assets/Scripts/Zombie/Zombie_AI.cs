
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Zombie_AI : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public Transform player;

    [Header("Zombie Settings")]
    public float chaseRange = 10f;
    public float chaseSpeed = 2f;
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
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    public ZombieState currentState = ZombieState.Idle;

    private CapsuleCollider capsuleCollider;
    private bool isAttacking;
    private float nextAttackTime;
    private GameObject instantiatedObject;

   void Start()
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
            animator = GetComponent<Animator>();

        capsuleCollider = GetComponent<CapsuleCollider>();
        nextAttackTime = 0f;

        // CALL RESET HERE ON SPAWN
        ResetZombieState();

        if (navMeshAgent != null)
            navMeshAgent.speed = chaseSpeed;
    }
void Update()
    {
        if (currentState == ZombieState.Dead || player == null)
            return;

        float distance =
            Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case ZombieState.Idle:

                SetAnimationState(false, false);

                if (distance <= chaseRange)
                    currentState = ZombieState.Chasing;

                break;

            case ZombieState.Chasing:

                if (distance > chaseRange)
                {
                    currentState = ZombieState.Idle;

                    if (navMeshAgent != null)
                        navMeshAgent.isStopped = true;

                    SetAnimationState(false, false);
                    break;
                }

                if (distance <= attackRange)
                {
                    currentState = ZombieState.Attacking;

                    if (navMeshAgent != null)
                        navMeshAgent.isStopped = true;

                    SetAnimationState(false, true);
                }
                else
                {
                    if (navMeshAgent != null)
                    {
                        navMeshAgent.speed = chaseSpeed;
                        navMeshAgent.isStopped = false;
                        navMeshAgent.SetDestination(player.position);
                    }

                    SetAnimationState(true, false);
                }

                break;

            case ZombieState.Attacking:

                if (distance > attackRange)
                {
                    currentState = ZombieState.Chasing;

                    if (navMeshAgent != null)
                    {
                        navMeshAgent.speed = chaseSpeed;
                        navMeshAgent.isStopped = false;
                    }

                    SetAnimationState(true, false);
                    break;
                }

                if (navMeshAgent != null)
                    navMeshAgent.isStopped = true;

                FacePlayer();
                SetAnimationState(false, true);

                if (!isAttacking && Time.time >= nextAttackTime)
                    StartCoroutine(AttackWithDelay());

                break;
        }
    }

    public void ResetZombieState()
    {
        currentState = ZombieState.Idle;
        health = 100;
        isAttacking = false;

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

        // Clean up any lingering blood marks
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.name == "BloodImpactMark")
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    private void SetAnimationState(bool walking, bool attacking)
    {
        if (animator == null)
            return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isAttacking", attacking);
    }

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

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCoolDown;

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
                instantiatedObject = Instantiate(bloodScreenEffect);
                Destroy(instantiatedObject, 0.5f);
            }
        }

        isAttacking = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == ZombieState.Dead)
            return;

        health -= damageAmount;

        Debug.Log("Zombie health: " + health);

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

        Debug.Log("Zombie died. +1 Score");
    }
}

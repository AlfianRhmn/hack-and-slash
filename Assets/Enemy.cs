using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField]private int HP = 100;
    private Animator animator;
    private NavMeshAgent navAgent;
    Transform player;
    public float chaseSpeed = 6f;
    public float stopChasingDistance = 21f;
    public float attackingDistance = 2.5f;
    public float stopAttackingDistance = 2.5f;
    public float idleTime = 0f;
    public float detectionAreaRadius = 10f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {
            animator.SetTrigger("DIE");
        }
        else
        {
            animator.SetTrigger("HIT");
        }
    }

    private void Update()
    {
        animator.transform.LookAt(player);


        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        if (distanceFromPlayer < detectionAreaRadius)
        {
            animator.SetBool("isRun", true);
        }
        if (distanceFromPlayer > stopChasingDistance)
        {
            animator.SetBool("isRun", true);
        }

        if (distanceFromPlayer < attackingDistance)
        {
            animator.SetBool("isAttacking", true);
        }

        if (distanceFromPlayer > attackingDistance)
        {
            animator.SetBool("isAttacking", false);
        }
    }
}

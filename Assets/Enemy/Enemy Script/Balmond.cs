using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Balmond : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public float HP = 100;
    public Slider healthBar;
    private Animator animator;
    private float smoothDampVelocity;

    [Header("Movement & Target Settings")]
    public Transform target;
    public float detectionRange = 40f;
    public float stopChasingRange = 50f;
    public float attackRange = 2.5f;
    public float movementSpeed = 3f;
    private NavMeshAgent navAgent;
    private bool isChasing = false; 

    [Header("Attack Settings")]
    public float attackCooldown = 2.0f; 
    private float nextAttackTime;

    [Header("Taunt Settings")]
    public float tauntCooldown = 5.0f; 
    private float nextTauntTime;
    [Range(0f, 1f)] public float tauntChance = 0.3f; 
    private bool isEnraged = false;
    private bool isDead = false; 

    private void Start()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogWarning("PlayerManager.Instance is null. Balmond not added to enemy list.");
        }

        healthBar.maxValue = HP;
        healthBar.value = HP;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        nextAttackTime = Time.time;
        nextTauntTime = Time.time;

        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.speed = movementSpeed;
            navAgent.stoppingDistance = attackRange; 
        }
    }

    private void Update()
    {
        if (isDead && navAgent.enabled)
        {
            {
                navAgent.isStopped = true;
                navAgent.enabled = false;
            }
            return;
        }

        healthBar.value = Mathf.SmoothDamp(healthBar.value, HP, ref smoothDampVelocity, 0.1f);

        if (target != null && navAgent != null && navAgent.isActiveAndEnabled) 
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (HP <= 50 && !isEnraged)
            {
                ActivateEnragedMode();
            }

            // --- Movement and Chasing Logic ---
            if (distanceToTarget <= detectionRange && distanceToTarget > attackRange)
            {
                isChasing = true;
                navAgent.isStopped = false; 
                navAgent.SetDestination(target.position);
                animator.SetFloat("Speed", movementSpeed); 
            }
            else if (distanceToTarget <= attackRange)
            {
                isChasing = false;
                navAgent.isStopped = true; 
                animator.SetFloat("Speed", 0f); 
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z)); // Menghadap target

                // --- Attack Logic ---
                if (Time.time >= nextAttackTime)
                {
                    if (isEnraged)
                    {
                        PerformPhase2Attack();
                    }
                    else
                    {
                        PerformAttack(); 
                    }
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else if (distanceToTarget > stopChasingRange)
            {
                isChasing = false;
                navAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
            }
            else
            {
                isChasing = false;
                navAgent.isStopped = true;
                animator.SetFloat("Speed", 0f); 
            }
        }
        else 
        {
            isChasing = false;
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.isStopped = true;
            }
            animator.SetFloat("Speed", 0f); 
        }

        if (Time.time >= nextTauntTime && !isChasing && animator.GetFloat("Speed") < 0.1f && !animator.GetCurrentAnimatorStateInfo(0).IsTag("AttackState"))
        {
            if (UnityEngine.Random.value < tauntChance) 
            {
                PerformTaunt(); 
            }
            nextTauntTime = Time.time + tauntCooldown;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return; 
        HP -= damageAmount;
        HP = Mathf.Max(HP, 0f); 

        // Memicu animasi HIT
        animator.SetTrigger("Hit");   

        if (HP <= 50 && !isEnraged)
        {
            ActivateEnragedMode();
        }

        // Cek kematian
        if (HP <= 0 && !isDead)
        {
            isDead = true; 
            animator.SetBool("IsDead", true);
            StartCoroutine(Dead());
        }
    }

    void PerformAttack()
    {
       
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        animator.SetTrigger("Attack"); 
        Debug.Log("Balmond Attacks (Phase 1)!");
       
    }

    void PerformPhase2Attack() 
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        animator.SetTrigger("Phase2Attack"); 
        Debug.Log("Balmond performs Phase 2 Attack!");

    }

    void PerformTaunt()
    {
        // Memastikan NavMeshAgent berhenti selama animasi taunt
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        animator.SetFloat("Speed", 0f); 

        if (!isEnraged)
        {
            animator.SetTrigger("TauntChest"); 
            Debug.Log("Balmond Taunts (Chest - Phase 1)!");
        }
        else 
        {
            animator.SetTrigger("TauntBattlecry"); 
            Debug.Log("Balmond Taunts (Battlecry - Phase 2)!");
        }
    }

    void ActivateEnragedMode()
    {
        isEnraged = true;
        animator.SetBool("ENRAGED", true);
        Debug.Log("Balmond enters ENRAGED mode (Phase 2)!");

        nextAttackTime = Time.time;


        movementSpeed *= 1.3f; 
        attackCooldown *= 0.7f;
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.speed = movementSpeed;

            PerformTaunt();
        }
    }

    public IEnumerator LaunchEnemy(float duration, float launchHeight)
    {
        if (navAgent != null && navAgent.isActiveAndEnabled) 
        {
            navAgent.enabled = false; 
        }

        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float yOffset = Mathf.Sin(t * Mathf.PI) * launchHeight; 

            transform.position = new Vector3(
                originalPosition.x,
                originalPosition.y + yOffset,
                originalPosition.z
            );

            yield return null;
        }

        transform.position = originalPosition; 
        if (!isDead && navAgent != null)
        {
            navAgent.enabled = true; 
        }
    }

    IEnumerator Dead()
    {
        isDead = true; 
        if (navAgent != null)
        {
            navAgent.isStopped = true; 
            navAgent.enabled = false; 
        }
        animator.SetFloat("Speed", 0f); 

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float dieAnimLength = 0f;
        if (stateInfo.IsName("Die"))
        {
            dieAnimLength = stateInfo.length;
        } else {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.ToLower().Contains("die")) 
                {
                    dieAnimLength = clip.length;
                    break;
                }
            }
            if (dieAnimLength == 0f) dieAnimLength = 1f; 
        }
        
        yield return new WaitForSeconds(dieAnimLength + 1);

        healthBar.gameObject.SetActive(false); 

        if (PlayerManager.Instance != null && PlayerManager.Instance.enemyList.Contains(this))
        {
            PlayerManager.Instance.enemyList.Remove(this);
        }

        // Hancurkan GameObject Balmond setelah animasi mati selesai dan semua bersih
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Jangkauan serangan
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Jangkauan deteksi (mulai mengejar)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Jangkauan berhenti mengejar (jika ada)
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, stopChasingRange);
    }
}
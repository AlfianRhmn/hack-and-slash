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
    public Transform target; // Pastikan ini terisi dengan GameObject Player di Inspector
    public float detectionRange = 40f;
    public float stopChasingRange = 50f;
    public float attackRange = 2.5f;
    public float movementSpeed = 3f;
    private NavMeshAgent navAgent;
    private bool isChasing = false; 

    [Header("Attack Settings")]
    public float attackCooldown = 2.0f; 
    private float nextAttackTime;
    public float phase2AttackCooldown = 2.0f; 
    private float currentAttackCooldown;   
    public float attackDamagePhase1 = 15f; // Damage untuk Fase 1
    public float attackDamagePhase2 = 25f; // Damage untuk Fase 2

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
            //PlayerManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogWarning("PlayerManager.Instance is null. Balmond not added to enemy list.");
        }

        healthBar.maxValue = HP;
        healthBar.value = HP;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        currentAttackCooldown = attackCooldown;

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
        if (isDead)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
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
                animator.SetFloat("Speed", navAgent.velocity.magnitude); // Menggunakan kecepatan NavMeshAgent
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
                    nextAttackTime = Time.time + currentAttackCooldown; // Menggunakan currentAttackCooldown
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

        animator.SetTrigger("Hit");   

        if (HP <= 50 && !isEnraged)
        {
            ActivateEnragedMode();
        }

        if (HP <= 0)
        {
            isDead = true; 
            animator.SetTrigger("Die");
            if (PlayerManager.Instance != null)
            {
                //PlayerManager.Instance.enemyList.Remove(this);
            }
            StartCoroutine(Dead());
        }
    }

    public void SetAir(bool onAir)
    {
        if (onAir)
        {
            navAgent.enabled = false;
        } else
        {
            navAgent.enabled = true;
        }
    }

    void PerformAttack()
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        animator.SetTrigger("Attack"); 
    }

    void PerformPhase2Attack()
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        animator.SetTrigger("Phase2Attack");

        // nextAttackTime akan diatur di Update setelah panggilan PerformPhase2Attack
        // agar sesuai dengan cooldown yang diperbarui saat enraged.
    }

    void PerformTaunt()
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        animator.SetFloat("Speed", 0f); 

        if (!isEnraged)
        {
            animator.SetTrigger("TauntChest"); 
        }
        else 
        {
            animator.SetTrigger("TauntBattlecry"); 
        }
    }

    void ActivateEnragedMode() //function
    {
        isEnraged = true;
        animator.SetBool("ENRAGED", true);

        currentAttackCooldown = phase2AttackCooldown; 
        nextAttackTime = Time.time; 

        movementSpeed *= 1.3f;
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.speed = movementSpeed; 
        }
        PerformTaunt();
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
        if (navAgent != null && navAgent.isActiveAndEnabled)
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
        
        yield return new WaitForSeconds(dieAnimLength);

        healthBar.gameObject.SetActive(false); 

        //Destroy(gameObject); // Hancurkan GameObject Balmond
    }

    // Metode ini akan dipanggil oleh Animation Event
    public void DealDamageToPlayer()
    {
        Debug.Log("DealDamageToPlayer() dipanggil oleh Animation Event!"); // Debug log untuk konfirmasi

        // Pastikan ada target dan berada dalam jangkauan serangan saat event dipicu
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            PlayerCombat playerCombat = target.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                float damageToDeal = isEnraged ? attackDamagePhase2 : attackDamagePhase1;
                
                // --- PENTING: Panggil TakeDamage dengan DUA parameter ---
                playerCombat.TakeDamage(damageToDeal, this.transform); // Menggunakan 'this.transform' sebagai sourceOfDamage
                Debug.Log($"Balmond menyerang pemain dengan {damageToDeal} damage! (Fase: {(isEnraged ? "2" : "1")})");
            }
            else
            {
                Debug.LogWarning("Target pemain tidak memiliki komponen PlayerCombat atau namanya salah!");
            }
        }
        else
        {
            Debug.Log("DealDamageToPlayer() dipanggil, tapi target tidak ada atau di luar jangkauan.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
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
    private bool isChasing = false; // Status apakah Balmond sedang mengejar

    [Header("Attack Settings")]
    public float attackCooldown = 2.0f; // Cooldown antar serangan
    private float nextAttackTime;

    [Header("Taunt Settings")]
    public float tauntCooldown = 5.0f; // Cooldown untuk taunt
    private float nextTauntTime;
    [Range(0f, 1f)] public float tauntChance = 0.3f; // Peluang untuk taunt (0.0 - 1.0)

    private bool isEnraged = false;

    private void Start()
    {
        // Contoh penambahan ke daftar musuh. Sesuaikan dengan implementasi PlayerManager Anda.
        PlayerManager.Instance.enemyList.Add(this);

        healthBar.maxValue = HP;
        healthBar.value = HP;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        // Inisialisasi waktu cooldown awal
        nextAttackTime = Time.time;
        nextTauntTime = Time.time;

        // Pastikan NavMeshAgent aktif dan diatur kecepatannya
        navAgent.speed = movementSpeed;
        navAgent.stoppingDistance = attackRange; // Agen akan berhenti di jarak serangan
    }

    private void Update()
    {
        // Update Health Bar UI
        healthBar.value = Mathf.SmoothDamp(healthBar.value, HP, ref smoothDampVelocity, 0.1f);

        // Logika AI Utama
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // Cek kondisi ENRAGED (Phase 2)
            if (HP <= 50 && !isEnraged)
            {
                ActivateEnragedMode();
            }

            // --- Movement and Chasing Logic ---
            if (distanceToTarget <= detectionRange && distanceToTarget > attackRange)
            {
                // Dalam jangkauan deteksi tapi di luar jangkauan serangan, mulai mengejar
                isChasing = true;
                navAgent.isStopped = false; // Pastikan agen tidak berhenti
                navAgent.SetDestination(target.position);
                animator.SetFloat("Speed", movementSpeed); // Mengatur animasi lari (akan mengontrol Run_Forward atau Phase2_Run)
            }
            else if (distanceToTarget <= attackRange)
            {
                // Dalam jangkauan serangan, berhenti bergerak dan bersiap menyerang
                isChasing = false;
                navAgent.isStopped = true; // Hentikan pergerakan NavMeshAgent
                animator.SetFloat("Speed", 0f); // Kembali ke Idle atau Attack
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z)); // Menghadap target

                // --- Attack Logic ---
                if (Time.time >= nextAttackTime)
                {
                    if (isEnraged)
                    {
                        PerformPhase2Attack(); // Memanggil Phase 2 Attack di mode ENRAGED
                    }
                    else
                    {
                        PerformAttack(); // Memanggil Attack di fase normal
                    }
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else if (distanceToTarget > stopChasingRange)
            {
                // Target terlalu jauh, berhenti mengejar
                isChasing = false;
                navAgent.isStopped = true;
                animator.SetFloat("Speed", 0f); // Kembali ke Idle
            }
            else
            {
                // Di luar jangkauan deteksi tetapi belum terlalu jauh untuk berhenti total
                isChasing = false;
                navAgent.isStopped = true;
                animator.SetFloat("Speed", 0f); // Kembali ke Idle
            }
        }
        else // Jika tidak ada target
        {
            isChasing = false;
            navAgent.isStopped = true;
            animator.SetFloat("Speed", 0f); // Kembali ke Idle
        }

        // --- Taunt Logic (bisa terjadi kapan saja, dengan peluang) ---
        // Balmond tidak akan taunt saat sedang mengejar atau menyerang
        if (Time.time >= nextTauntTime && !isChasing && animator.GetFloat("Speed") < 0.1f && !animator.GetCurrentAnimatorStateInfo(0).IsTag("AttackState"))
        {
            if (UnityEngine.Random.value < tauntChance) // Memberikan peluang untuk taunt
            {
                PerformTaunt(); // Akan memanggil TauntChest atau TauntBattlecry tergantung fase
            }
            nextTauntTime = Time.time + tauntCooldown;
        }
    }

    // Fungsi ini dipanggil dari luar (misalnya, serangan pemain)
    public void TakeDamage(float damageAmount)
    {
        RumbleManager.instance.PlayRumble(1f, 1f, 0.2f);
        HP -= damageAmount;
        HP = Mathf.Max(HP, 0f); // Pastikan HP tidak di bawah 0

        // Memicu animasi HIT
        animator.SetTrigger("Hit");    // Memicu animasi "Hit"

        if (HP <= 50 && !isEnraged)
        {
            ActivateEnragedMode();
        }

        // Cek kematian
        if (HP <= 0)
        {
            StartCoroutine(Dead());
        }
    }

    void PerformAttack()
    {
        // Memastikan NavMeshAgent berhenti selama animasi serangan
        navAgent.isStopped = true;
        animator.SetTrigger("Attack"); // Memicu trigger Attack
        Debug.Log("Balmond Attacks (Phase 1)!");
        // Tambahkan logika serangan di sini (misalnya damage ke pemain)
    }

    void PerformPhase2Attack() // Ini adalah "Phase 2 Attack"
    {
        // Memastikan NavMeshAgent berhenti selama animasi serangan
        navAgent.isStopped = true;
        animator.SetTrigger("Phase2Attack"); // Memicu trigger Phase2Attack
        Debug.Log("Balmond performs Phase 2 Attack!");
        // Tambahkan logika serangan kombo/Phase 2 di sini
    }

    void PerformTaunt()
    {
        // Memastikan NavMeshAgent berhenti selama animasi taunt
        navAgent.isStopped = true;
        animator.SetFloat("Speed", 0f); // Pastikan animasi lari berhenti saat taunt

        if (!isEnraged)
        {
            animator.SetTrigger("TauntChest"); // Memicu trigger TauntChest
            Debug.Log("Balmond Taunts (Chest - Phase 1)!");
        }
        else // Jika sudah di mode ENRAGED
        {
            animator.SetTrigger("TauntBattlecry"); // Memicu trigger TauntBattlecry
            Debug.Log("Balmond Taunts (Battlecry - Phase 2)!");
        }
    }

    void ActivateEnragedMode()
    {
        isEnraged = true;
        animator.SetBool("ENRAGED", true); // Mengatur parameter ENRAGED di Animator
        Debug.Log("Balmond enters ENRAGED mode (Phase 2)!");

        // Atur ulang cooldown serangan agar bisa langsung menyerang
        nextAttackTime = Time.time;

        // Contoh: Meningkatkan kecepatan dan mengurangi cooldown serangan di fase 2
        movementSpeed *= 1.3f; // Lebih cepat
        attackCooldown *= 0.7f; // Lebih sering menyerang
        navAgent.speed = movementSpeed; // Update kecepatan NavMeshAgent

        // Pemicu taunt battlecry saat masuk ENRAGED (opsional, bisa diatur sesuai kebutuhan)
        PerformTaunt();
    }

    // Coroutine untuk efek diluncurkan
    public IEnumerator LaunchEnemy(float duration, float launchHeight)
    {
        navAgent.enabled = false; // Nonaktifkan NavMeshAgent sementara

        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float yOffset = Mathf.Sin(t * Mathf.PI) * launchHeight; // Arc parabola

            transform.position = new Vector3(
                originalPosition.x,
                originalPosition.y + yOffset,
                originalPosition.z
            );

            yield return null;
        }

        transform.position = originalPosition; // Kembali ke posisi semula
        navAgent.enabled = true; // Aktifkan kembali NavMeshAgent
    }

    IEnumerator Dead()
    {
        GetComponent<CapsuleCollider>().enabled = false;
        animator.SetTrigger("Die");
        navAgent.enabled = false;
        animator.SetFloat("Speed", 0f);
        healthBar.gameObject.SetActive(false); // Sembunyikan health bar
        PlayerManager.Instance.enemyList.Remove(this); // Hapus dari daftar musuh
        // Mungkin perlu delay lebih lama tergantung durasi animasi DIE
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 2); // Menunggu durasi animasi DIE


        // Hancurkan GameObject Balmond setelah animasi mati selesai
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

        // Jangkauan berhenti mengejar
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, stopChasingRange);
    }
}
using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Animator animator; 
    public float currentHealth = 100f; // Contoh darah musuh
    public float maxHealth = 100f;
    public float movementSpeed = 3f; // Kecepatan gerakan musuh
    public Transform target; // Target musuh (misalnya Player)

    [Header("Attack Settings")]
    public float attackRange = 2.0f; // Jarak untuk serangan
    public float attackCooldown = 2.0f; // Cooldown antar serangan
    private float nextAttackTime;

    [Header("Taunt Settings")]
    public float tauntCooldown = 5.0f; // Cooldown untuk taunt
    private float nextTauntTime;
    public float tauntChance = 0.3f; // Peluang untuk taunt (0.0 - 1.0)

    private bool isPhase2 = false;
    // velocity tidak lagi diperlukan jika kita hanya menggunakan movementSpeed untuk animator.SetFloat("Speed")
    // float velocity; 

    // Pastikan untuk menginisialisasi animator di Start jika belum ditarik di Inspector
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        nextAttackTime = Time.time;
        nextTauntTime = Time.time;
    }

    void Update()
    {
        float distanceToTarget = Mathf.Infinity;
        if (target != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, target.position);
        }

        // Cek kondisi fase 2
        if (currentHealth <= maxHealth * 0.5f && !isPhase2)
        {
            ActivatePhase2();
        }

        // Logika AI Utama
        if (target != null)
        {
            // Jika target berada di luar jangkauan serangan, musuh akan lari (run_forward / phase2_run)
            if (distanceToTarget > attackRange)
            {
                animator.SetFloat("Speed", movementSpeed);
                // Rotasi dan gerak menuju target
                Vector3 direction = (target.position - transform.position).normalized;
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
                transform.Translate(direction * movementSpeed * Time.deltaTime, Space.World);
            }
            // Jika target dalam jangkauan serangan
            else if (distanceToTarget <= attackRange)
            {
                animator.SetFloat("Speed", 0f); // Berhenti bergerak saat menyerang atau taunt
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z)); // Tetap menghadap target

                // Logika Serangan
                if (Time.time >= nextAttackTime)
                {
                    if (isPhase2)
                    {
                        ComboAttack(); // Memanggil ComboAttack di fase 2
                    }
                    else
                    {
                        Attack(); // Memanggil Attack di fase 1
                    }
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else
        {
            // Jika tidak ada target, kembali ke Idle
            animator.SetFloat("Speed", 0f);
        }

        // Logika Taunt (bisa terjadi kapan saja, dengan peluang)
        if (Time.time >= nextTauntTime)
        {
            if (Random.value < tauntChance) // Memberikan peluang untuk taunt
            {
                Taunt(); // Akan memanggil TauntChest atau TauntBattlecry tergantung fase
            }
            nextTauntTime = Time.time + tauntCooldown;
        }
    }

    // Fungsi untuk mengurangi darah (contoh)
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f); // Pastikan darah tidak di bawah 0
        Debug.Log("Enemy Health: " + currentHealth);
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        Debug.Log("Enemy Attacks!");
        // Tambahkan logika serangan di sini (misalnya damage ke player)
    }

    void Taunt()
    {
        if (!isPhase2)
        {
            animator.SetTrigger("TauntChest");
            Debug.Log("Enemy Taunts (Chest)!");
        }
        else // Jika sudah di Phase 2
        {
            animator.SetTrigger("TauntBattlecry");
            Debug.Log("Enemy Taunts (Battlecry)!");
        }
    }

    void ActivatePhase2()
    {
        isPhase2 = true;
        animator.SetBool("Phase2", true);
        Debug.Log("Enemy enters Phase 2!");
        // Reset cooldown serangan saat masuk fase 2 agar bisa langsung menyerang
        nextAttackTime = Time.time;
        // Tambahkan efek atau perubahan perilaku lain untuk Fase 2
        // Misalnya, meningkatkan kecepatan, damage, dll.
        movementSpeed *= 1.2f; // Contoh: kecepatan meningkat 20% di fase 2
        attackCooldown *= 0.8f; // Contoh: cooldown serangan berkurang 20% di fase 2
    }

    void ComboAttack()
    {
        animator.SetTrigger("ComboAttack");
        Debug.Log("Enemy performs Combo Attack!");
    }

    // Contoh Coroutine untuk menunda aksi (opsional, tergantung kebutuhan)
    IEnumerator DelayedAction(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Lakukan sesuatu setelah delay
    }
}
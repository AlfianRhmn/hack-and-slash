using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("General - Statistics")]
    public float currentHP;
    public float maxHP;
    public float currentDamage;
    [Header("General - AI")]
    public Transform target;
    public State[] enemyState;
    public Animator anim;
    private int currentState = 0;
    private NavMeshAgent agent;
    bool isChangingState = false;
    bool isAttacking = false;
    bool hasNoticedPlayer;
    bool readyToAttack = true;
    EnemyMoveset currentMoveset;
    [Header("User Interface")]
    public Slider healthBar;
    float currentVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.maxValue = maxHP;
        healthBar.value = currentHP;
        agent = GetComponent<NavMeshAgent>();
        PlayerManager.Instance.enemyList.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        SetupUI();
        CheckEnemyState();
        HandleMovement();
        if (Vector3.Distance(transform.position, target.position) < enemyState[currentState].distanceUntilAttack && !isAttacking && !isChangingState && hasNoticedPlayer && readyToAttack)
        {
            HandleAttack();
        }
        if (currentHP <= 0)
        {
            agent.isStopped = true;
        }
    }

    void SetupUI()
    {
        healthBar.value = Mathf.SmoothDamp(healthBar.value, currentHP, ref currentVelocity, 0.1f);
    }

    void HandleMovement() //  bagian kode untuk menggerakkan musuh
    {
        if (hasNoticedPlayer && !isChangingState && !isAttacking)
        {
            agent.SetDestination(target.position);
        } else if (!hasNoticedPlayer && Vector3.Distance(target.position, transform.position) < enemyState[currentState].distanceUntilNotice && !isAttacking && !isChangingState && !isAttacking)
        {
            hasNoticedPlayer = true;
            StartCoroutine(StartChangingState(currentState));
        }

        anim.SetBool("Moving", IsWalking());
    }

    void HandleAttack() // bagian attack, mikir attack, dll.
    {
        readyToAttack = false;
        // untuk bagian pertama : musuh akan mikir dulu mau pake serangan yang mana!
        int totalProbability = 0;
        foreach (EnemyMoveset moveset in enemyState[currentState].moveset)
        {
            totalProbability += moveset.probability;
        }
        int randomAttack = Random.Range(0, totalProbability); // lempar dadu
        float cumulativeWeight = 0;
        EnemyMoveset selectedMoveset = null;
        foreach (EnemyMoveset moveset in enemyState[currentState].moveset) // memilih attack dari probabilitas
        {
            cumulativeWeight += moveset.probability;
            if (randomAttack < cumulativeWeight)
            {
                selectedMoveset = moveset;
                break;
            }
        }
        StartCoroutine(StartAttack(selectedMoveset)); // mulai serang!
    }

    bool IsWalking()
    {
        return agent.velocity.magnitude > 0.1f &&
               !agent.pathPending &&
               agent.remainingDistance > agent.stoppingDistance;
    }

    public void SetAir(bool onAir)
    {
        if (onAir)
        {
            agent.enabled = false;
        }
        else
        {
            agent.enabled = true;
        }
    }

    IEnumerator StartAttack(EnemyMoveset moveset) // masukin semua logic serangan disini
    {
        agent.isStopped = true;
        // masukin semua variable penting yang ada di EnemyMoveset disini
        currentMoveset = moveset;
        anim.runtimeAnimatorController = moveset.animOV;
        currentDamage = moveset.damage;
        isAttacking = true;
        // masukin vfx ting! buat penanda serangan
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        anim.SetTrigger("Attack");
        for (int i = 0; i < moveset.timeBeforeHitCheck.Length; i++)
        {
            StartCoroutine(CheckForHit(moveset.timeBeforeHitCheck[i]));
        }
        yield return new WaitForSeconds(moveset.duration);
        currentMoveset = null;
        isAttacking = false;
        agent.isStopped = false;
        yield return new WaitForSeconds(Random.Range(enemyState[currentState].minCooldownPerAttack, enemyState[currentState].maxCooldownPerAttack));
        readyToAttack = true;
    }


    IEnumerator CheckForHit(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (Vector3.Distance(target.position, transform.position) <= enemyState[currentState].distanceUntilAttack)
        {
            target.GetComponent<PlayerCombat>().TakeDamage(currentDamage, transform);
        }
    }

    void CheckEnemyState() // cek kalau semua kondisi darah terpenuhi
    {
        for (int i = 0; i < enemyState.Length; i++)
        {
            if (currentState >= i) continue; // jika state sudah dijalani, skip aja

            if (currentHP <= maxHP * enemyState[i].hpCondition) // kalau darah sudah dibawah persentase max hp...
            {
                StartCoroutine(StartChangingState(i)); //... mulai ganti state!
            }
        }
    }

    IEnumerator StartChangingState(int stateID)
    {
        if (currentMoveset != null)
        {
            StopCoroutine(StartAttack(currentMoveset));
            currentMoveset = null;
            isAttacking = false;
        }
        agent.isStopped = true;
        currentState = stateID;
        isChangingState = true;
        if (enemyState[stateID].changeStateAnim != null)
        {
            anim.runtimeAnimatorController = enemyState[stateID].changeStateAnim; //ganti animasi musuh
            anim.Play("Taunt");
        }
        yield return new WaitForSeconds(enemyState[stateID].changingDuration);
        agent.isStopped = false;
        isChangingState = false;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            PlayerManager.Instance.enemyList.Remove(this);
            StartCoroutine(Dead());
            //dead
        } else
        {
            StartCoroutine(EnemyHit());
        }
    }

    IEnumerator EnemyHit()
    {
        if (Random.Range(0f, 1f) <= enemyState[currentState].painTolerance)
        {
            HandleAttack();
        } else
        {
            anim.SetTrigger("Hit");
            agent.isStopped = true;
            yield return new WaitForSeconds(0.2f);
            agent.isStopped = false;
        }
    }

    IEnumerator Dead()
    {
        agent.isStopped = true;
        anim.SetTrigger("Dead");
        yield return new WaitForSeconds(0.1f);
        healthBar.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public IEnumerator LaunchEnemy(float duration, float launchHeight)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.enabled = false;
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
        if (currentHP <= 0 && agent != null)
        {
            agent.enabled = true;
        }
    }
}

[System.Serializable]
public class State
{
    [Range(0f, 1f)]
    public float hpCondition; // jika musuhnya dibawah XX% darah, state akan ganti ke ini - 0.1 itu 10% darah, 1 itu 100%, dll.
    public EnemyMoveset[] moveset; // moveset yang bisa dipakai musuh kalo dengan state ini
    public float distanceUntilNotice; // jarak musuh ke player untuk nyadar dan mulai mendekat
    public float distanceUntilAttack; // jarak musuh ke player untuk nyerang
    public AnimatorOverrideController changeStateAnim; // yang diganti cuman animasi musuh ganti state
    public float changingDuration; // waktu proses transisi
    public float minCooldownPerAttack; // cooldown setiap serangan - minimal
    public float maxCooldownPerAttack; // cooldown setiap serangan - maksimal
    [Range(0f, 1f)]
    public float painTolerance; //random jika kena hit, akan nyerang dan tidak play animasi, 1 = 100% immune
}

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
    [Header("General - AI")]
    public Transform target;
    public State[] enemyState;
    public Animator anim;
    public EnemyWeapon weapon;
    private int currentState = 0;
    private NavMeshAgent agent;
    bool isChangingState = false;
    bool isAttacking = false;
    bool hasNoticedPlayer;
    bool readyToAttack = true;
    int damageTaken;
    bool onAir = false;
    bool isDead = false;
    float timer = 0;
    Rigidbody rb;
    private Coroutine launchRoutine;
    [HideInInspector] public bool isBeingLaunched = false;
    private int airborneHitCount = 0;
    EnemyMoveset currentMoveset;
    [Header("User Interface")]
    public Slider healthBar;
    float currentVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.maxValue = maxHP;
        healthBar.value = currentHP;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        PlayerManager.Instance.enemyList.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        SetupUI();
        if (currentHP > 0)
        {
            CheckEnemyState();
            HandleMovement();
            if (Vector3.Distance(transform.position, target.position) < enemyState[currentState].distanceUntilAttack && !isAttacking && !isChangingState && hasNoticedPlayer && readyToAttack && !onAir)
            {
                HandleAttack();
            }
        }
        if (currentHP <= 0 && agent != null && agent.isOnNavMesh)
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
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(target.position);
            }
        }
        else if (!hasNoticedPlayer && Vector3.Distance(target.position, transform.position) < enemyState[currentState].distanceUntilNotice && !isAttacking && !isChangingState && !isAttacking)
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
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    IEnumerator StartAttack(EnemyMoveset moveset) // masukin semua logic serangan disini
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        // masukin semua variable penting yang ada di EnemyMoveset disini
        currentMoveset = moveset;
        anim.runtimeAnimatorController = moveset.animOV;
        weapon.damage = moveset.damage;
        isAttacking = true;
        // masukin vfx ting! buat penanda serangan
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(moveset.duration);
        currentMoveset = null;
        isAttacking = false;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        yield return new WaitForSeconds(Random.Range(enemyState[currentState].minCooldownPerAttack, enemyState[currentState].maxCooldownPerAttack));
        readyToAttack = true;
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

    public void CheckHit()
    {
        weapon.DoHit();
    }

    IEnumerator StartChangingState(int stateID)
    {
        if (currentMoveset != null)
        {
            StopCoroutine(StartAttack(currentMoveset));
            currentMoveset = null;
            isAttacking = false;
        }
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        currentState = stateID;
        isChangingState = true;
        if (enemyState[stateID].changeStateAnim != null)
        {
            anim.runtimeAnimatorController = enemyState[stateID].changeStateAnim; //ganti animasi musuh
            anim.Play("Taunt");
        }
        yield return new WaitForSeconds(enemyState[stateID].changingDuration);
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        isChangingState = false;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (onAir)
            airborneHitCount++;

        if (currentHP <= 0 && !isDead)
        {
            PlayerManager.Instance.enemyList.Remove(this);
            StopAllCoroutines();
            StartCoroutine(Dead());
        }
        else if (!isDead)
        {
            if (!isChangingState)
            {
                StartCoroutine(EnemyHit());
            }
        }
    }


    IEnumerator EnemyHit()
    {
        if (Random.Range(0f, 1f) <= enemyState[currentState].painTolerance)
        {
            HandleAttack();
        }
        else
        {
            anim.SetTrigger("Hit");
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            yield return new WaitForSeconds(0.2f);
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }

    IEnumerator Dead()
    {
        isDead = true;
        if (onAir)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        anim.SetTrigger("Dead");
        yield return new WaitForSeconds(0.1f);
        healthBar.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void StartLaunch(float duration, float height)
    {
        // Call this to safely start the launch
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        launchRoutine = StartCoroutine(LaunchEnemy(duration, height));
    }

    public IEnumerator LaunchEnemy(float duration, float launchHeight)
    {
        if (isBeingLaunched)
            yield break;

        isBeingLaunched = true;
        airborneHitCount = 0;
        onAir = true;

        if (agent != null && agent.isActiveAndEnabled)
            agent.enabled = false;

        // Freeze during launch arc
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        float halfDuration = duration / 2f;

        // Launch arc (manual movement upward)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float yOffset = Mathf.Sin(t * Mathf.PI * 0.5f) * launchHeight;

            transform.position = new Vector3(
                startPosition.x,
                startPosition.y + yOffset,
                startPosition.z
            );

            yield return null;
        }

        // Floating phase: wait for 5 hits or 1.5s with no hit
        float noHitTimer = 0f;
        int lastHitCount = airborneHitCount;

        while (true)
        {
            yield return null;

            if (airborneHitCount >= 5 && PlayerManager.Instance.combat.juggleAttack >= 5)
                break;

            if (airborneHitCount != lastHitCount)
            {
                lastHitCount = airborneHitCount;
                noHitTimer = 0f;
            }
            else
            {
                noHitTimer += Time.deltaTime;
            }

            if (noHitTimer >= 1.5f)
                break;

            if (!PlayerManager.Instance.onAir)
                break;
        }

        // Allow natural falling
        rb.isKinematic = false;
        rb.useGravity = true;

        onAir = false;
        isBeingLaunched = false;
        launchRoutine = null;
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

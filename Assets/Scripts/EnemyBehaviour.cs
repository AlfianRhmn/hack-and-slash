using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("General - Statistics")]
    public string enemyName = "Enemy";
    public bool isBoss = false;
    public float currentHP;
    public float maxHP;
    [Header("General - AI")]
    public Transform target;
    public State[] enemyState;
    public Animator anim;
    public EnemyWeapon weapon;
    public GameObject warningFlash;
    private int currentState = 0;
    private NavMeshAgent agent;
    bool isChangingState = false;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool canBeParried = false;
    [HideInInspector] public bool bufferFrame = false;
    bool hasNoticedPlayer;
    bool readyToAttack = true;
    bool onAir = false;
    bool isDead = false;
    Rigidbody rb;
    private Coroutine launchRoutine;
    [HideInInspector] public bool isBeingLaunched = false;
    [HideInInspector] public EnemySpawner source;
    private int airborneHitCount = 0;
    [HideInInspector] public EnemyMoveset currentMoveset;
    [Header("User Interface")]
    public Slider healthBar;
    public Transform headOfModel;
    float currentVelocity;
    float peakY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!isBoss)
        {
            healthBar.maxValue = maxHP;
            healthBar.value = currentHP;
        }
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        PlayerManager.Instance.enemyList.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBoss)
        {
            SetupUI();
        }
        if (currentHP > 0)
        {
            CheckEnemyState();
            HandleMovement();
            if (Vector3.Distance(transform.position, target.position) < enemyState[currentState].distanceUntilAttack && !isAttacking && !isChangingState && hasNoticedPlayer && readyToAttack && !onAir && !PlayerManager.Instance.isDead)
            {
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
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
        Instantiate(warningFlash, headOfModel.position + transform.forward, Quaternion.identity);
        // masukin vfx ting! buat penanda serangan
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        if (!isChangingState)
        {
            anim.SetTrigger("Attack");
        }
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

    public IEnumerator StartBuffer(float frame)
    {
        bufferFrame = true;
        yield return new WaitForSeconds(frame);
        bufferFrame = false;
    }

    public void CheckHit() //USED IN ANIMATION EVENT
    {
        weapon.DoHit();
    }

    public void ReadyToParry() //USED IN ANIMATION EVENT
    {
        canBeParried = true;
        if (bufferFrame)
        {
            print("BUFFER TRIGGER");
            PlayerManager.Instance.combat.bufferParryDone++;
            PlayerManager.Instance.combat.ResetAllBufferFrame();
            StopCoroutine(StartBuffer(PlayerManager.Instance.combat.leniencyFrame));
            bufferFrame = false;
            PlayerManager.Instance.combat.Parry(transform);
        }
    }

    public void StopParry() // USED IN ANIMATION EVENT
    {
        canBeParried = false;
        if (bufferFrame)
        {
            StopCoroutine(StartBuffer(PlayerManager.Instance.combat.leniencyFrame));
            bufferFrame = false;
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
        if (Random.Range(0f, 1f) <= enemyState[currentState].dodgeChance)
        {
            // DODGE
            // masukin efek yang menunjukkan musuh dodging
            // seperti animasi, vfx, sound, dll.
        } else
        {
            currentHP -= damage;

            if (onAir)
            {
                StopCoroutine(ForceMoveUpward(5));
                StopCoroutine(PlayerManager.Instance.combat.ForceMoveUpward(3f));
                StartCoroutine(ForceMoveUpward(5));
                StartCoroutine(PlayerManager.Instance.combat.ForceMoveUpward(3f));
                airborneHitCount++;
            }

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
    }


    IEnumerator EnemyHit()
    {
        if (Random.Range(0f, 1f) <= enemyState[currentState].painTolerance && !onAir)
        {
            HandleAttack();
        }
        else
        {
            if (!isAttacking && !isChangingState)
            {
                anim.SetTrigger("Hit");
            }
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
        if (Random.Range(0, 2) == 1)
        {
            PlayerManager.Instance.combat.HealPlayer(Random.Range(5f, 20f));
        }
        yield return new WaitForSeconds(0.1f);
        if (isBoss)
        {
            source.BossDeath();
        } else
        {
            healthBar.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void StartLaunch(float duration, float height)
    {
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        launchRoutine = StartCoroutine(LaunchEnemy(duration, height));
    }

    public IEnumerator LaunchEnemy(float duration, float launchHeight)
    {
        if (isBoss)
            yield break;
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
        peakY = transform.position.y;
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
                if (!PlayerManager.Instance.combat.pauseJuggleTimer)
                {
                    noHitTimer = PlayerManager.Instance.combat.timer;
                }
            }

            if (noHitTimer >= 0.1f)
                break;

            if (!PlayerManager.Instance.onAir)
                break;
        }

        // Allow natural falling
        StopCoroutine(ForceMoveUpward(5f));
        rb.linearVelocity += Vector3.down * 200;
        rb.isKinematic = false;
        rb.useGravity = true;
        onAir = false;
        isBeingLaunched = false;
        launchRoutine = null;
    }
    public IEnumerator ForceMoveUpward(float initialVelocity)
    {
        float velocity = initialVelocity;
        float gravity = -9.81f;
        float startY = peakY;
        float positionY = startY;

        // Move until it comes back down to (or below) the starting position
        while (positionY >= startY || velocity > 0)
        {
            velocity += gravity * Time.deltaTime;
            positionY += velocity * Time.deltaTime;

            transform.position = new Vector3(transform.position.x, positionY, transform.position.z);

            yield return null;
        }

        // Snap back exactly to starting position
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
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
    [Range(0f, 1f)]
    public float dodgeChance; //dodge chance
}

using EasyTextEffects;
using SmallHedge.SoundManager;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering.Universal;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player References")]
    public PlayerManager manager;
    [Header("General - Statistics")]
    public float currentHealth;
    public float maxHealth = 100;
    public float currentMana;
    public float maxMana = 100;
    [Range(0, 100)]
    public int ultimateProgress;
    public List<StatusEffects> activeStatusEffect;
    public float attackModifier = 1;
    public float defenseModifier = 1;
    [Range(0, 100f)]
    public float critChance = 50f;
    public float critDamage = 50f;
    [Header("Combat")]
    public List<MovesetSO> moveset; //usable moveset, preset
    public List<Combo.attackTypes> playerAttacks = new List<Combo.attackTypes>();
    [SerializeField] int comboCounter;
    public List<SkillSO> listOfSpecial;
    public UltimateSO ultimate;
    public float timeUntilManaRegen = 2;
    [Range(0f, 1f)]
    public float percentageManaRegen = 0.1f;
    public float leniencyFrame = 0.05f;
    public float parryCooldown = 1.5f;
    public int weaponDurability = 4;
    public GameObject VFX_ModifierA;
    public GameObject VFX_ModifierB;
    public GameObject swordVFX;
    public GameObject impactVFX;
    public TextMeshProUGUI debug;
    int specialSelected;
    float healthVelocity;
    float manaVelocity;
    float timeLastUsedSpecial = 0;
    bool alreadyInputReady;
    bool isModifierA;
    bool isModifierB;
    MovesetSO lastMoveset;
    [HideInInspector] public int juggleAttack = 0;
    [HideInInspector] public float timer = 0; //used for juggling
    Vector3 originalPosition;
    Quaternion originalRotation;
    Transform lastEnemyHit;
    Coroutine attackCooldownCoroutine;
    Coroutine slowFrameDamage;
    [HideInInspector] public bool pauseJuggleTimer = false;
    GameObject dodgeNumber;
    float peakY;
    int perfectParryDone = 0;
    [HideInInspector] public int bufferParryDone = 0;
    float parryTimer;
    EnemyBehaviour[] listOfBuffer;
    EnemyBehaviour[] listOfParriable;
    int currentParry;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        manager.healthBar.maxValue = maxHealth;
        manager.manaBar.maxValue = maxMana;
        manager.healthBar.value = currentHealth;
        manager.manaBar.value = currentMana;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void Respawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        manager.deathCanvas.SetActive(false);
        manager.input.SwitchCurrentActionMap("Player");
        manager.virtualDeathCam.SetActive(false);
        manager.virtualHardLockCam.SetActive(false);
        manager.virtualThirdCam.SetActive(true);
        manager.movement.CancelLockOn();
        manager.gameCanvas.SetActive(true);
        manager.camCanvas.SetActive(true);
        manager.pauseCanvas.SetActive(true);
        currentHealth = maxHealth;
        currentMana = maxMana;
        manager.isDead = false;
        manager.readyToAttack = true;
        manager.readyToDodge = true;
        manager.readyToHurt = true;
        manager.readyToSpecial = true;
        manager.readyToUltimate = true;
        activeStatusEffect.Clear();
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        for (int i = 0; i < manager.gridStatus.childCount; i++)
        {
            Destroy(manager.gridStatus.GetChild(i).gameObject);
        }
        manager.anim.Play("Idle");
    }

    private void OnEnable()
    {
        manager.anim.Play("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (!manager.isDead)
        {
            SetupSpecial();
            SetupUltimate();
            EndAttack();
            debug.text = "last hit : " + timer.ToString() + "<br>player falling speed : " + (Mathf.Round(manager.rb.linearVelocity.y * 100) / 100) + "<br>timescale : " + Time.timeScale + "<br>parry timer : " + parryTimer + "<br>perfect parry : " + perfectParryDone + "<br>buffer parry : " + bufferParryDone + "<br>weapon durability : " + (weaponDurability - currentParry) + "/" + weaponDurability;
            if (!pauseJuggleTimer)
            {
                timer += Time.deltaTime;
            }

            if (!manager.onAir)
            {
                StopCoroutine(JuggleUp());
                juggleAttack = 0;
            }

            for (int i = 0; i < activeStatusEffect.Count; i++)
            {
                StatusEffects effect = activeStatusEffect[i];
                effect.duration -= Time.deltaTime;
                if (effect.type == StatusEffects.statusType.poison && currentHealth > 1)
                {
                    currentHealth -= effect.severity * Time.deltaTime;
                }
                if (effect.duration <= 0)
                {
                    switch (effect.type)
                    {
                        case StatusEffects.statusType.attack:
                            attackModifier -= effect.severity;
                            break;
                        case StatusEffects.statusType.defense:
                            defenseModifier -= effect.severity;
                            break;
                        case StatusEffects.statusType.critDMG:
                            critDamage -= effect.severity;
                            break;
                        case StatusEffects.statusType.critRate:
                            critChance -= effect.severity;
                            break;
                        case StatusEffects.statusType.poison:
                            break;
                    }
                    activeStatusEffect.Remove(effect);
                }
            }

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            if (currentMana > maxMana)
            {
                currentMana = maxMana;
            }

            timeLastUsedSpecial += Time.deltaTime;
            if (timeLastUsedSpecial > timeUntilManaRegen && currentMana < maxMana)
            {
                currentMana += (maxMana * percentageManaRegen) * Time.deltaTime;
                if (currentMana > maxMana)
                {
                    currentMana = maxMana;
                }
            }

            if (manager.healthBar.maxValue != maxHealth)
            {
                manager.healthBar.maxValue = maxHealth;
            }
            if (manager.manaBar.maxValue != maxMana)
            {
                manager.manaBar.maxValue = maxMana;
            }
        }

        float temp = Mathf.SmoothDamp(manager.healthBar.value, currentHealth, ref healthVelocity, 0.1f);
        manager.healthBar.value = temp;
        float tempX = Mathf.SmoothDamp(manager.manaBar.value, currentMana, ref manaVelocity, 0.1f);
        manager.manaBar.value = tempX;
    }

    public void SetupSpecial()
    {
        var special = listOfSpecial[specialSelected];

        // Set special icon and name
        manager.specialIcon.sprite = special.skillIcon;
        manager.specialName.text = $"{special.attackName}<br><size=15>[{special.manaCost} Energy]";

        // Inputs
        string inputText = manager.input.actions.FindAction("Special Attack").GetBindingDisplayString();
        inputText = inputText.Replace("Tap;action.interactions ", "");
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        manager.specialInput.text = "<sprite name=" + inputText + ">";
        inputText = manager.input.actions.FindAction("Change Special - Negative").GetBindingDisplayString();
        inputText = inputText.Replace("Tap;action.interactions ", "");
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        inputText = TurnToWord(inputText);
        manager.scrollLeftInput.text = "< <sprite name=" + inputText + ">";
        inputText = manager.input.actions.FindAction("Change Special - Positive").GetBindingDisplayString();
        inputText = inputText.Replace("Tap;action.interactions ", "");
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        inputText = TurnToWord(inputText);
        manager.scrollRightInput.text = "<sprite name=" + inputText + "> >";
    }

    string TurnToWord(string inputText)
    {
        switch (inputText)
        {
            case "0": return "zero";
            case "1": return "one";
            case "2": return "two";
            case "3": return "three";
            case "4": return "four";
            case "5": return "five";
            case "6": return "six";
            case "7": return "seven";
            case "8": return "eight";
            case "9": return "nine";
            default:
                return inputText;
        }
    }


    public void SetupUltimate()
    {
        manager.ultimateIcon.sprite = ultimate.skillIcon;
        manager.ultimateName.text = ultimate.attackName;
        if (ultimateProgress >= 100)
        {
            manager.ultimateProgress.text = "<color=yellow>READY";
        }
        else
        {
            string progressDisplayed = "";
            string progressEmptyDisplay = "";
            int amountOfPoint = ultimateProgress / 10;
            int amountOfEmpty = 10 - amountOfPoint;
            for (int i = 0; i < amountOfPoint; i++)
            {
                progressDisplayed += "O";
            }
            for (int i = 0; i < amountOfEmpty; i++)
            {
                progressEmptyDisplay += "O";
            }
            manager.ultimateProgress.text = "[ <color=yellow>" + progressDisplayed + "</color>" + progressEmptyDisplay + " ]";
        }
    }

    public void MoveInFrontOfEnemy(Transform enemy)
    {
        Vector3 targetPosition = enemy.position + enemy.forward * 1;
        StartCoroutine(MoveToPosition(targetPosition, 0.2f));
    }

    private IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target; // Ensure exact position at end
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToUltimate && manager.readyToSpecial && manager.readyToDodge && manager.readyToHurt && manager.enemyClose.Count > 0 && !manager.isParry && !manager.restrictParry)
        {
            listOfParriable = FindParryable();
            if (listOfParriable.Length > 0)
            {
                // try parrying
                print("PARRY SUCCESSFUL...");
                perfectParryDone++;
                Transform closest = FindClosestFromList(listOfParriable);
                Parry(closest);
            }
            else
            {
                listOfBuffer = FindAlmostAttack();
                if (listOfBuffer.Length > 0)
                {
                    foreach (EnemyBehaviour subject in listOfBuffer)
                    {
                        if (!subject.bufferFrame)
                        {
                            StartCoroutine(subject.StartBuffer(leniencyFrame));
                        }
                    }
                }
                else
                {
                    StartCoroutine(ParryCooldown()); // prevent spamming
                }
            }
        }
    }

    public void ResetAllBufferFrame()
    {
        foreach (EnemyBehaviour enemy in listOfBuffer)
        {
            StopCoroutine(enemy.StartBuffer(leniencyFrame));
            enemy.bufferFrame = false;
        }
        listOfBuffer = null;
    }

    IEnumerator ParryCooldown()
    {
        manager.restrictParry = true;
        yield return new WaitForSeconds(parryCooldown);
        manager.restrictParry = false;
    }

    public void Parry(Transform closest)
    {
        if (manager.isParry) return;
        manager.isParry = true;
        StartCoroutine(StartParrying(closest));
    }

    IEnumerator StartParrying(Transform closest)
    {
        currentParry = 0;
        manager.playerBody.LookAt(new Vector3(closest.position.x, transform.position.y, closest.position.z));
        MoveInFrontOfEnemy(closest);
        manager.anim.SetBool("Parrying", true);
        EnemyBehaviour enemy = closest.GetComponent<EnemyBehaviour>();
        parryTimer = 0;
        while (AllEnemyDoneAttack() || currentParry < weaponDurability)
        {
            // wait until all checks are finished
            // parriedattacks increase on TakeDamage()
            parryTimer += Time.deltaTime;
            if (parryTimer > 1.2f)
            {
                break;
            }
            if (currentParry >= weaponDurability)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.1f);
        if (currentParry < weaponDurability)
        {
            StartCoroutine(StartInvincible());
        } else
        {
            // play animation where player is repelled backward
            // add vfx to show durability is broken
            // add text where your weapon cant stand the attacks
            // no invincibility added
            print("WEAPON BROKEN THROUGH, PARRY STOPPED AND NO INVINCIBILITY GIVEN...");
        }
        listOfParriable = null;
        parryTimer = 0;
        currentParry = 0;
        manager.isParry = false;
        manager.anim.SetBool("Parrying", false);
        yield return new WaitForSeconds(0.4f);
        manager.virtualParryCam.SetActive(false);
    }

    public bool AllEnemyDoneAttack()
    {
        if (listOfParriable.Length > 0)
        {
            int i = 0;
            foreach (EnemyBehaviour enemy in listOfParriable)
            {
                if (enemy.isAttacking == false)
                {
                    i++;
                }
            }
            return i >= listOfParriable.Length;
        } else
        {
            return true;
        }
    }

    IEnumerator StartInvincible()
    {
        gameObject.tag = "Untagged"; // THIS IS CHEAP WAY FOR INVINCIBLE WITHOUT VARIABLE
        yield return new WaitForSeconds(1f);
        gameObject.tag = "Player";
    }

    EnemyBehaviour[] FindParryable()
    {
        List<EnemyBehaviour> result = new List<EnemyBehaviour>();
        for (int i = 0; i < manager.enemyClose.Count; i++)
        {
            if (Vector3.Distance(manager.enemyClose[i].transform.position, transform.position) < 8 && manager.enemyClose[i].canBeParried)
            {
                result.Add(manager.enemyClose[i]);
            }
        }
        return result.ToArray();
    }

    EnemyBehaviour[] FindAlmostAttack()
    {
        List<EnemyBehaviour> result = new List<EnemyBehaviour>();
        for (int i = 0; i < manager.enemyClose.Count; i++)
        {
            if (Vector3.Distance(manager.enemyClose[i].transform.position, transform.position) < 8 && manager.enemyClose[i].isAttacking)
            {
                result.Add(manager.enemyClose[i]);
            }
        }
        return result.ToArray();
    }

    public void OnChangeSpecialPos(InputAction.CallbackContext context)
    {
        if (context.performed && !manager.isDead)
        {
            specialSelected++;
            if (specialSelected >= listOfSpecial.Count)
            {
                specialSelected = 0;
            }
        }
    }

    public void OnChangeSpecialNeg(InputAction.CallbackContext context)
    {
        if (context.performed && !manager.isDead)
        {
            specialSelected--;
            if (specialSelected < 0)
            {
                specialSelected = listOfSpecial.Count - 1;
            }
        }
    }

    public void OnModifierA(InputAction.CallbackContext context)
    {
        if (context.performed && !manager.isDead)
        {
            VFX_ModifierA.SetActive(true);
            isModifierA = true;
        } else if (context.canceled)
        {
            VFX_ModifierA.SetActive(false);
            isModifierA = false;
        }
    }

    public void OnModifierB(InputAction.CallbackContext context)
    {
        if (context.performed && !manager.isDead)
        {
            VFX_ModifierB.SetActive(true);
            isModifierB = true;
        }
        else if (context.canceled)
        {
            VFX_ModifierB.SetActive(false);
            isModifierB = false;
        }
    }


    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            timer = 0;
            if (manager.readyToSpecial && manager.readyToUltimate && manager.readyToDodge && !manager.isDead && !manager.isParry)
            {
                if (isModifierA)
                {
                    if (context.interaction is TapInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedTapLightAttackA);
                    }
                    else if (context.interaction is HoldInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedHoldLightAttackA);
                    }
                }
                else if (isModifierB)
                {
                    if (context.interaction is TapInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedTapLightAttackB);
                    }
                    else if (context.interaction is HoldInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedHoldLightAttackB);
                    }
                }
                else
                {
                    if (context.interaction is TapInteraction)
                    {
                        HandleAttack(Combo.attackTypes.TapLightAttack);
                    }
                    else if (context.interaction is HoldInteraction)
                    {
                        HandleAttack(Combo.attackTypes.HoldLightAttack);
                    }
                }
            }
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            timer = 0;
            if (manager.readyToSpecial && manager.readyToUltimate && manager.readyToDodge && !manager.isDead && !manager.isParry)
            {
                if (isModifierA)
                {
                    if (context.interaction is TapInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedTapHeavyAttackA);
                    }
                    else if (context.interaction is HoldInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedHoldHeavyAttackA);
                    }
                }
                else if (isModifierB)
                {
                    if (context.interaction is TapInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedTapHeavyAttackB);
                    }
                    else if (context.interaction is HoldInteraction)
                    {
                        HandleAttack(Combo.attackTypes.ModifiedHoldHeavyAttackB);
                    }
                }
                else
                {
                    if (context.interaction is TapInteraction)
                    {
                        HandleAttack(Combo.attackTypes.TapHeavyAttack);
                    }
                    else if (context.interaction is HoldInteraction)
                    {
                        HandleAttack(Combo.attackTypes.HoldHeavyAttack);
                    }
                }
            }
        }
    }


    void HandleAttack(Combo.attackTypes input)
    {
        if (playerAttacks.Count >= moveset[0].comboList.Length)
        {
            playerAttacks.Clear();
            comboCounter = 0;
        }
        if (input == Combo.attackTypes.HoldLightAttack || input == Combo.attackTypes.HoldHeavyAttack || input == Combo.attackTypes.ModifiedHoldHeavyAttackA || input == Combo.attackTypes.ModifiedHoldLightAttackA || input == Combo.attackTypes.ModifiedHoldHeavyAttackB || input == Combo.attackTypes.ModifiedHoldLightAttackB)
        {
            if (!alreadyInputReady)
            {
                alreadyInputReady = true;
                StartCoroutine(ReadyToHold(input));
            }
        }
        CancelInvoke("EndAttack");
        CancelInvoke("EndCombo");
        if (lastMoveset != null)
        {
            if (comboCounter == 0)
            {
                if (!lastMoveset.skipAnimation && !manager.readyToAttack)
                {
                    return;
                }
            } else
            {
                if (!lastMoveset.skipAnimation && !manager.readyToAttack)
                {
                    return;
                } else if (lastMoveset.skipAnimation && input == lastMoveset.comboList[comboCounter - 1].keyUsed && !manager.readyToAttack)
                {
                    return;
                }
            }
        } else
        {
            if (!manager.readyToAttack) return;
        }
        // Add to combo input buffer
        playerAttacks.Add(input);

        MovesetSO move = CheckMoveset();
        if (move == null || comboCounter >= move.comboList.Length)
        {
            if (comboCounter == 0)
            {
                print("Invalid button, " + input + " doesn't exist in moveset.");
                playerAttacks.Clear();
                comboCounter = 0;
                return;
            }
            playerAttacks.Clear();
            comboCounter = 0;
            HandleAttack(input);
            return;
        }
        lastMoveset = move;

        var attack = move.comboList[comboCounter].attackUsed;

        if ((move.isAirAttack && manager.onAir) || (!move.isAirAttack && !manager.onAir))
        {
            manager.readyToAttack = false;
            if (!move.isAirAttack)
            {
                foreach (var push in attack.movementDone)
                {
                    StartCoroutine(PushingPlayerCount(push));
                }
            }

            manager.anim.runtimeAnimatorController = attack.animOV;
            Transform enemy = FindClosestEnemy();
            if (enemy != null)
            {
                if (Vector3.Distance(enemy.position, manager.playerBody.position) < 7)
                {
                    manager.playerBody.LookAt(new Vector3(enemy.position.x, manager.playerBody.position.y, enemy.position.z));
                }
            }
            manager.anim.SetTrigger("Basic Attack");
            swordVFX.SetActive(true);
            manager.anim.Update(0f);
            manager.weapon.damage = attack.damage * attackModifier;
            manager.weapon.critChance = critChance;
            manager.weapon.critDamage = critDamage;
            manager.rightLeg.damage = attack.damage * attackModifier;
            manager.rightLeg.critChance = critChance;
            manager.rightLeg.critDamage = critDamage;
            if (manager.onAir)
            {
                juggleAttack++;
            }
            foreach (SpecialEffects effect in attack.addEffects)
            {
                switch (effect.specialEffect)
                {
                    case SpecialEffects.Effects.JuggleUp:
                        var obj = Instantiate(effect.specialObject, manager.frontOfBody.position, Quaternion.identity);
                        StartCoroutine(JuggleUp());
                        break;
                    case SpecialEffects.Effects.Knockback:
                        break;
                    case SpecialEffects.Effects.UseUltimate:
                        if (ultimateProgress >= 100)
                        {
                            manager.readyToUltimate = false;
                            manager.ultCamera.SetActive(true);
                            manager.ultCanvas.SetActive(true);
                            StartCoroutine(UltimateInitiation());
                        }
                        break;
                }
            }
            comboCounter++;
            if (attackCooldownCoroutine != null)
            {
                StopCoroutine(attackCooldownCoroutine);
            }
            attackCooldownCoroutine = StartCoroutine(WaitForAnotherAttack(attack.timeToNextAnim));

        }
    }

    IEnumerator ReadyToHold(Combo.attackTypes input)
    {
        yield return new WaitUntil(() => manager.readyToAttack);
        HandleAttack(input);
        alreadyInputReady = false;
    }

    IEnumerator JuggleUp()
    {
        yield return new WaitUntil(() => manager.rb.linearVelocity.y < -0.1f);
        peakY = transform.position.y;
        if (lastEnemyHit != null && !manager.virtualHardLockCam.activeSelf)
        {
            manager.jugglePoint.position = Vector3.Lerp(manager.playerBody.position, lastEnemyHit.position, 0.5f);
            manager.virtualJuggleCam.SetActive(true);
        }
        juggleAttack = 0;
        if (timer <= 0.4f)
        {
            manager.rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            manager.rb.linearVelocity = Vector3.zero;
        }
        yield return new WaitUntil(() => juggleAttack > 5 || !manager.onAir || timer >= 0.1f || !CheckEnemyOnAir());
        manager.virtualJuggleCam.SetActive(false);
        print("Juggle Canceled - Attack : " + juggleAttack + ", OnAir : " + manager.onAir + ", Timer : " + timer + " Enemy OnAir : " + CheckEnemyOnAir());
        manager.rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        manager.rb.AddForce(Vector3.down * 200);
    }

    bool CheckEnemyOnAir()
    {
        for (int i = 0; i < manager.enemyClose.Count; i++)
        {
            if (manager.enemyClose[i].isBeingLaunched)
            {
                return true;
            }
        }
        return false;
    }

    MovesetSO CheckMoveset()
    {
        MovesetSO bestMatch = null;
        int longestMatch = 0;

        foreach (var m in moveset)
        {
            if (m.isAirAttack != manager.onAir)
                continue;

            if (playerAttacks.Count > m.comboList.Length)
                continue;

            bool isMatch = true;
            for (int i = 0; i < playerAttacks.Count; i++)
            {
                if (m.comboList[i].keyUsed != playerAttacks[i])
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch && playerAttacks.Count > longestMatch)
            {
                bestMatch = m;
                longestMatch = playerAttacks.Count;
            }
        }

        return bestMatch;
    }


    public void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToSpecial && currentMana >= listOfSpecial[specialSelected].manaCost && manager.readyToDodge && manager.readyToUltimate && !manager.onAir && !manager.isDead && !manager.isParry)
        {
            timeLastUsedSpecial = 0;
            currentMana -= listOfSpecial[specialSelected].manaCost;
            manager.readyToSpecial = false;
            SkillSO specialUsed = listOfSpecial[specialSelected];
            for (int i = 0; i < specialUsed.soundUsed.Length; i++)
            {
                StartCoroutine(StartSounds(specialUsed.soundUsed[i]));
            }
            manager.anim.runtimeAnimatorController = specialUsed.animOV;
            manager.anim.SetTrigger("Special Attack");
            StartCoroutine(WaitForSpecial(specialUsed.duration, specialUsed.vfx));
            for (int i = 0; i < specialUsed.skillType.Length; i++)
            {
                switch (specialUsed.skillType[i])
                {
                    case SkillSO.typeOfSkill.Fireball:
                        StartCoroutine(SpawnFireball(specialUsed, i));
                        break;
                    case SkillSO.typeOfSkill.GiveStatus:
                        StartCoroutine(GiveStatus(specialUsed.status, specialUsed.timeBeforeApply));
                        break;
                    case SkillSO.typeOfSkill.Heal:
                        HealPlayer(specialUsed.heal);
                        break;
                    case SkillSO.typeOfSkill.Quake:
                        StartCoroutine(SpawnFireball(specialUsed, i));
                        break;
                }
            }
        }
    }

    public void HealPlayer(float amount)
    {
        //add vfx for heal n shit
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void ResetTimer()
    {
        timer = 0;
    }

    IEnumerator StartSounds(Sounds sound)
    {
        yield return new WaitForSeconds(sound.time);
        SoundManager.PlaySound(sound.type);
    }

    IEnumerator WaitForSpecial(float length, GameObject vfeffect)
    {
        if (vfeffect != null)
        {
            GameObject vfx = Instantiate(vfeffect, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(length);
            Destroy(vfx);
        }
        else
        {
            yield return new WaitForSeconds(length);
        }
            manager.readyToSpecial = true;
    }

    public void OnUltimate(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToUltimate && ultimateProgress >= 100 && manager.readyToDodge && manager.readyToSpecial && manager.readyToAttack && !manager.isDead && !manager.isParry)
        {
            manager.readyToUltimate = false;
            manager.ultCamera.SetActive(true);
            manager.ultCanvas.SetActive(true);
            StartCoroutine(UltimateInitiation());
        }
    }


    IEnumerator UltimateInitiation()
    {
        manager.readyToUltimate = false;
        yield return new WaitForSeconds(ultimate.waitingUltimateInitiation);
        manager.ultCanvas.SetActive(false);
        manager.ultCamera.SetActive(false);
        if (ultimate.movePlayer.Length > 0)
        {
            for (int i = 0; i < ultimate.movePlayer.Length; i++)
            {
                Transform enemy = FindClosestEnemy();
                if (enemy != null)
                {
                    manager.playerBody.LookAt(new Vector3(enemy.position.x, manager.playerBody.position.y, enemy.position.z));
                }
                StartCoroutine(PushingPlayerCount(ultimate.movePlayer[i]));
            }
        }
        CancelInvoke("EndCombo");
        CancelInvoke("EndAttack");
        ultimateProgress = 0;
        manager.anim.runtimeAnimatorController = ultimate.animOV;
        manager.anim.SetTrigger("Ultimate");
        manager.weapon.damage = ultimate.damage * attackModifier;
        manager.weapon.critChance = critChance;
        manager.weapon.critDamage = critDamage;
        manager.rightLeg.damage = ultimate.damage * attackModifier;
        manager.rightLeg.critChance = critChance;
        manager.rightLeg.critDamage = critDamage;
        yield return new WaitForSeconds(0.25f);
        yield return new WaitForSeconds(manager.anim.GetCurrentAnimatorStateInfo(0).length);
        manager.readyToUltimate = true;
    }

    IEnumerator PushingPlayerCount(Movement movement)
    {
        yield return new WaitForSeconds(movement.timeBeforeMoving);
        PushPlayer(movement.amountToMove, movement.moveDirection);
    }

    IEnumerator SpawnFireball(SkillSO specialUsed, int skillTypeIndex)
    {
        Transform enemyTransform;
        if (manager.currentLockOnTarget != null)
        {
            enemyTransform = manager.currentLockOnTarget;
        }
        else
        {
            enemyTransform = FindClosestEnemy();
        }
        if (enemyTransform != null)
        {
            manager.playerBody.transform.LookAt(new Vector3(enemyTransform.position.x, manager.playerBody.position.y, enemyTransform.position.z));
        }
        yield return new WaitForSeconds(specialUsed.timeBeforeApply);
        GameObject obj = Instantiate(specialUsed.projectile, manager.rightHand.position, Quaternion.identity);
        Projectile fireball = obj.GetComponent<Projectile>();
        if (fireball != null)
        {
            fireball.damageNumber = manager.damageNumber;
            fireball.damage = specialUsed.damage * attackModifier;
            fireball.critChance = critChance;
            fireball.critDamage = critDamage;
            if (specialUsed.skillType[skillTypeIndex] == SkillSO.typeOfSkill.Quake)
            {
                fireball.transform.position = manager.playerBody.position;
                fireball.effectID = 1;
            }
            if (enemyTransform == null)
            {
                fireball.GetComponent<Rigidbody>().AddForce(manager.playerBody.forward * specialUsed.velocity, ForceMode.Impulse);
            }
            else
            {
                fireball.GetComponent<Rigidbody>().AddForce((enemyTransform.position - manager.playerBody.position).normalized * specialUsed.velocity, ForceMode.Impulse);
            }
        }
    }

    public void PushPlayer(float amountOfPush, Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            direction = manager.playerBody.forward;
        }
        manager.rb.AddForce(direction * amountOfPush, ForceMode.Impulse);
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

    public Transform FindClosestFromList(EnemyBehaviour[] list)
    {
        if (list == null || list.Length == 0) return null;

        float closestSqr = Mathf.Infinity;
        int closestIndex = -1;
        Vector3 pos = transform.position;

        for (int i = 0; i < list.Length; i++)
        {
            var enemy = list[i];
            if (enemy == null) continue; // skip dead/null entries

            float sqr = (enemy.transform.position - pos).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closestIndex = i;
            }
        }

        if (closestIndex == -1) return null;
        return list[closestIndex].transform;
    }


    public Transform FindClosestEnemy()
    {
        if (manager.enemyList.Count == 0)
        {
            return null;
        }
        float closestDistance = Mathf.Infinity;
        int enemyIndex = 0;
        foreach (EnemyBehaviour enemy in manager.enemyList)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < closestDistance)
            {
                closestDistance = Vector3.Distance(transform.position, enemy.transform.position);
                enemyIndex = manager.enemyList.IndexOf(enemy);
            }
        }
        return manager.enemyList[enemyIndex].transform;
    }

    IEnumerator GiveStatus(StatusEffects[] statuses, float time)
    {
        yield return new WaitForSeconds(time);
        foreach (StatusEffects statusEffect in statuses)
        {
            StatusEffects newStatus = new StatusEffects();
            newStatus = statusEffect.Copy();
            var existing = activeStatusEffect.Find(s => s.type == statusEffect.type);
            if (existing != null)
            {
                existing.duration = statusEffect.duration;
            }
            else
            {
                activeStatusEffect.Add(newStatus);
                switch (newStatus.type)
                {
                    case StatusEffects.statusType.attack:
                        attackModifier += newStatus.severity;
                        break;
                    case StatusEffects.statusType.defense:
                        defenseModifier += newStatus.severity;
                        break;
                    case StatusEffects.statusType.critDMG:
                        critDamage += newStatus.severity;
                        break;
                    case StatusEffects.statusType.critRate:
                        critChance += newStatus.severity;
                        break;
                    case StatusEffects.statusType.poison:
                        break;

                }
                StatusEffectUI game = Instantiate(manager.statusDisplay, manager.gridStatus).GetComponent<StatusEffectUI>();
                game.Initialize(newStatus);
            }
        }
    }

    public void SetLastHit(Transform enemy)
    {
        lastEnemyHit = enemy;
    }

    public void Reset()
    {
        manager.readyToAttack = true;
        Invoke("EndCombo", 1);
    }
    IEnumerator WaitForAnotherAttack(float waiting)
    {
        pauseJuggleTimer = true;
        yield return new WaitForSeconds(waiting);
        swordVFX.SetActive(false);
        manager.readyToAttack = true;
        Invoke("EndCombo", 1);
        yield return new WaitForSeconds(0.2f);
        pauseJuggleTimer = false;
    }

    void EndAttack()
    {
        AnimatorStateInfo anim = manager.anim.GetCurrentAnimatorStateInfo(0);

        if (anim.normalizedTime >= 1f && anim.IsTag("Basic Attack"))
        {
            Invoke("EndCombo", 1);
        }
    }

    private void OnValidate()
    {
        attackModifier = Mathf.Max(attackModifier, 0);
        defenseModifier = Mathf.Max(defenseModifier, 0.001f);
    }

    public void EndCombo()
    {
        playerAttacks.Clear();
        manager.readyToAttack = true;
        comboCounter = 0;
    }

    public void TakeDamage(float damage, Transform sourceOfDamage)
    {
        if (manager.readyToHurt && manager.readyToUltimate)
        {
            if (manager.invulnerability)
            {
                if (dodgeNumber == null)
                {
                    AlwaysLookAt look = manager.damageNumber.GetObject().GetComponent<AlwaysLookAt>();
                    dodgeNumber = look.gameObject;
                    look.sourceOfPool = manager.damageNumber;
                    look.transform.position = manager.playerBody.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 3f), UnityEngine.Random.Range(-1f, 1f));
                    look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().text = "DODGE";
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.cyan;
                    look.transform.GetChild(0).GetComponent<TextEffect>().Refresh();
                    StartCoroutine(DodgeNumberRefresh());
                    if (slowFrameDamage != null)
                    {
                        StopCoroutine(slowFrameDamage);
                        if (manager.succesfulDodgeSettings.TryGet(out ColorAdjustments adjust))
                        {
                            adjust.saturation.value = 0f; // back to normal immediately
                        }
                    }
                    slowFrameDamage = StartCoroutine(SlowFrameDodge());
                }
            } 
            else if (manager.isParry)
            {
                parryTimer = 0;
                currentParry++;
                manager.virtualParryCam.SetActive(true);
                StartCoroutine(StartImpactVFX());
                manager.playerBody.transform.LookAt(new Vector3(sourceOfDamage.position.x, manager.playerBody.transform.position.y, sourceOfDamage.position.z));
                manager.anim.SetTrigger("AltParry");
                if (slowFrameDamage != null)
                {
                    StopCoroutine(slowFrameDamage);
                    if (manager.succesfulDodgeSettings.TryGet(out ColorAdjustments adjust))
                    {
                        adjust.saturation.value = 0f; // back to normal immediately
                    }
                }
                slowFrameDamage = StartCoroutine(SlowFrameParry());
            } 
            else
            {
                manager.restrictParry = true;
                manager.readyToHurt = false;
                damage /= defenseModifier;
                if (Gamepad.current != null)
                {
                    Gamepad.current.SetMotorSpeeds(1f, 1f);
                }
                if (damage < 1)
                {
                    damage = 1;
                }
                currentHealth -= damage;
                manager.rb.linearVelocity = new Vector3(0, manager.rb.linearVelocity.y, 0);
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    manager.readyToAttack = false;
                    manager.readyToDodge = false;
                    manager.readyToSpecial = false;
                    manager.readyToUltimate = false;
                    manager.isDead = true;
                    manager.rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    manager.playerBody.transform.LookAt(new Vector3(sourceOfDamage.position.x, manager.playerBody.transform.position.y, sourceOfDamage.position.z));
                    if (!manager.onAir)
                    {
                        manager.rb.AddForce(manager.playerBody.forward * -10, ForceMode.Impulse);
                    }
                    AlwaysLookAt look = manager.damageNumber.GetObject().GetComponent<AlwaysLookAt>();
                    look.sourceOfPool = manager.damageNumber;
                    look.transform.position = manager.playerBody.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 3f), UnityEngine.Random.Range(-1f, 1f));
                    look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().text = "DEAD";
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.red;
                    look.transform.GetChild(0).GetComponent<TextEffect>().Refresh();
                    manager.anim.SetTrigger("Dead");
                    StopAllCoroutines();
                    StartCoroutine(StartDead());
                    // RUN DEATH SEQUENCE
                }
                else
                {
                    manager.rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    StopCoroutine(manager.movement.DodgeCooldown());
                    manager.readyToDodge = false;
                    manager.playerBody.transform.LookAt(new Vector3(sourceOfDamage.position.x, manager.playerBody.transform.position.y, sourceOfDamage.position.z));
                    if (!manager.onAir)
                    {
                        manager.rb.AddForce(manager.playerBody.forward * -10, ForceMode.Impulse);
                    }
                    AlwaysLookAt look = manager.damageNumber.GetObject().GetComponent<AlwaysLookAt>();
                    look.sourceOfPool = manager.damageNumber;
                    look.transform.position = manager.playerBody.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, 3f), UnityEngine.Random.Range(-1f, 1f));
                    look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().text = Mathf.RoundToInt(damage).ToString();
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.red;
                    look.transform.GetChild(0).GetComponent<TextEffect>().Refresh();
                    if (manager.readyToUltimate && !manager.onAir)
                    {
                        manager.anim.SetTrigger("Hit");
                        if (!manager.readyToSpecial)
                        {
                            StopCoroutine(GiveStatus(listOfSpecial[specialSelected].status, listOfSpecial[specialSelected].timeBeforeApply));
                            for (int i = 0; i < listOfSpecial[specialSelected].skillType.Length; i++)
                            {
                                StopCoroutine(SpawnFireball(listOfSpecial[specialSelected], i));
                            }
                        }
                    }
                    StartCoroutine(DamageCooldown());
                    //PLAY HIT ANIMATION
                }
            }
        }
    }

    IEnumerator StartImpactVFX()
    {
        SoundManager.PlaySoundIndex(10);
        GameObject obj = Instantiate(impactVFX, manager.weapon.transform, false);
        obj.transform.localScale = Vector3.one * 3;
        yield return new WaitForSeconds(1f);
        Destroy(obj);
    }

    IEnumerator DodgeNumberRefresh()
    {
        yield return new WaitForSeconds(2);
        dodgeNumber = null;
    }

    IEnumerator SlowFrameDodge()
    {
        Time.timeScale = 0.1f;

        if (manager.succesfulDodgeSettings.TryGet(out ColorAdjustments adjust))
        {
            float startSat = adjust.saturation.value;
            float graySat = -100f; // fully gray

            // Fade to gray
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.3f; // 0.3s fade time
                adjust.saturation.value = Mathf.Lerp(startSat, graySat, t);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.3f); // stay gray

            // Fade back to normal
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.3f; // same duration back
                adjust.saturation.value = Mathf.Lerp(graySat, startSat, t);
                yield return null;
            }

            adjust.saturation.value = startSat; // ensure exact reset
        }

        Time.timeScale = 1f;
    }

    IEnumerator SlowFrameParry()
    {
        Time.timeScale = 0.5f;

        if (manager.succesfulDodgeSettings.TryGet(out ColorAdjustments adjust))
        {
            float startSat = adjust.saturation.value;
            float graySat = -100f; // fully gray

            // Fade to gray
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.3f; // 0.3s fade time
                adjust.saturation.value = Mathf.Lerp(startSat, graySat, t);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.3f); // stay gray

            // Fade back to normal
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.3f; // same duration back
                adjust.saturation.value = Mathf.Lerp(graySat, startSat, t);
                yield return null;
            }

            adjust.saturation.value = startSat; // ensure exact reset
        }

        Time.timeScale = 1f;
    }

    IEnumerator StartDead()
    {
        manager.virtualDeathCam.SetActive(true);
        manager.virtualHardLockCam.SetActive(false);
        manager.virtualThirdCam.SetActive(false);
        if (manager.deathVolumeSettings.TryGet<ColorAdjustments>(out ColorAdjustments colorAdjustments))
        {
            float currVelocity = 0;
            colorAdjustments.saturation.value = 0f;
            while (colorAdjustments.saturation.value > -98f)
            {
                colorAdjustments.saturation.value = Mathf.SmoothDamp(colorAdjustments.saturation.value, -100f, ref currVelocity, 0.1f);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForSeconds(1f);
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
        manager.deathCanvas.SetActive(true);
        foreach (InputDevice device in manager.input.devices)
        {
            if (device is Gamepad)
            {
                manager.eventSystem.SetSelectedGameObject(manager.deathHighlight.gameObject);
            }
        }
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        manager.input.SwitchCurrentActionMap("UI");
    }

    IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(0.05f);
        manager.readyToHurt = true;
        manager.readyToAttack = true;
        manager.readyToDodge = true;
        manager.readyToSpecial = true;
        yield return new WaitForSeconds(0.5f);
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
        manager.restrictParry = false;
    }
}
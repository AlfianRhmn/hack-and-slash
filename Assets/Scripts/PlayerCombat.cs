using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;

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
    public PlayerInput input;
    public float timeUntilManaRegen = 2;
    [Range(0f, 1f)]
    public float percentageManaRegen = 0.1f;
    float lastClickedTime;
    float lastComboEnd;
    int specialSelected;
    float healthVelocity;
    float manaVelocity;
    float timeLastUsedSpecial = 0;
    float lightPressTime;
    bool lightAttackTriggered;
    float heavyPressTime;
    bool heavyAttackTriggered;
    bool alreadyInputReady;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        manager.healthBar.maxValue = maxHealth;
        manager.manaBar.maxValue = maxMana;
        manager.healthBar.value = currentHealth;
        manager.manaBar.value = currentMana;
    }

    // Update is called once per frame
    void Update()
    {
        SetupSpecial();
        SetupUltimate();
        EndAttack();

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

        if (manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Special Attack"))
        {
            manager.readyToSpecial = true;
        }
        if (manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Ultimate"))
        {
            manager.readyToUltimate = true;
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
        string inputText = input.actions.FindAction("Special Attack").GetBindingDisplayString();
        inputText = inputText.Replace("Tap;action.interactions ", "");
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        manager.specialInput.text = "<sprite name=" + inputText + ">";
        inputText = input.actions.FindAction("Change Special - Negative").GetBindingDisplayString();
        inputText = inputText.Replace("Tap;action.interactions ", "");
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        inputText = TurnToWord(inputText);
        manager.scrollLeftInput.text = "< <sprite name=" + inputText + ">";
        inputText = input.actions.FindAction("Change Special - Positive").GetBindingDisplayString();
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
        string inputText = input.actions.FindAction("Ultimate").GetBindingDisplayString();
        inputText = inputText.Replace("Tap;action.interactions ", "");
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        manager.ultimateInput.text = "<sprite name=" + inputText + ">";
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
                progressDisplayed += "■";
            }
            for (int i = 0; i < amountOfEmpty; i++)
            {
                progressEmptyDisplay += "■";
            }
            manager.ultimateProgress.text = "[ <color=yellow>" + progressDisplayed + "</color>" + progressEmptyDisplay + " ]";
        }
    }

    public void OnChangeSpecialPos(InputAction.CallbackContext context)
    {
        if (context.performed)
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
        if (context.performed)
        {
            specialSelected--;
            if (specialSelected < 0)
            {
                specialSelected = listOfSpecial.Count - 1;
            }
        }
    }


    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (manager.readyToSpecial && manager.readyToUltimate && manager.readyToDodge)
        {
            if (context.performed && context.interaction is TapInteraction)
            {
                HandleAttack(Combo.attackTypes.TapLightAttack);
            }
            else if (context.performed && context.interaction is HoldInteraction)
            {
                HandleAttack(Combo.attackTypes.HoldLightAttack);
            }
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (manager.readyToSpecial && manager.readyToUltimate && manager.readyToDodge)
        {
            if (context.performed && context.interaction is TapInteraction)
            {
                HandleAttack(Combo.attackTypes.TapHeavyAttack);
            }
            else if (context.performed && context.interaction is HoldInteraction)
            {
                HandleAttack(Combo.attackTypes.HoldHeavyAttack);
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
        if (input == Combo.attackTypes.HoldLightAttack || input == Combo.attackTypes.HoldHeavyAttack)
        {
            if (!alreadyInputReady)
            {
                alreadyInputReady = true;
                StartCoroutine(ReadyToHold(input));
            }
        }
        CancelInvoke("EndAttack");
        CancelInvoke("EndCombo");
        if (!manager.readyToAttack) return;
        // Add to combo input buffer
        playerAttacks.Add(input);

        MovesetSO move = CheckMoveset();
        if (move == null || comboCounter >= move.comboList.Length)
        {
            playerAttacks.Clear();
            comboCounter = 0;
            HandleAttack(input);
            return;
        }

        var attack = move.comboList[comboCounter].attackUsed;

        manager.readyToAttack = false;
        manager.weapon.repeatingDamage = true;

        manager.weapon.EnableHitbox();
        foreach (var push in attack.movementDone)
        {
            StartCoroutine(PushingPlayerCount(push));
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
        manager.anim.Update(0f);
        manager.weapon.damage = attack.damage * attackModifier;
        manager.weapon.critChance = critChance;
        manager.weapon.critDamage = critDamage;

        lastClickedTime = Time.time;
        comboCounter++;
        StartCoroutine(WaitForAnotherAttack(attack.timeToNextAnim));

    }

    IEnumerator ReadyToHold(Combo.attackTypes input)
    {
        yield return new WaitUntil(() => manager.readyToAttack);
        HandleAttack(input);
        alreadyInputReady = false;
    }

    MovesetSO CheckMoveset()
    {
        MovesetSO bestMatch = null;
        int longestMatch = 0;

        foreach (var m in moveset)
        {
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
        if (context.performed && manager.readyToSpecial && currentMana >= listOfSpecial[specialSelected].manaCost && manager.readyToDodge && manager.readyToUltimate)
        {
            manager.weapon.repeatingDamage = false;
            timeLastUsedSpecial = 0;
            currentMana -= listOfSpecial[specialSelected].manaCost;
            manager.readyToSpecial = false;
            SkillSO specialUsed = listOfSpecial[specialSelected];
            manager.anim.runtimeAnimatorController = specialUsed.animOV;
            manager.anim.SetTrigger("Special Attack");
            for (int i = 0; i < specialUsed.skillType.Length; i++)
            {
                switch (specialUsed.skillType[i])
                {
                    case SkillSO.typeOfSkill.Fireball:
                        StartCoroutine(SpawnFireball(specialUsed));
                        break;
                    case SkillSO.typeOfSkill.GiveStatus:
                        StartCoroutine(GiveStatus(specialUsed.status, specialUsed.timeBeforeApply));
                        break;
                    case SkillSO.typeOfSkill.Heal:
                        currentHealth += specialUsed.heal;
                        break;
                    case SkillSO.typeOfSkill.Quake:
                        StartCoroutine(SpawnFireball(specialUsed));
                        break;
                }
            }
        }
    }

    public void OnUltimate(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToUltimate && ultimateProgress >= 100 && manager.readyToDodge && manager.readyToSpecial && manager.readyToAttack)
        {
            manager.readyToUltimate = false;
            manager.ultCamera.SetActive(true);
            manager.ultCanvas.SetActive(true);
            StartCoroutine(UltimateInitiation());
        }
    }


    IEnumerator UltimateInitiation()
    {
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
        manager.weapon.repeatingDamage = true;
        CancelInvoke("EndCombo");
        CancelInvoke("EndAttack");
        manager.readyToUltimate = false;
        ultimateProgress = 0;
        manager.anim.runtimeAnimatorController = ultimate.animOV;
        manager.anim.SetTrigger("Ultimate");
        manager.weapon.damage = ultimate.damage * attackModifier;
        manager.weapon.critChance = critChance;
        manager.weapon.critDamage = critDamage;
        manager.weapon.EnableHitbox();
    }

    IEnumerator PushingPlayerCount(Movement movement)
    {
        yield return new WaitForSeconds(movement.timeBeforeMoving);
        PushPlayer(movement.amountToMove, movement.moveDirection);
    }

    IEnumerator SpawnFireball(SkillSO specialUsed)
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
            if (enemyTransform == null)
            {
                fireball.GetComponent<Rigidbody>().AddForce(manager.playerBody.forward * specialUsed.velocity, ForceMode.Impulse);
            }
            else
            {
                fireball.GetComponent<Rigidbody>().AddForce((enemyTransform.position - manager.playerBody.position).normalized * specialUsed.velocity, ForceMode.Impulse);
            }
        }
        Weapon quake = obj.GetComponent<Weapon>();
        if (quake != null)
        {
            obj.transform.position = transform.position;
            quake.effectID = 1;
            quake.damage = specialUsed.damage * attackModifier;
            quake.critChance = critChance;
            quake.critDamage = critDamage;
            quake.damageNumber = manager.damageNumber;
            yield return new WaitForEndOfFrame();
            quake.EnableHitbox();
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

    public Transform FindClosestEnemy()
    {
        if (manager.enemyList.Count == 0)
        {
            return null;
        }
        float closestDistance = Mathf.Infinity;
        int enemyIndex = 0;
        foreach (Balmond enemy in manager.enemyList)
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

    public void Reset()
    {
        manager.readyToAttack = true;
        Invoke("EndCombo", 1);
    }
    IEnumerator WaitForAnotherAttack(float waiting)
    {
        yield return new WaitForSeconds(waiting);
        manager.readyToAttack = true;
        Invoke("EndCombo", 1);
    }

    void EndAttack()
    {
        AnimatorStateInfo anim = manager.anim.GetCurrentAnimatorStateInfo(0);

        if (anim.normalizedTime >= 1f && anim.IsTag("Basic Attack"))
        {
            Invoke("EndCombo", 1);
        }
    }



    public void EndCombo()
    {
        manager.weapon.DisableHitbox();
        playerAttacks.Clear();
        manager.readyToAttack = true;
        comboCounter = 0;
        lastComboEnd = Time.time;
    }

    public void TakeDamage(float damage, Transform sourceOfDamage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // RUN DEATH SEQUENCE
        }
        else
        {
            StopCoroutine(manager.movement.DodgeCooldown());
            manager.readyToDodge = false;
            manager.playerBody.transform.LookAt(new Vector3(sourceOfDamage.position.x, manager.playerBody.transform.position.y, sourceOfDamage.position.z));
            manager.anim.SetTrigger("Hit");
            //PLAY HIT ANIMATION
        }
    }

    internal void TakeDamage(float damageToDeal)
    {
        throw new NotImplementedException();
    }

    internal static void TakeDamage()
    {
        throw new NotImplementedException();
    }
}
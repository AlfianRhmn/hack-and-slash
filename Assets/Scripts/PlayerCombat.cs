using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player References")]
    public PlayerManager manager;
    [Header("General - Statistics")]
    public float currentHealth;
    public float maxHealth = 100;
    public float currentMana;
    public float maxMana = 100;
    public List<StatusEffects> activeStatusEffect;
    public float attackModifier = 1;
    public float defenseModifier = 1;
    [Range(0, 100f)]
    public float critChance = 50f;
    public float critDamage = 50f;
    [Header("Combat")]
    public List<AttackSO> combo;
    public List<AttackSO> listOfSpecial;
    public PlayerInput input;
    public float timeUntilManaRegen = 2;
    [Range(0f, 1f)]
    public float percentageManaRegen = 0.1f;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    int specialSelected;
    float healthVelocity;
    float manaVelocity;
    float timeLastUsedSpecial = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        manager.healthBar.maxValue = maxHealth;
        manager.manaBar.maxValue = maxMana;
        manager.healthBar.value = currentHealth;
        manager.manaBar.value = currentMana;
        SetupSpecial();
    }

    // Update is called once per frame
    void Update()
    {
        SetupSpecial();
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
        manager.specialIcon.sprite = listOfSpecial[specialSelected].skillIcon;
        manager.specialName.text = listOfSpecial[specialSelected].attackName + "<br><size=15>[" + listOfSpecial[specialSelected].manaCost + " Energy]";
        string inputText = input.actions.FindAction("Special Attack").GetBindingDisplayString();
        inputText = inputText.Replace("Tap ", "");
        inputText = inputText.Replace("Hold ", "");
        inputText = inputText.Replace("Multi Tap ", "");
        inputText = inputText.Replace("Press ", "");
        inputText = inputText.Replace("Slow Tap ", "");
        manager.specialInput.text = "<sprite name=" + inputText + ">";
    }

    public void OnChangeSpecial(InputAction.CallbackContext context)
    {
        float x = context.ReadValue<float>();
        if (x != 0)
        {
            if (x < 0)
            {
                specialSelected--;
                if (specialSelected < 0)
                {
                    specialSelected = listOfSpecial.Count - 1;
                }
            } else
            {
                specialSelected++;
                if (specialSelected >= listOfSpecial.Count)
                {
                    specialSelected = 0;
                }
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.time - lastComboEnd > 0.2f && comboCounter <= combo.Count)
            {
                manager.readyToAttack = false;
                CancelInvoke("EndCombo");
                CancelInvoke("EndAttack");
                manager.weapon.EnableHitbox();
                if (Time.time - lastClickedTime >= manager.anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * combo[comboCounter].timeToNextAnim)
                {
                    manager.anim.runtimeAnimatorController = combo[comboCounter].animOV;
                    manager.anim.SetTrigger("Basic Attack");
                    manager.weapon.damage = combo[comboCounter].damage * attackModifier;
                    manager.weapon.critChance = critChance;
                    manager.weapon.critDamage = critDamage;
                    //Set all variables here...
                    comboCounter++;
                    lastClickedTime = Time.time;

                    if (comboCounter + 1 > combo.Count)
                    {
                        comboCounter = 0;
                    }
                }
            }
        }
    }

    public void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToSpecial && currentMana >= listOfSpecial[specialSelected].manaCost)
        {
            timeLastUsedSpecial = 0;
            currentMana -= listOfSpecial[specialSelected].manaCost;
            manager.readyToSpecial = false;
            AttackSO specialUsed = listOfSpecial[specialSelected];
            manager.anim.runtimeAnimatorController = specialUsed.animOV;
            manager.anim.SetTrigger("Special Attack");
            for (int i = 0; i < specialUsed.skillType.Length; i++)
            {
                switch (specialUsed.skillType[i])
                {
                    case AttackSO.typeOfSkill.Fireball:
                        StartCoroutine(SpawnFireball(specialUsed));
                        break;
                    case AttackSO.typeOfSkill.GiveStatus:
                        StartCoroutine(GiveStatus(specialUsed.status, specialUsed.timeBeforeApply));
                        break;
                    case AttackSO.typeOfSkill.Heal:
                        currentHealth += specialUsed.heal;
                        break;
                    case AttackSO.typeOfSkill.Quake:
                        StartCoroutine(SpawnFireball(specialUsed));
                        break;
                }
            }
        }
    }

    IEnumerator SpawnFireball(AttackSO specialUsed)
    {
        Vector3 closestEnemy = FindClosestEnemy().transform.position;
        if (closestEnemy != null)
        {
            manager.playerBody.transform.LookAt(new Vector3(closestEnemy.x, manager.playerBody.position.y, closestEnemy.z));
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
            if (closestEnemy == null)
            {
                fireball.GetComponent<Rigidbody>().AddForce(manager.playerBody.forward * specialUsed.velocity, ForceMode.Impulse);
            }
            else
            {
                fireball.GetComponent<Rigidbody>().AddForce((closestEnemy - manager.playerBody.position).normalized * specialUsed.velocity, ForceMode.Impulse);
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

    Transform FindClosestEnemy()
    {
        if (manager.enemyList.Count == 0) return null;
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
            } else
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

    public void EndAttack()
    {
        if (manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Basic Attack"))
        {
            manager.readyToAttack = true;
            Invoke("EndCombo", 1);
        }
    }

    public void EndCombo()
    {
        manager.weapon.DisableHitbox();
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
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

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
    [SerializeField] Weapon weapon;
    public List<AttackSO> combo;
    public List<AttackSO> listOfSpecial;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    int specialSelected;
    float healthVelocity;
    float manaVelocity;
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

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.time - lastComboEnd > 0.2f && comboCounter <= combo.Count)
            {
                manager.readyToAttack = false;
                CancelInvoke("EndCombo");
                if (Time.time - lastClickedTime >= manager.anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * combo[comboCounter].timeToNextAnim)
                {
                    manager.anim.runtimeAnimatorController = combo[comboCounter].animOV;
                    manager.anim.SetTrigger("Basic Attack");
                    weapon.damage = combo[comboCounter].damage * attackModifier;
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
            currentMana -= listOfSpecial[specialSelected].manaCost;
            manager.readyToSpecial = false;
            AttackSO specialUsed = listOfSpecial[specialSelected];
            manager.anim.runtimeAnimatorController = specialUsed.animOV;
            manager.anim.SetTrigger("Special Attack");
            for (int i = 0; i < specialUsed.skillType.Length; i++)
            {
                switch (specialUsed.skillType[i])
                {
                    case "projectile":
                        // spawn projectile, not planned
                        break;
                    case "giveStatus":
                        StartCoroutine(GiveStatus(specialUsed.status, specialUsed.timeBeforeApply));
                        break;
                    case "heal":
                        currentHealth += specialUsed.heal;
                        break;
                }
            }
        }
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

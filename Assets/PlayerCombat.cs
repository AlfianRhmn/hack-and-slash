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
    public float attackModifier = 1;
    [Header("Combat")]
    [SerializeField] Weapon weapon;
    public List<AttackSO> combo;
    public List<AttackSO> listOfSpecial;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    int specialSelected;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        EndAttack();
    }

    void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Attack();
        }
    }

    public void Attack()
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

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Balmond : MonoBehaviour
{
    [SerializeField] private float HP = 100;
    public Slider healthBar;
    private Animator animator;
    float velocity;

    private NavMeshAgent navAgent;

    private void Start()
    {
        PlayerManager.Instance.enemyList.Add(this);
        healthBar.maxValue = HP;
        healthBar.value = HP;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        healthBar.value = Mathf.SmoothDamp(healthBar.value, HP, ref velocity, 0.1f);
    }

    public void TakeDamage(float damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {
            animator.SetTrigger("DIE");
            StartCoroutine(Dead());
        }
        else
        {
            animator.SetTrigger("DAMAGE");
        }
    }

    IEnumerator Dead()
    {
        yield return new WaitForSeconds(1f);
        healthBar.gameObject.SetActive(false);
        PlayerManager.Instance.enemyList.Remove(this);
        Destroy(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f); // Attacking // stop attacking

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 40f); //detection star chasing
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, 50f); // stop chasing
    }
}

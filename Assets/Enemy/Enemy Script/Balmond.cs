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

        if (HP <= 50)
        {
            GetComponent<Animator>().SetBool("ENRAGED", true);
        }
     
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

    public IEnumerator LaunchEnemy(float duration, float launchHeight)
    {
        navAgent.enabled = false;

        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Create a smooth arc: 0 �� 1 �� 0 over the duration
            float yOffset = Mathf.Sin(t * Mathf.PI) * launchHeight;

            transform.position = new Vector3(
                originalPosition.x,
                originalPosition.y + yOffset,
                originalPosition.z
            );

            yield return null;
        }

        // Restore exact original position to avoid float drift
        transform.position = originalPosition;
        navAgent.enabled = true;
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

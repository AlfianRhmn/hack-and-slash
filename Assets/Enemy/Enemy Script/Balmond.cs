using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Balmond : MonoBehaviour
{
    public ObjectPooling damageNumber;
    [SerializeField] private float HP = 100;
    public Slider healthBar;
    private Animator animator;
    float velocity;

    private NavMeshAgent navAgent;

    private void Start()
    {
        healthBar.maxValue = HP;
        healthBar.value = HP;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        if (damageNumber == null)
        {
            damageNumber = GameObject.FindWithTag("Damage Number").GetComponent<ObjectPooling>();
        }
    }

    private void Update()
    {
        healthBar.value = Mathf.SmoothDamp(healthBar.value, HP, ref velocity, 0.1f);
    }

    public void TakeDamage(float damageAmount)
    {
        HP -= damageAmount;
        AlwaysLookAt look = damageNumber.GetObject().GetComponent<AlwaysLookAt>();
        look.transform.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        look.sourceOfPool = damageNumber;
        look.transform.position = transform.position;
        look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
        look.transform.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.Range(-50f, 50f), 400, UnityEngine.Random.Range(-50f, 50f)));
        look.transform.GetChild(0).GetComponent<TextMeshPro>().text = Mathf.RoundToInt(damageAmount).ToString();

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

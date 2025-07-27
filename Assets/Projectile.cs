using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ObjectPooling damageNumber;
    public float duration;
    public float damage;
    [UnityEngine.Range(0, 100f)]
    public float critChance = 50f;
    public float critDamage = 50f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Balmond enemy = other.GetComponent<Balmond>();
        if (enemy != null)
        {
            float totalDamage = damage;
            AlwaysLookAt look = damageNumber.GetObject().GetComponent<AlwaysLookAt>();
            look.transform.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            look.sourceOfPool = damageNumber;
            look.transform.position = transform.position;
            look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
            look.transform.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.Range(-50f, 50f), 400, UnityEngine.Random.Range(-50f, 50f)));
            look.transform.GetChild(0).GetComponent<TextMeshPro>().text = Mathf.RoundToInt(damage).ToString();
            look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.white;
            if (Random.Range(1, 101) <= critChance)
            {
                //Critical Hits!
                totalDamage = damage + (damage * (critDamage / 100f));
                look.transform.GetChild(0).GetComponent<TextMeshPro>().text = Mathf.RoundToInt(totalDamage).ToString();
                look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.yellow;
            }
            enemy.TakeDamage(totalDamage);
        }
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}

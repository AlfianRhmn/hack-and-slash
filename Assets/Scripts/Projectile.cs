using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int effectID; // 0 - none, 1 - quake
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
        EnemyBehaviour enemy = other.GetComponent<EnemyBehaviour>();
        if (enemy != null)
        {
            float totalDamage = damage;
            AlwaysLookAt look = damageNumber.GetObject().GetComponent<AlwaysLookAt>();
            look.sourceOfPool = damageNumber;
            look.transform.position = enemy.transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f));
            look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
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
            switch (effectID)
            {
                case 1:
                    StartCoroutine(enemy.LaunchEnemy(1f, 3f));
                    break;
            }
        }
        if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Ground")) && effectID != 1)
        {
            Destroy(gameObject);
        }
    }
}

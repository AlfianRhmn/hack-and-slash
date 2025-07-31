using UnityEngine;

public class Juggle : MonoBehaviour
{
    public float velocity = 20;
    float time;

    private void Update()
    {
        time += Time.deltaTime;
        if (time > 0.2f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<Rigidbody>().AddForce(Vector3.up * velocity, ForceMode.Impulse);
        }
    }
}

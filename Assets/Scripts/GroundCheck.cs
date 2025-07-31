using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isEnemy;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (isEnemy)
            {
                transform.parent.GetComponent<EnemyBehaviour>().SetAir(false);
            } else
            {
                PlayerManager.Instance.onAir = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (isEnemy)
            {
                transform.parent.GetComponent<EnemyBehaviour>().SetAir(true);
            }
            else
            {
                PlayerManager.Instance.onAir = true;
            }
        }
    }
}

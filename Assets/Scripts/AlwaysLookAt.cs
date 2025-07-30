using UnityEngine;

public class AlwaysLookAt : MonoBehaviour
{
    public ObjectPooling sourceOfPool;
    public Transform lookAtWhat;
    public bool enableTimer = true;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (lookAtWhat == null)
        {
            lookAtWhat = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(lookAtWhat.position);
        if (enableTimer)
        {
            timer += Time.deltaTime;
            if (timer > 3)
            {
                //dead
                sourceOfPool.ReturnObject(gameObject);
            }
        }
    }

    public void DestroyMe()
    {
        sourceOfPool.ReturnObject(gameObject);
    }
}

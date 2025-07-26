using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public GameObject prefab;
    public Queue<GameObject> pool = new Queue<GameObject>();
    public int maxAvailable;

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        } else
        {
            if (maxAvailable > 0)
            {
                if (pool.Count >= maxAvailable)
                {
                    return null;
                }
                else
                {
                    return Instantiate(prefab);
                }
            }
            else
            {
                return Instantiate(prefab);
            }
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}

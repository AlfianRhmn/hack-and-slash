using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicScrollHeight : MonoBehaviour
{
    public float offsetPerChild;
    private RectTransform rect;
    private float originHeight;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        originHeight = rect.sizeDelta.y;
    }

    // Update is called once per frame
    void Update()
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, originHeight + (transform.childCount - 1) * offsetPerChild);
    }
}

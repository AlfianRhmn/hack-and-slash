using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    public StatusEffects statusEffects;
    public Image slider;
    public Image icon;
    float currentVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize(StatusEffects status)
    {
        icon.sprite = status.statusIcon;
        statusEffects = status;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = Mathf.SmoothDamp(slider.fillAmount, statusEffects.duration / statusEffects.maxDuration, ref currentVelocity, 0.1f);
        slider.fillAmount = temp;
        if (slider.fillAmount <= 0)
        {
            Destroy(gameObject);
        }
    }
}

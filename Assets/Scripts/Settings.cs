using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI m_leftButtonDisplay;
    public TextMeshProUGUI m_rightButtonDisplay;
    public PlayerInput input;
    [Header("Tabs")]
    public GameObject gameTab; // 0
    public TextMeshProUGUI gameButton;
    public GameObject videoTab; // 1
    public TextMeshProUGUI videoButton;
    public GameObject audioTab; // 2
    public TextMeshProUGUI audioButton;
    public GameObject controlsTab; // 3
    public TextMeshProUGUI controlsButton;
    public GameObject miscTab; // 4
    public TextMeshProUGUI miscButton;
    [Range(0, 4)]
    int currentTab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        OnChangeController();
        SwitchTab(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SettingsTabLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentTab > 0)
            {
                currentTab--;
            } else
            {
                currentTab = 4;
            }
            SwitchTab(currentTab);
        }
    }

    public void SettingsTabRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentTab < 4)
            {
                currentTab++;
            }
            else
            {
                currentTab = 0;
            }
            SwitchTab(currentTab);
        }
    }

    public void SwitchTab(int tab)
    {
        DeselectAll();
        switch (tab)
        {
            case 0:
                currentTab = 0;
                gameTab.SetActive(true);
                gameButton.color = Color.yellow;
                break;
            case 1:
                currentTab = 1;
                videoTab.SetActive(true);
                videoButton.color = Color.yellow;
                break;
            case 2:
                currentTab = 2;
                audioTab.SetActive(true);
                audioButton.color = Color.yellow;
                break;
            case 3:
                currentTab = 3;
                controlsTab.SetActive(true);
                controlsButton.color = Color.yellow;
                break;
            case 4:
                currentTab = 4;
                miscTab.SetActive(true);
                miscButton.color = Color.yellow;
                break;
        }
    }

    void DeselectAll()
    {
        gameButton.color = Color.white;
        videoButton.color = Color.white;
        audioButton.color = Color.white;
        controlsButton.color = Color.white;
        miscButton.color = Color.white;
        gameTab.SetActive(false);
        videoTab.SetActive(false);
        audioTab.SetActive(false);
        controlsTab.SetActive(false);
        miscTab.SetActive(false);
    }

    public void OnChangeController()
    {
        foreach (InputDevice device in input.devices)
        {
            if (device is Gamepad)
            {
                m_leftButtonDisplay.text = "<sprite name=" + input.actions.FindAction("Change Settings Tab - Left").GetBindingDisplayString() + ">";
                m_rightButtonDisplay.text = "<sprite name=" + input.actions.FindAction("Change Settings Tab - Right").GetBindingDisplayString() + ">";
            } else
            {
                m_leftButtonDisplay.text = "<";
                m_rightButtonDisplay.text = ">";
            }
        }
    }
}

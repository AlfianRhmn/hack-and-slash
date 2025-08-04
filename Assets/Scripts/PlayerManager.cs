using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [Header("Core References")]
    public PlayerCombat combat;
    public PlayerMovement movement;
    public Animator anim;
    public Rigidbody rb;
    public PlayerInput input;
    public Transform playerBody;
    public Transform frontOfBody;

    [Header("Camera & View")]
    public Transform cam;
    public GameObject virtualThirdCam;
    public GameObject virtualHardLockCam;

    [Header("Weapon & Combat")]
    public Weapon weapon;
    public Weapon rightLeg;
    public Transform rightHand;
    public LayerMask enemyLayer;
    public float enemyDistance;

    [Header("HUD - Health & Mana")]
    public Slider healthBar;
    public Slider manaBar;

    [Header("HUD - Specials & Ultimates")]
    public TextMeshProUGUI specialName;
    public Image specialIcon;
    public TextMeshProUGUI specialInput;
    public TextMeshProUGUI ultimateName;
    public Image ultimateIcon;
    public TextMeshProUGUI ultimateProgress;

    [Header("HUD - Input Prompts")]
    public TextMeshProUGUI scrollLeftInput;
    public TextMeshProUGUI scrollRightInput;

    [Header("HUD - General & Animation")]
    public GameObject statusDisplay;
    public Transform gridStatus;
    public Animator playerHUDAnim;

    [Header("HUD - Ult UI")]
    public GameObject ultCanvas;
    public GameObject ultCamera;

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject movesetPanel;
    public GameObject settingsPanel;
    public GameObject quitPanel;

    [Header("UI Navigation")]
    public Button menuStartHighlight;
    public Button movesetHiglight;
    public Button settingsHighlight;
    public Button quitHighlight;
    public ScrollRect movesetScroll;

    [Header("Pooling Systems")]
    public ObjectPooling damageNumber;
    public ObjectPooling movesetDisplay;

    [Header("Event Systems")]
    public EventSystem eventSystem;

    [Header("Player State Flags")]
    public bool readyToDodge = true;
    public bool readyToAttack = true;
    public bool readyToSpecial = true;
    public bool readyToUltimate = true;
    public bool readyToHurt = true;
    public bool invulnerability = false;
    public bool onAir = false;

    [Header("Enemy List & Lock-On")]
    public List<EnemyBehaviour> enemyList;
    public List<EnemyBehaviour> enemyClose;
    public Transform currentLockOnTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyList.Count == 0)
        {
            movement.CancelLockOn();
        }
        HandleEnemyClose();
    }

    public void OnControllerChange()
    {
        foreach (InputDevice device in input.devices)
        {
            if (device is Keyboard)
            {
                Cursor.visible = true;
                eventSystem.SetSelectedGameObject(null);
            }
            else if (device is Mouse)
            {
                Cursor.visible = true;
                eventSystem.SetSelectedGameObject(null);
            }
            else if (device is Gamepad)
            {
                Cursor.visible = false;
                if (settingsPanel.activeSelf)
                {
                    eventSystem.SetSelectedGameObject(settingsPanel.gameObject);
                } else if (movesetPanel.activeSelf)
                {
                    eventSystem.SetSelectedGameObject(movesetHiglight.gameObject);
                } else if (quitPanel.activeSelf)
                {
                    eventSystem.SetSelectedGameObject(quitHighlight.gameObject);
                } else
                {
                    eventSystem.SetSelectedGameObject(menuStartHighlight.gameObject);
                }
            }
        }
    }

    public void OnScrollMoveset(InputAction.CallbackContext context)
    {
        if (movesetPanel.activeSelf)
        {
            float value = context.ReadValue<Vector2>().y;
            movesetScroll.verticalNormalizedPosition += value * 0.2f;
            movesetScroll.verticalNormalizedPosition = Mathf.Clamp01(movesetScroll.verticalNormalizedPosition);
        }
    }

    public void OnCancelMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (settingsPanel.activeSelf)
            {
                SettingsOpen();
            }
            else if (movesetPanel.activeSelf)
            {
                MoveSetOpen();
            }
            else if (quitPanel.activeSelf)
            {
                //QuitOpen()
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if (context.performed && !ultCanvas.gameObject.activeSelf)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pausePanel.activeSelf)
        {
            input.SwitchCurrentActionMap("Player");
            playerHUDAnim.SetTrigger("MenuOFF");
            Cursor.lockState = CursorLockMode.Locked;
            pausePanel.GetComponent<Animator>().Play("PauseDisappear");
            StartCoroutine(WaitUntilPause());
        }
        else
        {
            input.SwitchCurrentActionMap("UI");
            playerHUDAnim.Play("GameOpen");
            playerHUDAnim.Play("MenuOpen");
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            foreach (InputDevice device in input.devices)
            {
                if (device is Gamepad)
                {
                    eventSystem.SetSelectedGameObject(menuStartHighlight.gameObject);
                }
            }
        }
    }

    public void MoveSetOpen()
    {
        if (movesetPanel.activeSelf)
        {
            movesetPanel.GetComponent<Animator>().Play("PanelDisappear");
            StartCoroutine(WaitUntilMoveset());
        } else
        {
            foreach (InputDevice device in input.devices)
            {
                if (device is Gamepad)
                {
                    eventSystem.SetSelectedGameObject(movesetHiglight.gameObject);
                }
            }
            movesetPanel.SetActive(true);
            for (int i = 0; i < combat.moveset.Count; i++)
            {
                Transform display = movesetDisplay.GetObject().transform;
                display.SetParent(movesetDisplay.transform);
                display.localScale = new Vector3(0.96464f, 0.96464f, 0.96464f);
                MovesetDisplay move = display.GetComponent<MovesetDisplay>();
                move.moveset = combat.moveset[i];
                move.index = i;
                move.Initialize();
            }
        }
    }

    public void SettingsOpen()
    {
        if (settingsPanel.activeSelf)
        {
            settingsPanel.GetComponent<Animator>().Play("PanelDisappear");
            StartCoroutine(WaitUntilSettings());
        }
        else
        {
            settingsPanel.SetActive(true);
            foreach (InputDevice device in input.devices)
            {
                if (device is Gamepad)
                {
                    eventSystem.SetSelectedGameObject(settingsHighlight.gameObject);
                }
            }
        }
    }

    IEnumerator WaitUntilPause()
    {
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    IEnumerator WaitUntilMoveset()
    {
        yield return new WaitForSecondsRealtime(1f);
        foreach (InputDevice device in input.devices)
        {
            if (device is Gamepad)
            {
                eventSystem.SetSelectedGameObject(menuStartHighlight.gameObject);
            }
        }
        for (int i = 0; i < movesetDisplay.transform.childCount; i++)
        {
            movesetDisplay.ReturnObject(movesetDisplay.transform.GetChild(i).gameObject);
        }
        movesetPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    IEnumerator WaitUntilSettings()
    {
        yield return new WaitForSecondsRealtime(1f);
        foreach (InputDevice device in input.devices)
        {
            if (device is Gamepad)
            {
                eventSystem.SetSelectedGameObject(menuStartHighlight.gameObject);
            }
        }
        settingsPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    void HandleEnemyClose()
    {
        enemyClose.Clear();
        foreach (EnemyBehaviour enemy in enemyList)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.position, enemy.transform.position - cam.position,out hit, enemyDistance, enemyLayer))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    enemyClose.Add(enemy);
                }
            }
        }
    }

    public Transform FindSuperCloseEnemy()
    {
        if (enemyClose.Count == 0)
        {
            return null;
        }
        float closestDistance = Mathf.Infinity;
        int enemyIndex = 0;
        foreach (EnemyBehaviour enemy in enemyClose)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < closestDistance)
            {
                closestDistance = Vector3.Distance(transform.position, enemy.transform.position);
                enemyIndex = enemyClose.IndexOf(enemy);
            }
        }
        return enemyClose[enemyIndex].transform;
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [Header("References")]
    public PlayerCombat combat;
    public PlayerMovement movement;
    public Animator anim;
    public Transform playerBody;
    public Rigidbody rb;
    public Transform cam;
    public Slider healthBar;
    public Slider manaBar;
    public GameObject statusDisplay;
    public Transform gridStatus;
    public Weapon weapon;
    public TextMeshProUGUI specialName;
    public Image specialIcon;
    public TextMeshProUGUI specialInput;    
    public TextMeshProUGUI ultimateName;
    public Image ultimateIcon;
    public TextMeshProUGUI ultimateInput;
    public TextMeshProUGUI ultimateProgress;
    public TextMeshProUGUI scrollLeftInput;
    public TextMeshProUGUI scrollRightInput;
    public ObjectPooling damageNumber;
    public Transform rightHand;
    public GameObject ultCanvas;
    public GameObject ultCamera;
    public GameObject virtualThirdCam;
    public GameObject virtualHardLockCam;
    public LayerMask enemyLayer;
    public float enemyDistance;
    [Header("Player Restrictions")]
    public bool readyToDodge = true;
    public bool readyToAttack = true;
    public bool readyToSpecial = true;
    public bool readyToUltimate = true;
    [Header("Enemy List")]
    public List<Balmond> enemyList;
    public List<Balmond> enemyClose;
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

    void HandleEnemyClose()
    {
        enemyClose.Clear();
        foreach (Balmond enemy in enemyList)
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
}

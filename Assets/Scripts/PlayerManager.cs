using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public PlayerCombat combat;
    public PlayerMovement movement;
    public Animator anim;
    public Transform playerBody;
    public Rigidbody rb;
    public Slider healthBar;
    public Slider manaBar;
    public GameObject statusDisplay;
    public Transform gridStatus;
    [Header("Player Restrictions")]
    public bool readyToDodge = true;
    public bool readyToAttack = true;
    public bool readyToSpecial = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

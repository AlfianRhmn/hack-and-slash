using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public PlayerCombat combat;
    public PlayerMovement movement;
    public Animator anim;
    public Transform playerBody;
    public Rigidbody rb;
    [Header("Player Restrictions")]
    public bool readyToDodge = true;
    public bool readyToAttack = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

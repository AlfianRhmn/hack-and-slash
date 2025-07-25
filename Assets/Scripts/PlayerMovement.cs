using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player References")]
    public PlayerManager manager;
    [Header("General - Movement")]
    public float moveSpeed = 2;
    public float dodgeDistance = 80;
    public float dodgeCooldownTime = 0.4f;
    public float rotationSpeed = 720;
    float targetRotY;
    Vector2 moveDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void FixedUpdate()
    {
        if (manager.rb.linearDamping == 5 && manager.readyToAttack && manager.readyToSpecial)
        {
            manager.rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.y) * moveSpeed, ForceMode.Force);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToDodge && manager.readyToSpecial)
        {
            manager.combat.Reset();
            manager.rb.linearDamping = 2;
            if (manager.anim != null)
            {
                manager.anim.SetTrigger("Roll");
            }
            StartCoroutine(DodgeCooldown());
            Vector2 changedDirection = new Vector2();
            if (moveDirection == Vector2.zero)
            {
                changedDirection.x = manager.playerBody.forward.x;
                changedDirection.y = manager.playerBody.forward.z;
                changedDirection.Normalize();
            }
            else
            {
                if (moveDirection.x > 0.1f)
                {
                    changedDirection.x = 1f;
                } else if (moveDirection.x < -0.1f)
                {
                    changedDirection.x = -1f;
                }
                if (moveDirection.y > 0.1f)
                {
                    changedDirection.y = 1f;
                }
                else if (moveDirection.y < -0.1f)
                {
                    changedDirection.y = -1f;
                }
            }
            manager.rb.AddForce((new Vector3(changedDirection.x, 0, changedDirection.y) * dodgeDistance), ForceMode.Impulse);
        }
    }

    // Update is called once per frame
    void Update()
    {
        {
            if (manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Dodge") && manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                manager.rb.linearDamping = 5f;
            }

            if (manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit") && manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                manager.readyToDodge = true;
            }

            if (moveDirection != Vector2.zero && manager.readyToAttack && manager.readyToSpecial && manager.readyToDodge)
            {
                Quaternion rotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.y), Vector3.up);
                manager.playerBody.rotation = Quaternion.RotateTowards(manager.playerBody.rotation, rotation, rotationSpeed * Time.deltaTime);
            }

            if (manager.anim != null)
            {
                manager.anim.SetBool("Moving", moveDirection != Vector2.zero);
            }
        }// Movement Calculation
    }

    public IEnumerator DodgeCooldown()
    {
        manager.readyToDodge = false;
        yield return new WaitForSeconds(dodgeCooldownTime);
        manager.readyToDodge = true;
    }
}

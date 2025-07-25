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
        if (manager.rb.linearDamping == 5 && manager.readyToAttack)
        {
            manager.rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.y) * moveSpeed, ForceMode.Force);
        }
    }

    void OnMove(InputValue inputValue)
    {
        moveDirection = inputValue.Get<Vector2>();
    }

    void OnDodge(InputValue inputValue)
    {
        if (inputValue.isPressed && manager.readyToDodge)
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
                changedDirection = new Vector2(manager.playerBody.forward.x, manager.playerBody.forward.z);
                if (changedDirection.x != 0)
                {
                    if (changedDirection.x > 0)
                    {
                        changedDirection = new Vector2(1, changedDirection.y);
                    }
                    else
                    {
                        changedDirection = new Vector2(-1, changedDirection.y);
                    }
                }
                if (changedDirection.y != 0)
                {
                    if (changedDirection.y > 0)
                    {
                        changedDirection = new Vector2(changedDirection.x, 1);
                    }
                    else
                    {
                        changedDirection = new Vector2(changedDirection.x, -1);
                    }
                }
            }
            else
            {
                if (moveDirection.x != 0)
                {
                    if (moveDirection.x > 0)
                    {
                        changedDirection = new Vector2(1, moveDirection.y);
                    }
                    else
                    {
                        changedDirection = new Vector2(-1, moveDirection.y);
                    }
                }
                if (moveDirection.y != 0)
                {
                    if (moveDirection.y > 0)
                    {
                        changedDirection = new Vector2(changedDirection.x, 1);
                    }
                    else
                    {
                        changedDirection = new Vector2(changedDirection.x, -1);
                    }
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

            if (moveDirection != Vector2.zero && manager.readyToAttack)
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

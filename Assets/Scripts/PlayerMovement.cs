using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player References")]
    public PlayerManager manager;
    [Header("General - Movement")]
    public float moveSpeed = 2;
    public float slowedSpeed = 2;
    public float dodgeDistance = 80;
    public float dodgeCooldownTime = 0.4f;
    public float rotationSpeed = 720;
    float currentSpeed;
    bool isTargeting;
    float targetRotY;
    Vector2 moveDirection;
    Vector3 moveDir;
    Vector2 currentInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeed = moveSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (manager.rb.linearDamping == 5 && manager.readyToAttack && manager.readyToSpecial && manager.readyToUltimate)
        {
            manager.rb.AddForce(moveDir * currentSpeed, ForceMode.Force);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToDodge && manager.readyToSpecial && manager.readyToUltimate)
        {
            manager.combat.Reset();
            manager.rb.linearDamping = 2;
            if (manager.anim != null)
            {
                manager.anim.SetTrigger("Roll");
            }
            StartCoroutine(DodgeCooldown());
            Vector3 dodgeDirection;

            if (moveDirection == Vector2.zero)
            {
                // Dodge forward relative to player body when idle
                dodgeDirection = manager.playerBody.forward;
            }
            else
            {
                // Get camera's flat forward and right vectors
                Vector3 camForward = manager.cam.forward;
                Vector3 camRight = manager.cam.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                // Calculate movement direction relative to camera
                dodgeDirection = (camForward * moveDirection.y + camRight * moveDirection.x).normalized;
            }

            // Apply impulse force in the dodge direction
            manager.rb.AddForce(dodgeDirection * dodgeDistance, ForceMode.Impulse);
        }
    }

    public void CancelLockOn()
    {
        if (isTargeting)
        {
            isTargeting = false;
            manager.virtualThirdCam.SetActive(true);
            manager.virtualHardLockCam.SetActive(false);
            currentSpeed = moveSpeed;
            manager.currentLockOnTarget = null;
            manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = null;
        }
    }

    public void OnLockOnTarget(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToUltimate)
        {
            if (!isTargeting && manager.enemyClose.Count > 0)
            {
                Transform target = manager.enemyClose[0].transform;
                isTargeting = true;
                manager.virtualHardLockCam.SetActive(true);
                manager.virtualThirdCam.SetActive(false);
                manager.currentLockOnTarget = target;
                currentSpeed = slowedSpeed;
                manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = target;

            } else
            {
                isTargeting = false;
                manager.virtualThirdCam.SetActive(true);
                manager.virtualHardLockCam.SetActive(false);
                currentSpeed = moveSpeed;
                manager.currentLockOnTarget = null;
                manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = null;
            }
        }
    }

    public void OnChangeLockPos(InputAction.CallbackContext context)
    {
        if (context.performed && isTargeting)
        {
            HandleChangeLockOn(true);
        }
    }

    void HandleChangeLockOn(bool isPos)
    {
        int index = manager.enemyClose.IndexOf(manager.currentLockOnTarget.GetComponent<Balmond>());
        if (isPos)
        {
            index++;
            if (index >= manager.enemyClose.Count)
            {
                index = 0;
            }
        } else
        {
            index--;
            if (index < 0)
            {
                index = manager.enemyClose.Count - 1;
            }
        }
        manager.currentLockOnTarget = manager.enemyClose[index].transform;
        manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = manager.enemyClose[index].transform;
    }

    public void OnChangeLockNeg(InputAction.CallbackContext context)
    {
        if (context.performed && isTargeting)
        {
            HandleChangeLockOn(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        {
            Vector3 camForward = manager.cam.forward;
            Vector3 camRight = manager.cam.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 forwardRelative = moveDirection.y * camForward;
            Vector3 rightRelative = moveDirection.x * camRight;

            moveDir = forwardRelative + rightRelative;

            if (manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Dodge") && manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                manager.rb.linearDamping = 5f;
            }

            if (manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit") && manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                manager.readyToDodge = true;
            }

            if (moveDirection != Vector2.zero && manager.readyToAttack && manager.readyToSpecial && manager.readyToDodge && !isTargeting)
            {
                Vector3 worldDirection = (camForward * moveDirection.y + camRight * moveDirection.x).normalized;

                // Optional: visualize in Scene view
                Debug.DrawRay(manager.playerBody.position, worldDirection * 2f, Color.red);

                Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
                manager.playerBody.rotation = Quaternion.RotateTowards(manager.playerBody.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            } else if (isTargeting)
            {
                manager.playerBody.LookAt(new Vector3(manager.currentLockOnTarget.position.x, 0, manager.currentLockOnTarget.position.z));
            }

            if (manager.anim != null)
            {
                Vector2 targetInput = new Vector2(moveDirection.x, moveDirection.y);
                currentInput = Vector2.Lerp(currentInput, targetInput, Time.deltaTime * 5);

                manager.anim.SetBool("Moving", moveDirection != Vector2.zero);
                manager.anim.SetFloat("moveX", currentInput.x);
                manager.anim.SetFloat("moveY", currentInput.y);
                manager.anim.SetBool("IsTargeting", isTargeting);
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

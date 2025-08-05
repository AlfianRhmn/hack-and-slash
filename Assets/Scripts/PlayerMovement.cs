using System;
using System.Collections;
using Unity.Cinemachine;
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
    [Header("Auto-Lock")]
    public GameObject crosshair;
    float currentSpeed;
    bool isTargeting;
    bool allowSnappyRotation;
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
        if (manager.rb.linearDamping == 5 && manager.readyToAttack && manager.readyToSpecial && manager.readyToUltimate && !manager.onAir && manager.readyToDodge && !manager.isDead)
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
        if (context.performed && manager.readyToDodge && manager.readyToSpecial && manager.readyToUltimate && !manager.onAir && !manager.isDead)
        {
            manager.invulnerability = true;
            manager.combat.Reset();
            manager.rb.linearDamping = 2;
            if (manager.anim != null)
            {
                manager.anim.SetTrigger("Roll");
            }
            StartCoroutine(DodgeCooldown());
            StartCoroutine(StartSnappy());
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
            manager.rb.linearVelocity = dodgeDirection * dodgeDistance;
        }
    }

    IEnumerator StartSnappy()
    {
        allowSnappyRotation = true;
        yield return new WaitForSeconds(0.05f);
        allowSnappyRotation = false;
    }

    public void CancelLockOn()
    {
        if (isTargeting)
        {
            isTargeting = false;
            crosshair.SetActive(false);
            manager.virtualThirdCam.SetActive(true);
            manager.virtualHardLockCam.SetActive(false);
            currentSpeed = moveSpeed;
            manager.currentLockOnTarget = null;
            manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = null;
        }
    }

    public void OnLockOnTarget(InputAction.CallbackContext context)
    {
        if (context.performed && manager.readyToUltimate && !manager.isDead)
        {
            if (!isTargeting && manager.enemyClose.Count > 0)
            {
                crosshair.SetActive(true);
                Transform target = manager.FindSuperCloseEnemy();
                isTargeting = true;
                manager.virtualHardLockCam.SetActive(true);
                manager.virtualThirdCam.SetActive(false);
                manager.currentLockOnTarget = target;
                currentSpeed = slowedSpeed;
                manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = target.GetComponent<EnemyBehaviour>().headOfModel;

            } else
            {
                crosshair.SetActive(false);
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
        if (context.performed && isTargeting && !manager.isDead)
        {
            HandleChangeLockOn(true);
        }
    }

    void HandleChangeLockOn(bool isPos)
    {
        int index = manager.enemyClose.IndexOf(manager.currentLockOnTarget.GetComponent<EnemyBehaviour>());
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
        manager.virtualHardLockCam.GetComponent<CinemachineCamera>().LookAt = manager.enemyClose[index].GetComponent<EnemyBehaviour>().headOfModel;
    }

    public void OnChangeLockNeg(InputAction.CallbackContext context)
    {
        if (context.performed && isTargeting && !manager.isDead)
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

            if (manager.onAir)
            {
                manager.rb.linearDamping = 0f;
            }
            else
            {
                manager.rb.linearDamping = 5f;
            }

            if (manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Dodge") && manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                manager.rb.linearDamping = 5f;
            }

            if (manager.anim.GetCurrentAnimatorStateInfo(0).IsTag("Dodge") && manager.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.4f)
            {
                manager.invulnerability = false;
            }

            if (moveDirection != Vector2.zero && manager.readyToAttack && manager.readyToSpecial && manager.readyToDodge && !isTargeting && !manager.onAir && !manager.isDead)
            {
                Vector3 worldDirection = (camForward * moveDirection.y + camRight * moveDirection.x).normalized;

                // Optional: visualize in Scene view
                Debug.DrawRay(manager.playerBody.position, worldDirection * 2f, Color.red);

                Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
                manager.playerBody.rotation = Quaternion.RotateTowards(manager.playerBody.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            } else if (moveDirection != Vector2.zero && manager.readyToDodge == false && allowSnappyRotation && !manager.isDead) 
            {
                Vector3 worldDirection = (camForward * moveDirection.y + camRight * moveDirection.x).normalized;

                // Optional: visualize in Scene view
                Debug.DrawRay(manager.playerBody.position, worldDirection * 2f, Color.red);

                Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
                manager.playerBody.rotation = targetRotation;
            }
            else if (isTargeting && !manager.isDead)
            {
                manager.playerBody.LookAt(new Vector3(manager.currentLockOnTarget.position.x, manager.playerBody.position.y, manager.currentLockOnTarget.position.z));
            }

            if (manager.anim != null && !manager.onAir)
            {
                Vector2 targetInput = new Vector2(moveDirection.x, moveDirection.y);
                currentInput = Vector2.Lerp(currentInput, targetInput, Time.deltaTime * 5);

                manager.anim.SetBool("Moving", moveDirection != Vector2.zero);
                manager.anim.SetFloat("moveX", currentInput.x);
                manager.anim.SetFloat("moveY", currentInput.y);
                manager.anim.SetBool("IsTargeting", isTargeting);
            }
            if (manager.onAir)
            {
                manager.anim.SetBool("Moving", false);
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

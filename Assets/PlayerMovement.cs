using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("General")]
    public float moveSpeed = 2;
    public float maxSpeed = 10;
    public float dodgeDistance = 80;
    public float dodgeCooldownTime = 0.4f;
    public float rotationSpeed = 720;
    public Transform playerBody;
    float targetRotY;
    Rigidbody rb;
    bool dodgeReady = true;
    Vector2 moveDirection;
    [Header("Animation")]
    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = playerBody.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        {
            moveDirection.x = Input.GetAxis("Horizontal");
            moveDirection.y = Input.GetAxis("Vertical");

            if (rb.linearDamping == 5)
            {

                rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.y) * moveSpeed, ForceMode.Force);
            }
            if (Input.GetKeyDown(KeyCode.LeftShift) && dodgeReady)
            {
                rb.linearDamping = 2;
                if (anim != null)
                {
                    anim.SetTrigger("Roll");
                }
                StartCoroutine(DodgeCooldown());
                Vector2 changedDirection = new Vector2();
                if (moveDirection ==  Vector2.zero)
                {
                    changedDirection = new Vector2(playerBody.forward.x, playerBody.forward.z);
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
                } else
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
                rb.AddForce((new Vector3(changedDirection.x, 0, changedDirection.y) * dodgeDistance), ForceMode.Impulse);
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Dodge") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                rb.linearDamping = 5f;
            }

            if (moveDirection != Vector2.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.y), Vector3.up);
                playerBody.rotation = Quaternion.RotateTowards(playerBody.rotation, rotation, rotationSpeed * Time.deltaTime);
            }

            IEnumerator DodgeCooldown()
            {
                dodgeReady = false;
                yield return new WaitForSeconds(dodgeCooldownTime);
                dodgeReady = true;
            }

            if (anim != null)
            {
                anim.SetBool("Moving", moveDirection != Vector2.zero);
            }
        }// Movement Calculation
    }
}

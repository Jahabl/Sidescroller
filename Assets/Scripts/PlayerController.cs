using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private BoxCollider2D coll;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip climb;

    [SerializeField] private float topSpeed = 3f;
    [SerializeField] private float bottomSpeed = 1f;
    private Vector2 input = Vector2.zero;
    private string currentGround;
    private bool hasToCrouch;
    public Vector2 ladderDirection;
    private bool canClimbLedge;
    private bool canWalk = true;
    private bool isOnLadder;

    private Rigidbody2D currentBox;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            HandleLeftInput(false);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            HandleLeftInput(true);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            HandleRightInput(false);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            HandleRightInput(true);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            HandleActionInput(false);
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            HandleActionInput(true);
        }

        if (currentGround != "")
        {
            if (canWalk)
            {
                if (animator.GetBool("IsPushing") || animator.GetBool("IsCrouching"))
                {
                    body.velocity = input * bottomSpeed;
                }
                else
                {
                    body.velocity = input * topSpeed;
                }
            }
            else if (animator.GetInteger("PlayerState") == (int)PlayerState.Climbing)
            {
                body.velocity = new Vector2(0, ladderDirection.y);
            }
            else
            {
                body.velocity = Vector2.zero;
            }
        }
    }

    public void HandleLeftInput(bool wasCancelled)
    {
        Debug.Log("L");

        if (!wasCancelled)
        {
            input.x = -1f;

            if (!isOnLadder)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
                animator.SetInteger("PlayerState", (int)PlayerState.Walking);
            }
        }
        else
        {
            if (input.x == -1f)
            {
                input.x = 0f;

                if (!isOnLadder)
                {
                    animator.SetInteger("PlayerState", (int)PlayerState.Idling);
                }
            }

            animator.SetBool("IsPushing", false);
        }
    }

    public void HandleRightInput(bool wasCancelled)
    {
        Debug.Log("R");

        if (!wasCancelled)
        {
            input.x = 1f;

            if (!isOnLadder)
            {
                transform.localScale = Vector3.one;
                animator.SetInteger("PlayerState", (int)PlayerState.Walking);
            }
        }
        else
        {
            if (input.x == 1f)
            {
                input.x = 0f;

                if (!isOnLadder)
                {
                    animator.SetInteger("PlayerState", (int)PlayerState.Idling);
                }
            }

            animator.SetBool("IsPushing", false);
        }
    }

    public void HandleActionInput(bool wasCancelled)
    {
        Debug.Log("X");

        if (!wasCancelled)
        {
            if (canWalk && !animator.GetBool("IsCrouching"))
            {
                if (canClimbLedge)
                {
                    canWalk = false;
                    animator.SetInteger("PlayerState", (int)PlayerState.HoldingLedge);
                }
                else if (ladderDirection.y != 0)
                {
                    canWalk = false;
                    animator.SetInteger("PlayerState", (int)PlayerState.Climbing);
                    body.gravityScale = 0f;
                    body.MovePosition(new Vector2(ladderDirection.x, transform.position.y));
                    coll.isTrigger = true;
                    isOnLadder = true;
                }
                else if (!animator.GetBool("IsPushing"))
                {
                    canWalk = !hasToCrouch;
                    animator.SetBool("IsCrouching", true);
                }
            }
            else if (isOnLadder)
            {
                animator.SetInteger("PlayerState", (int)PlayerState.Climbing);
            }
        }
        else
        {
            if (canClimbLedge && animator.GetInteger("PlayerState") == (int)PlayerState.HoldingLedge)
            {
                StartCoroutine("ClimbLedge");
            }
            else if (ladderDirection.y != 0)
            {
                if (isOnLadder)
                {
                    animator.SetInteger("PlayerState", (int)PlayerState.Idling);
                }
                else
                {
                    canWalk = true;
                    if (input.x != 0)
                    {
                        animator.SetInteger("PlayerState", (int)PlayerState.Walking);
                    }
                    else
                    {
                        animator.SetInteger("PlayerState", (int)PlayerState.Idling);
                    }
                    body.gravityScale = 1f;
                    coll.isTrigger = false;
                }
            }
            else if (!hasToCrouch && canWalk)
            {
                if (currentBox != null && animator.GetInteger("PlayerState") == (int)PlayerState.Walking)
                {
                    currentBox.bodyType = RigidbodyType2D.Dynamic;

                    animator.SetBool("IsPushing", true);
                }

                animator.SetBool("IsCrouching", false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (collision.GetContact(0).normal == Vector2.up)
            {
                currentGround = collision.collider.name;
                animator.SetBool("IsGrounded", true);
            }
            else
            {
                canClimbLedge = true;
            }
        }
        else if (collision.gameObject.CompareTag("Box"))
        {
            currentBox = collision.rigidbody;

            if (collision.GetContact(0).normal == Vector2.up)
            {
                currentGround = collision.collider.name;
                animator.SetBool("IsGrounded", true);
            }
            else if (!animator.GetBool("IsCrouching"))
            {
                animator.SetBool("IsPushing", true);
            }
            else
            {
                collision.rigidbody.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (currentGround == collision.collider.name)
            {
                currentGround = "";
                canWalk = false;
                animator.SetBool("IsGrounded", canClimbLedge);
            }
            else
            {
                canClimbLedge = false;
            }
        }
        else if (collision.gameObject.CompareTag("Box"))
        {
            currentBox = null;

            if (currentGround == collision.collider.name)
            {
                currentGround = "";
                animator.SetBool("IsGrounded", canClimbLedge);
            }

            collision.rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            Vector3 direction = collision.transform.position - transform.position;
            direction.Normalize();
            ladderDirection = new Vector2(collision.transform.position.x, Mathf.RoundToInt(direction.y));
        }
        else if (collision.gameObject.CompareTag("Tunnel"))
        {
            hasToCrouch = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            ladderDirection = Vector2.zero;
            isOnLadder = false;

            if (animator.GetInteger("PlayerState") == (int)PlayerState.Climbing)
            {
                canWalk = true;
                if (input.x != 0)
                {
                    animator.SetInteger("PlayerState", (int)PlayerState.Walking);
                }
                else
                {
                    animator.SetInteger("PlayerState", (int)PlayerState.Idling);
                }
                body.gravityScale = 1f;
                coll.isTrigger = false;
            }
        }
        else if (collision.gameObject.CompareTag("Tunnel"))
        {
            hasToCrouch = false;
        }
    }

    public void ToggleWalk(int canWalk)
    {
        this.canWalk = canWalk == 1;
    }

    IEnumerator ClimbLedge()
    {
        if (input.x != 0)
        {
            animator.SetInteger("PlayerState", (int)PlayerState.Walking);
        }
        else
        {
            animator.SetInteger("PlayerState", (int)PlayerState.Idling);
        }

        coll.enabled = false;

        float time = 0f;
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + Vector2.up * 0.75f;

        if (transform.localScale.x > 0) //right
        {
            targetPosition += Vector2.right * 0.5f;
        }
        else
        {
            targetPosition -= Vector2.right * 0.5f;
        }

        float duration = climb.length;

        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        canWalk = true;
        canClimbLedge = false;
        coll.enabled = true;
    }
}

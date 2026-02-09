using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip climb;

    [SerializeField] private float topSpeed = 3f;
    [SerializeField] private float bottomSpeed = 1f;
    private Vector2 input = Vector2.zero;
    public string currentGround;
    public bool hasToCrouch;
    public bool canClimbLedge;
    public bool canWalk = true;

    private Rigidbody2D currBox;

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
            transform.localScale = new Vector3(-1f, 1f, 1f);
            animator.SetInteger("PlayerState", (int)PlayerState.Walking);
        }
        else
        {
            if (input.x == -1f)
            {
                input.x = 0f;
                animator.SetInteger("PlayerState", (int)PlayerState.Idling);
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
            transform.localScale = Vector3.one;
            animator.SetInteger("PlayerState", (int)PlayerState.Walking);
        }
        else
        {
            if (input.x == 1f)
            {
                input.x = 0f;
                animator.SetInteger("PlayerState", (int)PlayerState.Idling);
            }

            animator.SetBool("IsPushing", false);
        }
    }

    public void HandleActionInput(bool wasCancelled)
    {
        Debug.Log("X");

        if (!wasCancelled && canWalk)
        {
            if (!animator.GetBool("IsCrouching"))
            {
                if (canClimbLedge)
                {
                    canWalk = false;
                    animator.SetInteger("PlayerState", (int)PlayerState.HoldingLedge);
                }
                else if (!animator.GetBool("IsPushing"))
                {
                    canWalk = !hasToCrouch;
                    animator.SetBool("IsCrouching", true);
                }
            }
        }
        else
        {
            if (canClimbLedge && animator.GetInteger("PlayerState") == (int)PlayerState.HoldingLedge)
            {
                StartCoroutine("ClimbLedge");
            }
            else if (!hasToCrouch && canWalk)
            {
                if (currBox != null && animator.GetInteger("PlayerState") == (int)PlayerState.Walking)
                {
                    currBox.bodyType = RigidbodyType2D.Dynamic;

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
            currBox = collision.rigidbody;

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
            currBox = null;

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
            //canClimbLedge = true;
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
            //canClimbLedge = false;
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

        BoxCollider2D coll = transform.GetComponent<BoxCollider2D>();
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
        //body.gravityScale = 1f;
    }
}

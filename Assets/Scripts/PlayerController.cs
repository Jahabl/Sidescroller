using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator;

    [SerializeField] private float topSpeed = 3f;
    [SerializeField] private float bottomSpeed = 1f;
    private Vector2 input = Vector2.zero;
    private string currentGround;
    private bool IsCrouching;
    private bool hasToCrouch;
    private bool canClimb;
    private bool isClimbing;
    private bool isPushing;
    public bool canWalk;

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
                if (isPushing || IsCrouching)
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
        if (!wasCancelled)
        {
            input.x = -1f;
            transform.localScale = new Vector3(-1f, 1f, 1f);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            input.x = 0f;
            animator.SetBool("IsWalking", false);

            isPushing = false;
            animator.SetBool("IsPushing", false);
        }
    }

    public void HandleRightInput(bool wasCancelled)
    {
        if (!wasCancelled)
        {
            input.x = 1f;
            transform.localScale = Vector3.one;
            animator.SetBool("IsWalking", true);
        }
        else
        {
            input.x = 0f;
            animator.SetBool("IsWalking", false);

            isPushing = false;
            animator.SetBool("IsPushing", false);
        }
    }

    public void HandleActionInput(bool wasCancelled)
    {
        if (!wasCancelled)
        {
            /*if (canClimb)
            {
                isClimbing = true;
                body.gravityScale = 0f;
            }
            else */if (!isPushing)
            {
                canWalk = !hasToCrouch;
                IsCrouching = true;
                animator.SetBool("IsCrouching", true);
            }
        }
        else
        {
            canWalk = true;

            if (canClimb)
            {
                /*body.gravityScale = 1f;*/
            }
            else if (!hasToCrouch)
            {
                if (currBox != null)
                {
                    currBox.bodyType = RigidbodyType2D.Dynamic;

                    isPushing = true;
                    animator.SetBool("IsPushing", true);
                }

                IsCrouching = false;
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
                canWalk = true;
                animator.SetBool("IsGrounded", true);
            }
            else
            {
                Debug.Log("CAN CLIMB");
            }
        }
        else if (collision.gameObject.CompareTag("Box"))
        {
            currBox = collision.rigidbody;

            if (collision.GetContact(0).normal == Vector2.up)
            {
                currentGround = collision.collider.name;
                canWalk = true;
                animator.SetBool("IsGrounded", true);
            }
            else if (!IsCrouching)
            {
                isPushing = true;
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
                animator.SetBool("IsGrounded", false);
            }
            else
            {
                Debug.Log("NO CLIMB");
            }
        }
        else if (collision.gameObject.CompareTag("Box"))
        {
            currBox = null;

            if (currentGround == collision.collider.name)
            {
                currentGround = "";
                animator.SetBool("IsGrounded", false);
            }

            collision.rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            canClimb = true;
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
            canClimb = false;
            isClimbing = false;
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
}

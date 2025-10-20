using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset playerActionsMap;
    private InputAction jumpAction;

    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Rigidbody2D rb;

    public float jumpPower = 5f;
    public float playerSpeed = 5f;

    private Vector2 finalVector;

    private float groundAngleThreshold = 45f;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;

    private void OnEnable()
    {
        playerActionsMap.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        playerActionsMap.FindActionMap("Player").Disable();
    }
    private void Awake()
    {
        jumpAction = playerActionsMap.FindAction("Jump");
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void FixedUpdate()
    {
        Movement();
        Jump();
        IconRotate();
    }
    private void Movement()
    {
        rb.linearVelocityX = playerSpeed;
    }
    private void Jump()
    {
        if(isGrounded && jumpAction.IsPressed())
        {
            rb.linearVelocityY = jumpPower;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        CheckIfGround(collision, false);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckIfGround(collision, true);
    }
    private void CheckIfGround(Collision2D collision, bool collisionState)
    {
        if(collisionState) {
            int countGround = 0;

            foreach (var contact in collision.contacts)
            {
                float angle = Vector2.Angle(contact.normal, Vector2.up);

                if (angle < groundAngleThreshold) countGround += 1;
            }
            if (countGround > 0) isGrounded = true;
            else isGrounded = false;
        }
        else
        {
            if (collision.contacts.Length == 0) isGrounded = false;
        }
    }
    private void IconRotate()
    {
        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float timeToPeak = -jumpPower / gravity;
        float perfectRotateTime = timeToPeak * 2;
        float speedRotateTime = 180/perfectRotateTime;
        if (!isGrounded)
        {
            playerTransform.Rotate(0f, 0f, -speedRotateTime * Time.fixedDeltaTime);
        }
        if (isGrounded && !wasGroundedLastFrame)
        {
            float currentAngle = playerTransform.eulerAngles.z;
            float targetAngle = Mathf.Round(currentAngle / 90f) * 90f;

            playerTransform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }
        wasGroundedLastFrame = isGrounded;
    }
}


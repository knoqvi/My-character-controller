using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerLogic : MonoBehaviour
{
    [SerializeField] private InputActionAsset playerActionsMap;
    private InputAction jumpAction;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private BoxCollider2D playerCollider;

    public float jumpVelocity = 5f;
    public float movementVelocity = 5f;
    private float airRotationSpeed;
    public float groundAlignmentSpeed = 720f;

    private List<RaycastHit2D> groundHits = new List<RaycastHit2D>();
    private RaycastHit2D groundHit;

    private float maxSlopeAngle = 45f;
    private float groundCheckOffset = 0.01f;
    private float rotationSpeed;
    private bool isGrounded;
    private bool wasGroundedLastFrame = false;

    enum GravityDirection
    {
        up,
        down
    }
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
        playerRb.freezeRotation = true;
    }
    private void FixedUpdate()
    {
        CheckGround();
        IconRotate();
        if (isGrounded && !jumpAction.IsPressed()) StickToGround();
        Movement();

        float gravity = Physics2D.gravity.y * playerRb.gravityScale;
        float timeToPeak = -jumpVelocity / gravity;
        float fullFlightTime = timeToPeak * 2;
        airRotationSpeed = 180 / fullFlightTime;

    }
    private void Movement()
    {
        playerRb.linearVelocityX = movementVelocity;
        Jump();
    }

    private void Jump()
    {
        if(jumpAction.IsPressed() && isGrounded)
        {
            playerRb.linearVelocityY = jumpVelocity;
        }
    }
    private void CheckGround()
    {
        List<RaycastHit2D> groundNormals = new List<RaycastHit2D>();
        int groundCount = 0;

        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, groundCheckOffset))
        {
            if (hit.collider == playerCollider) continue;
            else groundNormals.Add(hit);
        }
        groundHits = groundNormals;
        foreach (RaycastHit2D hit in groundHits)
        {
            if (Vector2.Angle(Vector2.up, hit.normal) < maxSlopeAngle) groundCount++;
        }
        isGrounded = groundCount > 0 ? true : false;
        foreach (RaycastHit2D hit in groundHits)
        {
            if (Vector2.Angle(Vector2.up, hit.normal) < maxSlopeAngle) groundHit = hit;
        }     
    }
    private void StickToGround()
    {
        if (playerRb.linearVelocityY < 0)
        {
            playerRb.linearVelocityY = 0;
        }

        float nextX = playerRb.position.x + playerRb.linearVelocityX * Time.fixedDeltaTime;

        float playerHalfHeight = playerCollider.bounds.extents.y;
        float surfaceY = groundHit.point.y;
        float targetY = surfaceY + playerHalfHeight;

        if (playerRb.position.y < targetY)
        {
            playerRb.MovePosition(new Vector2(nextX, targetY));
        }

    }
    private void IconRotate()
    {
        float gravity = Physics2D.gravity.y * playerRb.gravityScale;
        float timeToPeak = -jumpVelocity / gravity;
        float fullFlightTime = timeToPeak * 2;
      
        rotationSpeed = 180 / fullFlightTime;
        
        if (isGrounded)
        {
            float currentZ = transform.eulerAngles.z;
            float nearest90 = Mathf.Round(currentZ / 90f) * 90f;
            float slopeAngle = Vector2.SignedAngle(Vector2.up, groundHit.normal);
            float targetAngle = nearest90 + slopeAngle;

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            float speed = (rotationSpeed > 0) ? rotationSpeed : 360f; 
            float step = speed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        }
        else
        {
            transform.Rotate(0f, 0f, -airRotationSpeed * Time.fixedDeltaTime);
        }
    }
}


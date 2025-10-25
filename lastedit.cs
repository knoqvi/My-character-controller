using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerlLogic : MonoBehaviour
{

    [SerializeField] private InputActionAsset playerActionsMap;
    private InputAction jumpAction;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Collider2D playerCollider;
    private Bounds playerBounds;

    private float supportBoxCastDistance = 2f;
    private float boxCastAngle;
    private Vector2 boxCastDirection;
    private float boxCastOffset = 0.01f;

    public float gravityScale = 10f;
    public float jumpHeight = 2f;
    public float velocityX = 5f;
    public float velocityY;

    private RaycastHit2D closestHit;
    private Vector2 groundNormal;
    private float maxSlopeAngle = 45f;
    private bool isGrounded;

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
        playerRb.bodyType = RigidbodyType2D.Kinematic;
    }
    private void Update()
    {
        playerBounds = playerCollider.bounds;
        HandleJumpInput();
        CheckGround();
        DebugGame();
    }
    private void FixedUpdate()
    {
        GetGroundAlignmentAngle();
    
        HandleGravity();
        HandleMovement(closestHit);

        //PlayerRotate();
    }
    private void HandleJumpInput()
    {
        if (jumpAction.IsPressed() && isGrounded)
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * gravityScale));
        }
    }
    private void HandleGravity()
    {
        if (isGrounded && velocityY <= 0)
        {
            velocityY = 0f;
        }
        else
        {
            velocityY += Physics2D.gravity.y * gravityScale * Time.fixedDeltaTime;
        }
    }
    private void HandleMovement(RaycastHit2D groundHit)
    {
        Vector2 moveInput = Vector2.right * velocityX;
        Vector2 velocityVector;

        if (isGrounded)
        {
            velocityVector = Vector3.ProjectOnPlane(moveInput, groundNormal);
            velocityVector += groundNormal * velocityY;
        }
        else
        {
            velocityVector = moveInput + new Vector2(0, velocityY);
        }

        Vector2 finalDisplacement = velocityVector * Time.fixedDeltaTime;
        if (isGrounded && groundHit.collider != null)
        {
            float distanceToGround = groundHit.distance;

            if (finalDisplacement.y < -distanceToGround)
            {
                finalDisplacement.y = -distanceToGround;
            }
        }

        playerRb.MovePosition(playerRb.position + finalDisplacement);
    }
    private void GetGroundAlignmentAngle()
    {
        RaycastHit2D closestHit = new RaycastHit2D();
        Vector2 normal = Vector2.up;
        float minDistance = Mathf.Infinity;

        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerBounds.center, new Vector2(1,1), 0f, Vector2.down, supportBoxCastDistance))
        {
            if (hit.collider == playerCollider) continue;
            if (hit.distance < minDistance && Vector2.Angle(Vector2.up, hit.normal) <= maxSlopeAngle)
            {
                minDistance = hit.distance;
                closestHit = hit;
                normal = hit.normal;
            }
        }
        boxCastAngle = Vector2.SignedAngle(Vector2.up, normal);
        boxCastDirection = -closestHit.normal;
    }
    private void CheckGround()
    {
        RaycastHit2D closestHit = new RaycastHit2D();
        Vector2 normal = Vector2.up;
        float minDistance = Mathf.Infinity;
        int groundCount = 0;

        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerBounds.center, playerBounds.size, boxCastAngle, boxCastDirection, boxCastOffset))
        {
            if (hit.collider == playerCollider) continue;
            if (hit.distance < minDistance && Vector2.Angle(Vector2.up, hit.normal) <= maxSlopeAngle)
            {
                minDistance = hit.distance;
                closestHit = hit;
                normal = hit.normal;
                groundCount++;
            }
        }
        groundNormal = normal;
        isGrounded = groundCount > 0;
        this.closestHit = closestHit;
    }
    private void PlayerRotate()
    {
        float gravity = Physics2D.gravity.y * playerRb.gravityScale;
        float fullFlightTime = (-jumpHeight / gravity) * 2;
        float playerRotationSpeed = 180 / fullFlightTime;
        if (isGrounded && !jumpAction.IsPressed())
        {
            float currentAngle = transform.eulerAngles.z;
            float nearest90 = Mathf.Round(currentAngle / 90f) * 90f;
            float slopeAngle = Vector2.SignedAngle(Vector2.up, groundNormal);
            float targetAngle = nearest90 + slopeAngle;

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, playerRotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.Rotate(0f, 0f, -playerRotationSpeed * Time.fixedDeltaTime);
        }
    }
    private void DebugGame()
    {
    }
    private void OnDrawGizmos()
    {
        Bounds boundsForGizmo = playerCollider.bounds;
        DrawBoxCast2D(boundsForGizmo.center, boundsForGizmo.size, 0f, Vector2.down, supportBoxCastDistance, Color.green);
        DrawBoxCast2D(boundsForGizmo.center, new Vector2(1, 1), boxCastAngle, Vector2.down, boxCastOffset, Color.red);
    }
    public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, Color color)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        direction.Normalize();

        Vector2 halfSize = size / 2;
        Vector2 p1 = new Vector2(-halfSize.x, halfSize.y);
        Vector2 p2 = new Vector2(halfSize.x, halfSize.y);
        Vector2 p3 = new Vector2(halfSize.x, -halfSize.y);
        Vector2 p4 = new Vector2(-halfSize.x, -halfSize.y);

        p1 = rotation * p1;
        p2 = rotation * p2;
        p3 = rotation * p3;
        p4 = rotation * p4;

        Debug.DrawLine(origin + p1, origin + p2, color);
        Debug.DrawLine(origin + p2, origin + p3, color);
        Debug.DrawLine(origin + p3, origin + p4, color);
        Debug.DrawLine(origin + p4, origin + p1, color);

        Vector2 endOrigin = origin + direction * distance;

        Debug.DrawLine(endOrigin + p1, endOrigin + p2, color);
        Debug.DrawLine(endOrigin + p2, endOrigin + p3, color);
        Debug.DrawLine(endOrigin + p3, endOrigin + p4, color);
        Debug.DrawLine(endOrigin + p4, endOrigin + p1, color);

        Debug.DrawLine(origin + p1, endOrigin + p1, color);
        Debug.DrawLine(origin + p2, endOrigin + p2, color);
        Debug.DrawLine(origin + p3, endOrigin + p3, color);
        Debug.DrawLine(origin + p4, endOrigin + p4, color);
    }
}

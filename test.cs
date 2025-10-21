using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.Rendering;

public class PlayerLogic : MonoBehaviour
{
    [SerializeField] private InputActionAsset playerActionsMap;
    private InputAction jumpAction;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private BoxCollider2D playerCollider;

    public float jumpVelocity = 5f;
    public float movementVelocity = 5f;

    private List<RaycastHit2D> groundHits = new List<RaycastHit2D>();
    private RaycastHit2D groundHit;

    private float maxSlopeAngle = 45f;
    private float groundCheckOffset = 0.1f;
    private bool isGrounded;

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
        GetNormals();
        CheckGround();
        GetCurrentGroundNormal();
        Movement();
    }
    private void Movement()
    {

        Jump();
        //PlayerCollider();
    }
    private void PlayerCollider()
    {
        if (isGrounded) playerRb.linearVelocityY = 0f;
    }

    private void Jump()
    {
        if(jumpAction.IsPressed() && isGrounded)
        {
            playerRb.linearVelocityY = jumpVelocity;
        }
    }
    private void GetCurrentGroundNormal() 
    {        
        foreach(RaycastHit2D hit in groundHits)
        {
            if (Vector2.Angle(Vector2.up, hit.normal) < maxSlopeAngle) groundHit = hit;
        }
    }
    private void GetNormals()
    {
        List <RaycastHit2D> groundNormals = new List<RaycastHit2D>();
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, groundCheckOffset))
        {
            if (hit.collider == playerCollider) continue;
            else groundNormals.Add(hit);
        }
        GetCurrentGroundNormal();
    }
    private void CheckGround()
    {
        int groundCount = 0;
        foreach(RaycastHit2D hit in groundHits)
        {
            if (Vector2.Angle(Vector2.up, hit.normal) < maxSlopeAngle) groundCount++;
        }
        isGrounded = groundCount > 0 ? true : false;
    }
    private void StickToGround()
    {

        if (playerRb.linearVelocityY < 0)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX, 0);
        }
        float playerHalfHeight = playerCollider.bounds.extents.y;
        float surfaceY = groundHit.point.y;
        float targetY = surfaceY + playerHalfHeight;

        if (playerRb.position.y < targetY)
        {
            playerRb.MovePosition(new Vector2(playerRb.position.x, targetY));
        }
    }
}

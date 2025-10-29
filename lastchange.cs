using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerlLogic : MonoBehaviour
{

    [SerializeField] private InputActionAsset playerActionsMap;
    internal float verticalVelocity = 5f;
    private InputAction jumpAction;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Collider2D playerCollider;
    private Bounds playerBounds;

    private float supportBoxCastDistance = 2f;
    private float boxCastAngle;
    private Vector2 boxCastDirection;
    private float boxCastOffset = 0.01f;

    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 20f;

    private Vector2 verticalVector = Vector2.zero;
    private Vector2 horizontalVector = Vector2.zero;

    private Vector2 groundNormal;

    private float maxSlopeAngle = 45f;
    private bool isGrounded;

    internal bool isDead = false;


    private Orb currentOrb;
    private enum GravityState { Down, Up };
    private GravityState gravityState = GravityState.Down;
    private Vector2 gravityVector;

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
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterPlayer(this);
        }
    }
    private void Update()
    {
        playerBounds = playerCollider.bounds;
        if (gravityState == GravityState.Down) gravityVector = Vector2.down;
        if (gravityState == GravityState.Up) gravityVector = Vector2.up;
    }
    private void FixedUpdate()
    {
        PlayerGroundCastAlignmentAngle();
        PlayerGroundCast();
        PlayerRotate();

        FinalMovement();
        HitBox();
    }
    private void HitBox()
    {
        PlayerHitBox();
        WallHitbox();
        if (isDead)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    private void PlayerHitBox()
    {
        int orbCount = 0;
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerBounds.center, transform.localScale, 0f, Vector2.zero, 0f))
        {
            if (hit.collider == playerCollider) continue;
            if (hit.collider.CompareTag("Spike"))
            {
                isDead = true;
            }
            if (hit.collider.CompareTag("Orb"))
            {
                currentOrb = hit.collider.GetComponent<Orb>();
                orbCount++;
            }
        }
        if (orbCount == 0)
        {
            currentOrb = null;
        }
    }
    private void WallHitbox()
    {
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerBounds.center, transform.localScale * 0.25f, 0f, Vector2.zero, 0f))
        {
            if (hit.collider == playerCollider || hit.collider.CompareTag("Orb") || hit.collider.isTrigger) continue;

            isDead = true;
        }
    }
    private void FinalMovement()
    {
        GroundMovement();
        VerticalMovement();

        playerRb.MovePosition(transform.position + new Vector3(horizontalVector.x + verticalVector.x, horizontalVector.y + verticalVector.y, 0f));
    }
    private void GroundMovement()
    {
        Vector2 projectVector = Vector3.ProjectOnPlane(Vector2.right, groundNormal).normalized;
        float adjustedSpeed = horizontalSpeed / projectVector.x;
        horizontalVector = projectVector * adjustedSpeed * Time.deltaTime;
    }
    private void VerticalMovement()
    {
        RaycastHit2D closestHit = new RaycastHit2D();
        Vector2 normal = Vector2.up;
        float minDistance = Mathf.Infinity;
        bool foundHit = false;


        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerBounds.center, transform.localScale, boxCastAngle, Vector2.down, 10f))
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider == playerCollider) continue;
            if (hit.distance < minDistance && Vector2.Angle(Vector2.up, hit.normal) <= maxSlopeAngle)
            {
                minDistance = hit.distance;
                closestHit = hit;
                normal = hit.normal;
                foundHit = true;
            }

        }

        float distance = foundHit ? closestHit.distance : Mathf.Infinity;

        verticalVelocity -= gravity * Time.fixedDeltaTime;
        float downwardMovement = verticalVelocity * Time.fixedDeltaTime;
        if (currentOrb != null && jumpAction.IsPressed())
        {
            if (currentOrb.type == Orb.OrbType.Black)
            {
                verticalVelocity = -Mathf.Sqrt(2 * gravity * -currentOrb.orbJumpHeight);
            }
            else
            {
                if (currentOrb.orbJumpHeight >= 0)
                {
                    verticalVelocity = Mathf.Sqrt(2 * gravity * currentOrb.orbJumpHeight);
                }
            }
            downwardMovement = verticalVelocity * Time.fixedDeltaTime;
        }
        else
        {
            if (jumpAction.IsPressed() && isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);
                downwardMovement = verticalVelocity * Time.fixedDeltaTime;
            }
        }
        if (distance < Mathf.Abs(downwardMovement) && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
            verticalVector = Vector2.down * distance;
        }
        else
        {
            verticalVector = Vector2.up * downwardMovement;
        }
    }
    private void PlayerRotate()
    {
        float timeToPeak = Mathf.Sqrt(2f * jumpHeight * gravity) / gravity;
        float fullFlightTime = timeToPeak * 2f;

        float playerRotationSpeed = (fullFlightTime > 0) ? 180f / fullFlightTime : 360f;
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
    private void PlayerGroundCast()
    {
        RaycastHit2D closestHit = new RaycastHit2D();
        Vector2 normal = Vector2.up;
        float minDistance = Mathf.Infinity;
        bool isGrounded = false;

        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(playerBounds.center, transform.localScale, boxCastAngle, boxCastDirection, boxCastOffset))
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider == playerCollider) continue;
            if (hit.distance < minDistance && Vector2.Angle(Vector2.up, hit.normal) <= maxSlopeAngle)
            {
                minDistance = hit.distance;
                closestHit = hit;
                normal = hit.normal;
                isGrounded = true;
            }
        }

        this.isGrounded = isGrounded;
        groundNormal = normal;
    }
    private void PlayerGroundCastAlignmentAngle()
    {
        RaycastHit2D closestHit = GetClosestGroundHit(Physics2D.BoxCastAll(playerBounds.center, new Vector2(1, 1), 0f, Vector2.down, supportBoxCastDistance));
        Vector2 normal = closestHit ? closestHit.normal : Vector2.up;

        
        boxCastAngle = Vector2.SignedAngle(Vector2.up, normal);
        boxCastDirection = -closestHit.normal;
    }
    private RaycastHit2D GetClosestGroundHit(RaycastHit2D[] hits)
    {
        RaycastHit2D closestHit = new RaycastHit2D();
        float minDistance = Mathf.Infinity;

        foreach(RaycastHit2D hit in hits)
        {
            if (hit.distance < minDistance && Vector2.Angle(Vector2.up, hit.normal) <= maxSlopeAngle)
            {
                minDistance = hit.distance;
                closestHit = hit;
            }
        }
        return closestHit;
    }
    private void OnDrawGizmos()
    {
        Bounds boundsForGizmo = playerCollider.bounds;
        DrawBoxCast2D(boundsForGizmo.center, transform.localScale, 0f, Vector2.down, supportBoxCastDistance, Color.green);
        DrawBoxCast2D(boundsForGizmo.center, transform.localScale, boxCastAngle, Vector2.down, boxCastOffset, Color.red);
        DrawBoxCast2D(playerBounds.center, transform.localScale, boxCastAngle, Vector2.down, 10f, Color.yellow);
        DrawBoxCast2D(playerBounds.center, transform.localScale, 0f, Vector2.zero, 0f, Color.violet);
        DrawBoxCast2D(playerBounds.center, transform.localScale * 0.25f, 0f, Vector2.zero, 0F, Color.blue);
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

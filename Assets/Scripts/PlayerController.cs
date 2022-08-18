using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nebula;

public class PlayerController : MonoBehaviour
{
    private Transform transform;
    private Rigidbody2D playerBody;
    private BoxCollider2D playerUpperCollider;
    private SpriteRenderer sprite;

    // All state machine vaiables.
    private GStateMachine stateMachine;
    private GDelegateState idleState;
    private GDelegateState runningState;
    private GDelegateState sprintingState;
    private GDelegateState jumpState;
    private GDelegateState airborneState;
    private GDelegateState wallStallState;
    private GDelegateState wallJumpState;

    private Animator animator;

    private bool isGrounded = false;
    private float lengthOfAnklesRay = 0.2f;

    private bool isAgainstWall = false;
    private float lengthOfChestRay = 0.2f;

    private void Awake()
    {
        this.transform = GetComponent<Transform>();
        this.playerBody = GetComponent<Rigidbody2D>();
        this.playerUpperCollider = GetComponent<BoxCollider2D>();
        this.animator = GetComponent<Animator>();
        this.sprite = GetComponent<SpriteRenderer>();

        // Initialize the player states w/ the state machine
        this.stateMachine = new GStateMachine();

        int[] array1 = { 1, 3 };
        idleState = new GDelegateState(IdleStateCondition, IdleStateAction,
                                       null, LeaveIdleState,
                                       IdleUpdate,
                                       0, "Idle", array1.ToList(), 0,
                                       animator, "Idle");
        int[] array2 = { 2, 3, 4 };
        runningState = new GDelegateState(RunningStateCondition, RunningStateAction,
                                          EnterRunningState, LeaveRunningState,
                                          RunningUpdate,
                                          1, "Running", array2.ToList(), 1,
                                          animator, "Running");
        int[] array3 = { 1, 3, 4 };
        sprintingState = new GDelegateState(SprintingStateCondition, SprintingStateAction,
                                            EnterSprintingState, LeaveSprintingState,
                                            SprintingUpdate,
                                            2, "Sprinting", array3.ToList(), 2,
                                            animator, "Sprinting");
        int[] array4 = { 4 };
        jumpState = new GDelegateState(JumpingStateCondition, JumpingStateAction,
                                       EnterJumpingState, LeaveJumpingState,
                                       JumpingUpdate,
                                       3, "Jumping", array4.ToList(), 3,
                                       animator, "Jumping");
        int[] array5 = { 0, 1, 2, 5 };
        airborneState = new GDelegateState(AirborneStateCondition, AirborneStateAction,
                                           EnterAirborneState, LeaveAirborneState,
                                           AirborneUpdate,
                                           4, "Airborne", array5.ToList(), 4,
                                           animator, "Airborne");
        int[] array6 = { 0, 4, 6 };
        wallStallState = new GDelegateState(WallStallStateCondition, WallStallStateAction,
                                            EnterWallStallState, LeaveWallStallState,
                                            WallStallUpdate,
                                            5, "Wall Stall", array6.ToList(), 5,
                                            animator, "Wall Stall");

        int[] array7 = { 0, 4 };
        wallJumpState = new GDelegateState(WallJumpStateCondition, WallJumpStateAction,
                                            EnterWallJumpState, LeaveWallJumpState,
                                            WallJumpUpdate,
                                            6, "Wall Jump", array7.ToList(), 6,
                                            animator, "Wall Jump");

        this.stateMachine.SetIdleState(idleState);
        this.stateMachine.AddState(runningState);
        this.stateMachine.AddState(sprintingState);
        this.stateMachine.AddState(jumpState);
        this.stateMachine.AddState(airborneState);
        this.stateMachine.AddState(wallStallState);
        this.stateMachine.AddState(wallJumpState);

        // Turn these all into 'configureBLANKstate'
        SetJumpVariables();
        SetUpAirborneActions();
    }

    private void Start()
    {
        // Set waypoint to walk to maybe? Like intro animation...
    }

    private void Update()
    {
        this.stateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        this.stateMachine.PerformStateAction();
        HandleGrounded();
    }

    // Note: If it ever seems like the player is stopping too fast make sure the slippery material
    // has a friction of 0.1.
    void HandleGrounded()
    {
        // RayCast from player's ankles (Make sure this ray doesn't hit the player's box collider)
        Vector2 locationOfAnkles = new Vector2(transform.position.x,
                                              transform.position.y - 0.70f);
        // Check if it hits anything
        RaycastHit2D hit = Physics2D.Raycast(locationOfAnkles, -Vector2.up, lengthOfAnklesRay);
        Debug.DrawRay(locationOfAnkles, -Vector2.up * lengthOfAnklesRay, Color.red);

        if (hit.collider && hit.collider.tag == "Enviornment")
        {
            if (!this.isGrounded)
            {
                // Force feet on ground
                Vector2 position = playerBody.position;
                position.y = (hit.point.y + 0.75f);
                playerBody.position = position;
            }
            this.isGrounded = true;
        }
        else
        {
            this.isGrounded = false;
        }
    }

    void HandleAgainstWall()
    {
        // Create a +1 or -1 multiplier based on the direction the player is facing.
        int facingMultiplier = 1;
        if (sprite.flipX)
        {
            facingMultiplier = -1;
        }
        // RayCast from player's chest (Make sure this ray doesn't hit the player's box collider)
        Vector2 locationOfChest = new Vector2(transform.position.x + (0.25f * facingMultiplier),
                                              transform.position.y);
        // Check if it hits anything
        RaycastHit2D hit = Physics2D.Raycast(locationOfChest, Vector2.right * facingMultiplier, lengthOfChestRay);
        Debug.DrawRay(locationOfChest, Vector2.right * lengthOfChestRay * facingMultiplier, Color.red);

        if (hit.collider && hit.collider.tag == "Enviornment")
        {
            if (!this.isAgainstWall)
            {
                // Force chest to wall
                Vector2 position = playerBody.position;
                position.x = (hit.point.x - (0.25f * facingMultiplier));
                playerBody.position = position;
            }
            this.isAgainstWall = true;
        }
        else
        {
            this.isAgainstWall = false;
        }
    }

    // This points the player in the direction that is inputed.
    private void HandlePlayerFacingDirection()
    {
        if (Input.GetAxis("Horizontal") < 0)
        {
            sprite.flipX = true;
        }
        else if (Input.GetAxis("Horizontal") > 0)
        {
            if (sprite.flipX)
            {
                sprite.flipX = false;
            }
        }
    }

    #region Idle State Implementations

    [SerializeField] private float stoppingForce;

    // These two bools are needed so that stopping the character doesn't get into an infinte loop.
    private bool stoppingPositive = false;
    private bool stoppingNegative = false;

    private bool IdleStateCondition()
    {
        return isGrounded;
    }

    private void IdleStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Idle");

        // If player is moving. Stop them.
        if (playerBody.velocity.x > 0.1f && !stoppingNegative)
        {
            playerBody.AddForce(new Vector2(-stoppingForce, 0));
            stoppingPositive = true;
        }
        else if (playerBody.velocity.x < -0.1f && !stoppingPositive)
        {
            playerBody.AddForce(new Vector2(stoppingForce, 0));
            stoppingNegative = true;
        }
        else
        {
            playerBody.velocity = Vector2.zero;
        }
    }

    private void IdleUpdate()
    {
        // None
    }

    private void EnterIdleState()
    {
        // None
    }

    private void LeaveIdleState()
    {
        stoppingPositive = false;
        stoppingNegative = false;
    }

    #endregion

    #region Running State Implementations

    [SerializeField] private float runForce;
    [SerializeField] private float maxSpeed;

    private bool RunningStateCondition()
    {
        return Input.GetAxis("Horizontal") != 0;
    }

    private void RunningStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Running");

        // Get target speed and speed difference from that target.
        float targetSpeed = Input.GetAxis("Horizontal") * maxSpeed;
        float speedDifference = targetSpeed - playerBody.velocity.x;

        // Get the base running force in the correct direction.
        float horizontalForce = runForce * Input.GetAxis("Horizontal");

        // Make the player accelerate faster if they have to turn around.
        horizontalForce = Mathf.Abs(speedDifference / maxSpeed) > 1 ?
                          horizontalForce + stoppingForce * Input.GetAxis("Horizontal") :
                          horizontalForce;
        // Note: Player must have a small amount of friction so they doesn't get stuck at max speed.
        if (playerBody.velocity.x >= maxSpeed || playerBody.velocity.x <= -maxSpeed)
        {
            horizontalForce = 0;
        }

        // Slow the player down if their speed is too high.
        if (playerBody.velocity.x >= maxSpeed)
        {
            horizontalForce = -stoppingForce;
        }
        else if (playerBody.velocity.x <= -maxSpeed)
        {
            horizontalForce = stoppingForce;
        }

        playerBody.AddForce(new Vector2(horizontalForce, 0));
    }

    private void RunningUpdate()
    {
        HandlePlayerFacingDirection();
    }

    private void EnterRunningState()
    {
        // None
    }

    private void LeaveRunningState()
    {
        // None
    }

    #endregion

    #region Sprinting State Implementations

    [SerializeField] private float sprintBoost;

    private bool SprintingStateCondition()
    {
        return Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Horizontal") != 0;
    }

    private void SprintingStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Sprinting");

        // Get target speed and speed difference from that target.
        float targetSpeed = Input.GetAxis("Horizontal") * (maxSpeed + sprintBoost);
        float speedDifference = targetSpeed - playerBody.velocity.x;

        // Get the base running force in the correct direction.
        float horizontalForce = runForce * Input.GetAxis("Horizontal");

        // Make the player accelerate faster if they have to turn around.
        horizontalForce = Mathf.Abs(speedDifference / (maxSpeed + sprintBoost)) > 1 ?
                          horizontalForce + stoppingForce * Input.GetAxis("Horizontal") :
                          horizontalForce;
        // Note: Player must have a small amount of friction so they doesn't get stuck at max speed.
        if (playerBody.velocity.x >= (maxSpeed + sprintBoost) ||
            playerBody.velocity.x <= -(maxSpeed + sprintBoost))
        {
            horizontalForce = 0;
        }

        // Slow the player down if their speed is too high.
        if (playerBody.velocity.x >= (maxSpeed + sprintBoost))
        {
            horizontalForce = -stoppingForce;
        }
        else if (playerBody.velocity.x <= -(maxSpeed + sprintBoost))
        {
            horizontalForce = stoppingForce;
        }

        playerBody.AddForce(new Vector2(horizontalForce, 0));
    }

    private void SprintingUpdate()
    {
        HandlePlayerFacingDirection();
    }

    private void EnterSprintingState()
    {
        // None
    }

    private void LeaveSprintingState()
    {
        // None
    }

    #endregion

    #region Jumping State Implementations

    [SerializeField] private float MaxJumpHieght;
    [SerializeField] private float JumpForce;
    private float MaxJumpTime; // Calculated based on the two above variables.
    private int maxFixedUpdateCallsForJump; // From fixed update time step and the max jump time.
    private bool jumpAllowed = true;
    private int currentNumberOfJumpCalls = 0;
    private void SetJumpVariables()
    {
        // Caclulate the maximum time a player can hold the jump button.
        float gravity = Physics2D.gravity.magnitude;
        float numerator = 2.0f * MaxJumpHieght * gravity;
        float denominator = Mathf.Pow(JumpForce, 2) - JumpForce * gravity;
        MaxJumpTime = Mathf.Sqrt(numerator / denominator);
        maxFixedUpdateCallsForJump = (int)(MaxJumpTime / Time.fixedDeltaTime);
        Debug.Log("Player Jump, Max Input Time: " + MaxJumpTime);
        Debug.Log("Max Number of FixedUpdate calls for jump: " + maxFixedUpdateCallsForJump);
    }
    private bool JumpingStateCondition()
    {
        return Input.GetKey(KeyCode.Space);
    }

    private void JumpingStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Jumping");
        currentNumberOfJumpCalls += 1;

        // Uses impulse (force applied over a time duration) to allow for varying jump height.
        if (JumpInProgress())
        {
            playerBody.AddForce(new Vector2(0, JumpForce));
        }
    }

    private void JumpingUpdate()
    {
        // Exit early if they release the jump button early
        if (!Input.GetKey(KeyCode.Space))
        {
            jumpAllowed = false;
        }
    }

    private void EnterJumpingState()
    {
        jumpAllowed = true;
        currentNumberOfJumpCalls = 0;
    }

    private void LeaveJumpingState()
    {
        jumpAllowed = false;
    }

    // Helper function to allow for impulse.
    // Note: It runs pretty smooth both ways with a Time -> Fixed Timestep of 0.005.
    // But it runs consistently with the call counting method at the default time step 
    // of 0.02.
    private bool JumpInProgress()
    {
        return (currentNumberOfJumpCalls <= maxFixedUpdateCallsForJump && jumpAllowed);
        // Below is the implementation using just time.
        // float deltaTime = Time.time - jumpState.startTime;
        // return (deltaTime <= MaxJumpTime && jumpAllowed);
    }

    #endregion

    #region Airborne State Implementations

    public float airborneControlForce;
    [SerializeField] private float airborneMaxHorizontalSpeed;

    // Note: Whenever you change actions make sure to reset the current action.
    public enum AirborneActions
    {
        JumpToMouse,
        Glide
    }
    private List<AirborneAction> _AirbornActions;
    private int _CurrentAction = (int)AirborneActions.JumpToMouse;
    private void SetUpAirborneActions()
    {
        _AirbornActions = new List<AirborneAction>();
        _AirbornActions.Add(new JumpToMouse(playerBody, 500));
        _AirbornActions.Add(new Glide(playerBody, 5));
    }
    public bool AirborneStateCondition()
    {
        return !isGrounded && !JumpInProgress() || playerBody.velocity.y < -0.1f;
    }

    public void AirborneStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Airborne");
        // Strategy Pattern for different airborne mechanics.
        _AirbornActions[_CurrentAction].PerformAction();

        // Handle air horizontal movement.
        float horizontalForce = 0;
        if (playerBody.velocity.x <= airborneMaxHorizontalSpeed &&
            Input.GetAxis("Horizontal") > 0)
        {
            horizontalForce = airborneControlForce;
        }
        else if (playerBody.velocity.x >= -airborneMaxHorizontalSpeed &&
                 Input.GetAxis("Horizontal") < 0)
        {
            horizontalForce = -airborneControlForce;
        }
        playerBody.AddForce(new Vector2(horizontalForce, 0));
    }

    private void AirborneUpdate()
    {
        HandlePlayerFacingDirection();
        HandleAgainstWall();
    }

    private void EnterAirborneState()
    {
        _AirbornActions[_CurrentAction].StartAction();
    }

    private void LeaveAirborneState()
    {
        _AirbornActions[_CurrentAction].ResetAction();
    }

    #endregion

    #region Wall Stall State Implementations

    private float originalGravityScale;
    [SerializeField] private float wallStallGravityScale;

    public bool WallStallStateCondition()
    {
        return this.isAgainstWall && !this.isGrounded;
    }

    public void WallStallStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Wall Stall");

        // If the player is sliding up the wall stop them faster than gravity does.
        if (playerBody.velocity.y > 0)
        {
            playerBody.velocity = Utils2D.SmoothVectorTo(playerBody.velocity, 0.8f, component: "y");
        }
    }

    private void WallStallUpdate()
    {
        HandleAgainstWall();
    }

    private void EnterWallStallState()
    {
        // Set the gravity scale for stall.
        originalGravityScale = playerBody.gravityScale;
        playerBody.gravityScale = wallStallGravityScale;
    }

    private void LeaveWallStallState()
    {
        playerBody.gravityScale = originalGravityScale;
    }

    #endregion

    #region Wall Jump State Implementations

    [SerializeField] private float _wallJumpForce;

    public bool WallJumpStateCondition()
    {
        // Create a +1 or -1 multiplier based on the direction the player is facing.
        int directionFacing = 1;
        if (sprite.flipX)
        {
            directionFacing = -1;
        }
        return Input.GetKey(KeyCode.Space) && Input.GetAxis("Horizontal") != directionFacing && Input.GetAxis("Horizontal") != 0;
    }

    public void WallJumpStateAction()
    {
        Utils2D.DisplayInfo(this.transform, "Wall Jump");


    }

    private void WallJumpUpdate()
    {
        HandlePlayerFacingDirection();
    }

    private void EnterWallJumpState()
    {
        Vector2 jumpVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jumpVector.Normalize();
        if (Input.GetAxis("Vertical") < 0)
        {
            playerBody.AddForce(jumpVector * (_wallJumpForce / 2));
        }
        else
        {
            playerBody.AddForce(jumpVector * _wallJumpForce);
        }


        // SET A DOWNWARD TERMINAL VELOCITY.
    }

    private void LeaveWallJumpState()
    {

    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nebula;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _playerBody;
    private SpriteRenderer _sprite;

    // All state machine vaiables.
    private GStateMachine _stateMachine;
    private GDelegateState _idleState;
    private GDelegateState _runningState;
    private GDelegateState _sprintingState;
    private GDelegateState _jumpState;
    private GDelegateState _airborneState;
    private GDelegateState _wallStallState;
    private GDelegateState _wallJumpState;

    private Animator _animator;

    // Grounded Variables
    private bool _isGrounded = false;
    [Header("Grounded Parameters")]
    [SerializeField] private float _lengthOfAnklesRay = 0.2f;
    [SerializeField] private float _anklesRayOffset = 0.7f;

    // Against Wall Variables
    private bool _isAgainstWall = false;
    [Space]
    [Header("Against Wall Parameters")]
    [SerializeField] private float _lengthOfChestRay = 0.3f;
    [SerializeField] private float _chestRayOffset = 0.25f;

    private void Awake()
    {
        this._playerBody = GetComponent<Rigidbody2D>();
        this._animator = GetComponent<Animator>();
        this._sprite = GetComponent<SpriteRenderer>();

        // Initialize the player states w/ the state machine
        this._stateMachine = new GStateMachine();

        int[] idleAllowedTransitions = { 1, 3 };
        _idleState = new GDelegateState(IdleStateCondition, IdleStateAction,
                                       null, LeaveIdleState,
                                       null,
                                       0, "Idle", idleAllowedTransitions.ToList(), 0,
                                       _animator, "Idle");

        int[] runningAllowedTransitions = { 2, 3, 4 };
        _runningState = new GDelegateState(RunningStateCondition, RunningStateAction,
                                          null, null,
                                          RunningUpdate,
                                          1, "Running", runningAllowedTransitions.ToList(), 1,
                                          _animator, "Running");

        int[] sprintingAllowedTransitions = { 1, 3, 4 };
        _sprintingState = new GDelegateState(SprintingStateCondition, SprintingStateAction,
                                            null, null,
                                            SprintingUpdate,
                                            2, "Sprinting", sprintingAllowedTransitions.ToList(), 2,
                                            _animator, "Sprinting");

        int[] jumpAllowedTransitions = { 4 };
        _jumpState = new GDelegateState(JumpingStateCondition, JumpingStateAction,
                                       EnterJumpingState, LeaveJumpingState,
                                       JumpingUpdate,
                                       3, "Jumping", jumpAllowedTransitions.ToList(), 3,
                                       _animator, "Jumping");

        int[] airborneAllowedTransitions = { 0, 1, 2, 5 };
        _airborneState = new GDelegateState(AirborneStateCondition, AirborneStateAction,
                                           EnterAirborneState, LeaveAirborneState,
                                           AirborneUpdate,
                                           4, "Airborne", airborneAllowedTransitions.ToList(), 4,
                                           _animator, "Airborne");

        int[] wallStallAllowedTransitions = { 0, 4, 6 };
        _wallStallState = new GDelegateState(WallStallStateCondition, WallStallStateAction,
                                            EnterWallStallState, LeaveWallStallState,
                                            WallStallUpdate, 5,
                                            "Wall Stall", wallStallAllowedTransitions.ToList(), 5,
                                            _animator, "Wall Stall");


        int[] wallJumpAllowedTransitions = { 0, 4 };
        _wallJumpState = new GDelegateState(WallJumpStateCondition, WallJumpStateAction,
                                            EnterWallJumpState, null,
                                            WallJumpUpdate,
                                            6, "Wall Jump", wallJumpAllowedTransitions.ToList(), 6,
                                            _animator, "Wall Jump");

        this._stateMachine.SetIdleState(_idleState);
        this._stateMachine.AddState(_runningState);
        this._stateMachine.AddState(_sprintingState);
        this._stateMachine.AddState(_jumpState);
        this._stateMachine.AddState(_airborneState);
        this._stateMachine.AddState(_wallStallState);
        this._stateMachine.AddState(_wallJumpState);

        // Turn these all into 'configureBLANKstate'
        SetJumpVariables();
        SetUpAirborneActions();
    }

    private void Update()
    {
        this._stateMachine.UpdateState();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentAirborneAction = (int)AirborneActions.JumpToMouse;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _currentAirborneAction = (int)AirborneActions.Glide;
        }
    }

    private void FixedUpdate()
    {
        this._stateMachine.PerformStateAction();
        HandleGrounded();
    }

    // Note: If it ever seems like the player is stopping too fast make sure the slippery material
    // has a friction of 0.1.
    void HandleGrounded()
    {
        // RayCast from player's ankles (Make sure this ray doesn't hit the player's box collider)
        Vector2 locationOfAnkles = new Vector2(transform.position.x,
                                              transform.position.y - _anklesRayOffset);
        // Check if it hits anything
        RaycastHit2D hit = Physics2D.Raycast(locationOfAnkles, -Vector2.up, _lengthOfAnklesRay);
        Debug.DrawRay(locationOfAnkles, -Vector2.up * _lengthOfAnklesRay, Color.red);

        if (hit.collider && hit.collider.tag == "Enviornment")
        {
            this._isGrounded = true;
        }
        else
        {
            this._isGrounded = false;
        }
    }

    void HandleAgainstWall()
    {
        // Create a +1 or -1 multiplier based on the direction the player is facing.
        int facingMultiplier = 1;
        if (_sprite.flipX)
        {
            facingMultiplier = -1;
        }
        // RayCast from player's chest (Make sure this ray doesn't hit the player's box collider)
        Vector2 locationOfChest = new Vector2(transform.position.x + (_chestRayOffset * facingMultiplier),
                                              transform.position.y);
        // Check if it hits anything
        RaycastHit2D hit = Physics2D.Raycast(locationOfChest, Vector2.right * facingMultiplier, _lengthOfChestRay);
        Debug.DrawRay(locationOfChest, Vector2.right * _lengthOfChestRay * facingMultiplier, Color.red);

        if (hit.collider && hit.collider.tag == "Enviornment")
        {
            if (!this._isAgainstWall)
            {
                // Force chest to wall
                Vector2 position = _playerBody.position;
                position.x = (hit.point.x - (_chestRayOffset * facingMultiplier));
                _playerBody.position = position;
            }
            this._isAgainstWall = true;
        }
        else
        {
            this._isAgainstWall = false;
        }
    }

    // This points the player in the direction that is inputed.
    private void HandlePlayerFacingDirection()
    {
        if (Input.GetAxis("Horizontal") < 0)
        {
            this._sprite.flipX = true;
        }
        else if (Input.GetAxis("Horizontal") > 0)
        {
            if (this._sprite.flipX)
            {
                this._sprite.flipX = false;
            }
        }
    }

    #region Idle State Implementations

    [Space]
    [Header("Idle State Parameters")]
    [SerializeField] private float _stoppingForce;

    // These two bools are needed so that stopping the character doesn't get into an infinte loop.
    private bool _stoppingPositive = false;
    private bool _stoppingNegative = false;

    private bool IdleStateCondition()
    {
        return this._isGrounded;
    }

    private void IdleStateAction()
    {
        Utils.DisplayInfo(transform, "Idle");

        // If player is moving. Stop them.
        if (_playerBody.velocity.x > 0.1f && !_stoppingNegative)
        {
            _playerBody.AddForce(new Vector2(-_stoppingForce, 0));
            _stoppingPositive = true;
        }
        else if (_playerBody.velocity.x < -0.1f && !_stoppingPositive)
        {
            _playerBody.AddForce(new Vector2(_stoppingForce, 0));
            _stoppingNegative = true;
        }
        else
        {
            _playerBody.velocity = Vector2.zero;
        }
    }

    private void LeaveIdleState()
    {
        _stoppingPositive = false;
        _stoppingNegative = false;
    }

    #endregion

    #region Running State Implementations

    [Space]
    [Header("Running State Parameters")]
    [SerializeField] private float _runForce;
    [SerializeField] private float _maxSpeed;

    private bool RunningStateCondition()
    {
        return Input.GetAxis("Horizontal") != 0;
    }

    private void RunningStateAction()
    {
        Utils.DisplayInfo(this.transform, "Running");

        // Get target speed and speed difference from that target.
        float targetSpeed = Input.GetAxis("Horizontal") * _maxSpeed;
        float speedDifference = targetSpeed - _playerBody.velocity.x;

        // Get the base running force in the correct direction.
        float horizontalForce = _runForce * Input.GetAxis("Horizontal");

        // Make the player accelerate faster if they have to turn around.
        horizontalForce = Mathf.Abs(speedDifference / _maxSpeed) > 1 ?
                          horizontalForce + _stoppingForce * Input.GetAxis("Horizontal") :
                          horizontalForce;
        // Note: Player must have a small amount of friction so they doesn't get stuck at max speed.
        if (_playerBody.velocity.x >= _maxSpeed || _playerBody.velocity.x <= -_maxSpeed)
        {
            horizontalForce = 0;
        }

        // Slow the player down if their speed is too high.
        if (_playerBody.velocity.x >= _maxSpeed)
        {
            horizontalForce = -_stoppingForce;
        }
        else if (_playerBody.velocity.x <= -_maxSpeed)
        {
            horizontalForce = _stoppingForce;
        }

        _playerBody.AddForce(new Vector2(horizontalForce, 0));
    }

    private void RunningUpdate()
    {
        HandlePlayerFacingDirection();
    }

    #endregion

    #region Sprinting State Implementations

    [Space]
    [Header("Sprinting State Parameters")]
    [SerializeField] private float _sprintBoost;

    private bool SprintingStateCondition()
    {
        return Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Horizontal") != 0;
    }

    private void SprintingStateAction()
    {
        Utils.DisplayInfo(this.transform, "Sprinting");

        // Get target speed and speed difference from that target.
        float targetSpeed = Input.GetAxis("Horizontal") * (_maxSpeed + _sprintBoost);
        float speedDifference = targetSpeed - _playerBody.velocity.x;

        // Get the base running force in the correct direction.
        float horizontalForce = _runForce * Input.GetAxis("Horizontal");

        // Make the player accelerate faster if they have to turn around.
        horizontalForce = Mathf.Abs(speedDifference / (_maxSpeed + _sprintBoost)) > 1 ?
                          horizontalForce + _stoppingForce * Input.GetAxis("Horizontal") :
                          horizontalForce;
        // Note: Player must have a small amount of friction so they doesn't get stuck at max speed.
        if (_playerBody.velocity.x >= (_maxSpeed + _sprintBoost) ||
            _playerBody.velocity.x <= -(_maxSpeed + _sprintBoost))
        {
            horizontalForce = 0;
        }

        // Slow the player down if their speed is too high.
        if (_playerBody.velocity.x >= (_maxSpeed + _sprintBoost))
        {
            horizontalForce = -_stoppingForce;
        }
        else if (_playerBody.velocity.x <= -(_maxSpeed + _sprintBoost))
        {
            horizontalForce = _stoppingForce;
        }

        _playerBody.AddForce(new Vector2(horizontalForce, 0));
    }

    private void SprintingUpdate()
    {
        HandlePlayerFacingDirection();
    }

    #endregion

    #region Jumping State Implementations

    [Space]
    [Header("Jumping State Parameters")]
    [SerializeField] private float _maxJumpHieght;
    [SerializeField] private float _jumpForce;
    private float _maxJumpTime; // Calculated based on the two above variables.
    private int _maxFixedUpdateCallsForJump; // From fixed update time step and the max jump time.
    private bool _jumpAllowed = true;
    private int _currentNumberOfJumpCalls = 0;
    private void SetJumpVariables()
    {
        // Caclulate the maximum time a player can hold the jump button.
        float gravity = Physics2D.gravity.magnitude;
        float numerator = 2.0f * _maxJumpHieght * gravity;
        float denominator = Mathf.Pow(_jumpForce, 2) - _jumpForce * gravity;
        _maxJumpTime = Mathf.Sqrt(numerator / denominator);
        _maxFixedUpdateCallsForJump = (int)(_maxJumpTime / Time.fixedDeltaTime);
        Debug.Log("Player Jump, Max Input Time: " + _maxJumpTime);
        Debug.Log("Max Number of FixedUpdate calls for jump: " + _maxFixedUpdateCallsForJump);
    }
    private bool JumpingStateCondition()
    {
        return Input.GetKey(KeyCode.Space);
    }

    private void JumpingStateAction()
    {
        Utils.DisplayInfo(this.transform, "Jumping");
        _currentNumberOfJumpCalls += 1;

        // Uses impulse (force applied over a time duration) to allow for varying jump height.
        if (JumpInProgress())
        {
            _playerBody.AddForce(new Vector2(0, _jumpForce));
        }
    }

    private void JumpingUpdate()
    {
        // Exit early if they release the jump button early
        if (!Input.GetKey(KeyCode.Space))
        {
            _jumpAllowed = false;
        }
    }

    private void EnterJumpingState()
    {
        _jumpAllowed = true;
        _currentNumberOfJumpCalls = 0;
    }

    private void LeaveJumpingState()
    {
        _jumpAllowed = false;
    }

    // Helper function to allow for impulse.
    // Note: It runs pretty smooth both ways with a Time -> Fixed Timestep of 0.005.
    // But it runs consistently with the call counting method at the default time step 
    // of 0.02.
    private bool JumpInProgress()
    {
        return (_currentNumberOfJumpCalls <= _maxFixedUpdateCallsForJump && _jumpAllowed);
        // Below is the implementation using just time.
        // float deltaTime = Time.time - _jumpState.startTime;
        // return (deltaTime <= _maxJumpTime && _jumpAllowed);
    }

    #endregion

    #region Airborne State Implementations

    [Space]
    [Header("Airborne State Parameters")]
    [SerializeField] private float _airborneMaxHorizontalSpeed;
    public float airborneControlForce;

    // Note: Whenever you change actions make sure to reset the current action.
    public enum AirborneActions
    {
        JumpToMouse,
        Glide
    }
    private List<AirborneAction> _AirbornActions;
    private int _currentAirborneAction = (int)AirborneActions.JumpToMouse;
    private void SetUpAirborneActions()
    {
        this._AirbornActions = new List<AirborneAction>();
        this._AirbornActions.Add(new JumpToMouse(_playerBody, 500));
        this._AirbornActions.Add(new Glide(_playerBody, 5));
    }
    public bool AirborneStateCondition()
    {
        return !_isGrounded && !JumpInProgress() || _playerBody.velocity.y < -0.1f;
    }

    public void AirborneStateAction()
    {
        Utils.DisplayInfo(this.transform, "Airborne");
        // Strategy Pattern for different airborne mechanics.
        _AirbornActions[_currentAirborneAction].PerformAction();

        // Handle air horizontal movement.
        float horizontalForce = 0;
        if (_playerBody.velocity.x <= _airborneMaxHorizontalSpeed &&
            Input.GetAxis("Horizontal") > 0)
        {
            horizontalForce = airborneControlForce;
        }
        else if (_playerBody.velocity.x >= -_airborneMaxHorizontalSpeed &&
                 Input.GetAxis("Horizontal") < 0)
        {
            horizontalForce = -airborneControlForce;
        }
        _playerBody.AddForce(new Vector2(horizontalForce, 0));
    }

    private void AirborneUpdate()
    {
        HandlePlayerFacingDirection();
        HandleAgainstWall();
    }

    private void EnterAirborneState()
    {
        _AirbornActions[_currentAirborneAction].StartAction();
    }

    private void LeaveAirborneState()
    {
        _AirbornActions[_currentAirborneAction].ResetAction();
    }

    #endregion

    #region Wall Stall State Implementations

    [Space]
    [Header("Wall Stall State Parameters")]
    [SerializeField] private float _wallStallGravityScale;
    private float _originalGravityScale;

    public bool WallStallStateCondition()
    {
        return this._isAgainstWall && !this._isGrounded;
    }

    public void WallStallStateAction()
    {
        Utils.DisplayInfo(this.transform, "Wall Stall");

        // If the player is sliding up the wall stop them faster than gravity does.
        if (_playerBody.velocity.y > 0)
        {
            _playerBody.velocity = Utils2D.SmoothVectorTo(_playerBody.velocity, 0.8f, component: "y");
        }
    }

    private void WallStallUpdate()
    {
        HandleAgainstWall();
    }

    private void EnterWallStallState()
    {
        // Set the gravity scale for stall.
        _originalGravityScale = _playerBody.gravityScale;
        _playerBody.gravityScale = _wallStallGravityScale;
    }

    private void LeaveWallStallState()
    {
        _playerBody.gravityScale = _originalGravityScale;
    }

    #endregion

    #region Wall Jump State Implementations

    [Space]
    [Header("Wall Jump State Parameters")]
    [SerializeField] private float _wallJumpForce;

    public bool WallJumpStateCondition()
    {
        // Create a +1 or -1 multiplier based on the direction the player is facing.
        int directionFacing = 1;
        if (_sprite.flipX)
        {
            directionFacing = -1;
        }
        return Input.GetKey(KeyCode.Space) && Input.GetAxis("Horizontal") != directionFacing && Input.GetAxis("Horizontal") != 0;
    }

    public void WallJumpStateAction()
    {
        Utils.DisplayInfo(this.transform, "Wall Jump");
    }

    private void WallJumpUpdate()
    {
        HandlePlayerFacingDirection();
    }

    private void EnterWallJumpState()
    {
        Vector2 jumpVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jumpVector.Normalize();

        // Set Velocity to zero to have consistent jumps.
        _playerBody.velocity = Vector2.zero;

        if (Input.GetAxis("Vertical") < 0)
        {
            _playerBody.AddForce(jumpVector * (_wallJumpForce / 2));
        }
        else
        {
            _playerBody.AddForce(jumpVector * _wallJumpForce);
        }
    }

    #endregion
}

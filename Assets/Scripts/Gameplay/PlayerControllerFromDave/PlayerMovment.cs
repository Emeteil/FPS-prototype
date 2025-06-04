using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private const float baseWalkSpeed = 6f;
    [SerializeField] private const float baseSprintSpeed = 8f;
    [SerializeField] private const float spintFOV = 75f;
    [SerializeField] private const float spintFOVpeed = 4f;
    [SerializeField] private const float baseCrouchSpeed = 3.5f;
    [SerializeField] private bool viewSpeedGUI = false;

    public float BaseWalkSpeed => baseWalkSpeed;
    public float BaseSprintSpeed => baseSprintSpeed;
    public float BaseCrouchSpeed => baseCrouchSpeed;

    private Dictionary<string, SpeedModifier> walkSpeedModifiers = new Dictionary<string, SpeedModifier>();
    private Dictionary<string, SpeedModifier> sprintSpeedModifiers = new Dictionary<string, SpeedModifier>();
    private Dictionary<string, SpeedModifier> crouchSpeedModifiers = new Dictionary<string, SpeedModifier>();

    public float walkSpeed { get; private set; }
    public float sprintSpeed { get; private set; }
    public float crouchSpeed { get; private set; }

    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;

    [SerializeField] private float groundDrag;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private bool jumper = false;

    bool readyToJump;

    [Header("Crounching")]
    [SerializeField] private float crouchYScale;
    [SerializeField] private float startYScale;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;

    private bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

    private RaycastHit slopeHit;
    private bool exitingSlope;

    [SerializeField] private Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public struct SpeedModifier
    {
        public float value;
        public bool isMultiplicative;

        public SpeedModifier(float value, bool isMultiplicative)
        {
            this.value = value;
            this.isMultiplicative = isMultiplicative;
        }
    }

    public enum SpeedType
    {
        Walk,
        Sprint,
        Crouch
    }

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private bool _block = false;
    private bool _ignorePause = false;

    public void Block(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = true;
        _block = true;
    }

    public void Unblock(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = false;
        _block = false;
    }

    private void UpdateActualSpeeds()
    {
        walkSpeed = CalculateModifiedSpeed(baseWalkSpeed, walkSpeedModifiers);
        sprintSpeed = CalculateModifiedSpeed(baseSprintSpeed, sprintSpeedModifiers);
        crouchSpeed = CalculateModifiedSpeed(baseCrouchSpeed, crouchSpeedModifiers);
    }

    private float CalculateModifiedSpeed(float baseSpeed, Dictionary<string, SpeedModifier> modifiers)
    {
        if (modifiers.Count == 0) return baseSpeed;

        float minMultiplier = 1f;
        float additive = 0f;

        foreach (var modifier in modifiers.Values)
        {
            if (modifier.isMultiplicative)
                minMultiplier = Mathf.Min(minMultiplier, modifier.value);
            else
                additive += modifier.value;
        }

        return (baseSpeed + additive) * minMultiplier;
    }

    public void AddSpeedModifier(string id, SpeedModifier modifier, SpeedType speedType)
    {
        switch (speedType)
        {
            case SpeedType.Walk:
                walkSpeedModifiers[id] = modifier;
                break;
            case SpeedType.Sprint:
                sprintSpeedModifiers[id] = modifier;
                break;
            case SpeedType.Crouch:
                crouchSpeedModifiers[id] = modifier;
                break;
        }

        UpdateActualSpeeds();
    }

    public void RemoveSpeedModifier(string id, SpeedType speedType)
    {
        switch (speedType)
        {
            case SpeedType.Walk:
                if (walkSpeedModifiers.ContainsKey(id))
                    walkSpeedModifiers.Remove(id);
                break;
            case SpeedType.Sprint:
                if (sprintSpeedModifiers.ContainsKey(id))
                    sprintSpeedModifiers.Remove(id);
                break;
            case SpeedType.Crouch:
                if (crouchSpeedModifiers.ContainsKey(id))
                    crouchSpeedModifiers.Remove(id);
                break;
        }

        UpdateActualSpeeds();
    }

    private void Start()
    {
        Pause.Instance.AddScript(this);
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        UpdateActualSpeeds();
    }

    public static PlayerMovment Instance { get; private set; }

    private void Awake()
    {
        startYScale = transform.localScale.y;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (_block) return;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_block) return;

        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        bool isJumpKeyPressed = Input.GetKeyDown(jumpKey);
        bool isJumpKeyHeld = jumper && Input.GetKey(jumpKey);
        bool canJump = readyToJump && grounded;

        if ((isJumpKeyPressed || isJumpKeyHeld) && canJump)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            CrouchDown();
        }

        if (Input.GetKeyUp(crouchKey))
        {
            CrouchUp();
        }
    }

    public void CrouchDown()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    public void CrouchUp()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.air;
        }

        // if (state == MovementState.sprinting)
        if (Input.GetKey(sprintKey) && state != MovementState.crouching && (horizontalInput != 0 || verticalInput != 0))
            CameraFOVManager.Instance.RequestFOVChange(spintFOV, spintFOVpeed, CameraFOVManager.PRIORITY_LOW, this);
        else
            CameraFOVManager.Instance.ReleaseFOVRequest(this);

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 8f && moveSpeed != 0)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        Vector3 jumpVector = transform.up * jumpForce;

        rb.AddForce(jumpVector, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void OnGUI()
    {
        if (viewSpeedGUI)
            GUI.Label(new Rect(0, 0, 300, 30), $"Speed = {Math.Round(rb.velocity.magnitude, 2)}");
    }

}

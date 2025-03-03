using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PaperPlaneController : MonoBehaviour
{
    [Header("Thrust, Speed and Boost")]
    public float thrust = 10f; // Initial launch force
    public float maxSpeed = 50f;
    public float boostMultiplier = 1.5f;
    
    [Header("Lift & Drag (Gliding and Air Resistance)")]
    public float liftMultiplier = 2f; // Strength of the lift force
    public float dragCoefficient = 0.5f; // Air resistance
    public float maxLiftForce = 10f;  // Limits how much lift can be applied
    public float minGlideSpeed = 5f;  // Minimum speed required to stay in air
    
    [Header("Gravity & Altitude")]
    public float gravityScale = 2f;
    public float maxAltitude = 50f;
    
    [Header("Rotation Control (Movement - Ascend, Descend, Left, Right)")]
    public float pitchSpeed = 15f;  // How much the plane can pitch up/down
    public float rollSpeed = 10f;  // Roll sensitivity
    public float yawSpeed = 10f;
    public float resetSpeed = 2f;
    [SerializeField] private float sideDriftMultiplier = 10f;

    [Header("Rotation Limits")]
    public float maxPitch = 30f;  // Limit for Nose Up/Down
    public float maxRoll = 50f;
    
    [Header("Trail Renderer & Speedline Effect")]
    public ParticleSystem speedlineEffect;
    public TrailRenderer trailRendererLeft;
    public TrailRenderer trailRendererRight;
    private Coroutine disableTrailCoroutine;
    
    [Header("Debugging")]
    public float currentSpeed;
    
    public TextMeshProUGUI speedText;
    public Vector2 moveInput { get; private set; }
    private bool boosting = false;
    private bool isLaunched = false;
    
    
    private Rigidbody rb;
    private PaperPlaneControls controls;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        controls = new PaperPlaneControls();
        
        trailRendererLeft.enabled = false;
        trailRendererRight.enabled = false;
        
        controls.FlightControls.Trail.performed += OnTrailEnable;
        controls.FlightControls.Trail.canceled += OnTrailDisable;
        
        controls.FlightControls.Enable();
    }

    void OnEnable() => controls.FlightControls.Enable();
    void OnDisable() => controls.FlightControls.Disable();
    
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (!context.performed)
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!boosting)
            {
                boosting = true;
                StartCoroutine(EnableSpeedlinesWithDelay(0.01f));
            }
        }
        else if (context.canceled)
        {
            boosting = false;
            speedlineEffect.Stop(); 
        }
    }

    public void OnLaunch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LaunchPlane();
        }
    }
    
    public void OnTrailEnable(InputAction.CallbackContext context)
    {
        if (disableTrailCoroutine != null)
        {
            StopCoroutine(disableTrailCoroutine);
        }
        trailRendererLeft.enabled = true;
        trailRendererRight.enabled = true;
        
    }

    public void OnTrailDisable(InputAction.CallbackContext context)
    {
        if (disableTrailCoroutine != null)
        {
            StopCoroutine(disableTrailCoroutine);
        }
        disableTrailCoroutine = StartCoroutine(DisableTrailAfterDelay(2f));
    }
    
    private IEnumerator EnableSpeedlinesWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (boosting)
        {
            speedlineEffect.Play();
        }
    }

    private IEnumerator DisableTrailAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        trailRendererLeft.enabled = false;
        trailRendererRight.enabled = false;
    }

    private void Update()
    {
        if (rb != null)
        {
            float speed =  Mathf.RoundToInt(rb.linearVelocity.magnitude);
            speedText.text = speed.ToString();
        }
    }

    void FixedUpdate()
    {
        if (isLaunched)
        {
            ApplyPhysics();
            HandleInput();
        }
    }
    
    void LaunchPlane()
    {
        if (!isLaunched || (currentSpeed < 0.01f && transform.position.y <= 0.6f))
        {
            isLaunched = true;
            rb.useGravity = true; 
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(transform.forward * thrust + Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    void ApplyPhysics()
    {
        Vector3 velocity = rb.linearVelocity;
        float altitude = transform.position.y;
        currentSpeed = velocity.magnitude;
        
        // Altitude Effects on Thrust & Drag
        float altitudeFactor = Mathf.Clamp((altitude - 30f) / 10f, 0f, 1f);
        float adjustedThrust = thrust * (1f - 0.3f * altitudeFactor); // Slightly reduce thrust
        float adjustedDrag = dragCoefficient * (1f + 0.5f * altitudeFactor); // Slightly increase drag
        
        // Boost Variables
        float boostedThrust = adjustedThrust; 
        float boostedDrag =  adjustedDrag;
        float boostedMaxSpeed = maxSpeed;
        
        
        // Boost Logic
        if (boosting)
        {
            boostedThrust *= 1.5f;   
            boostedDrag *= 0.5f;     
            boostedMaxSpeed *= boostMultiplier;
        }
        
        
        // Apply Thrust
        rb.AddForce(transform.forward * (boostedThrust * 0.5f), ForceMode.Force); //Base thrust Force

        // Apply Lift
        if (altitude < maxAltitude && currentSpeed > minGlideSpeed)
        {
            float liftForce = Mathf.Clamp(currentSpeed  * liftMultiplier, 0, maxLiftForce);
            rb.AddForce(transform.up * liftForce, ForceMode.Force);
        }
        
        
        //Gravity Adjustment
        if (altitude >= maxAltitude)
        {
             //transform.position = new Vector3(transform.position.x, maxAltitude, transform.position.z);
             rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, -0.5f, 0.2f), rb.linearVelocity.z);
             rb.AddForce(Vector3.down * gravityScale * 50f, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(Vector3.down * gravityScale, ForceMode.Force); // Normal gravity
        }
        
        // Smooth Descent Instead of Instant Speed Drop
        if (moveInput == Vector2.zero && !boosting)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, transform.forward * currentSpeed * 0.95f, Time.deltaTime * 2f);
        }
        
        
        //Adjust Speed Based on Pitch (Up / Down)
        float pitch = transform.eulerAngles.x;
        if (pitch > 180) pitch -= 360;
        
        if (pitch > 5f)  // Going up
        {
            float climbSlowdown = 1f + (altitude / 50f);  // Higher altitude = more slowdown
            rb.AddForce(transform.forward * liftMultiplier * climbSlowdown, ForceMode.Force);
        }
        else if (pitch < -5f) // 🔽 Nose Down
        {
            float descentSpeedMultiplier = 2f + (Mathf.Abs(pitch) / 10f) + (altitude / 50f);
            if (boosting)
            {
                descentSpeedMultiplier *= 1.2f; // Boost increases descent effect
            }
            rb.AddForce(transform.forward * boostedThrust * descentSpeedMultiplier, ForceMode.Acceleration);
        }
        
        // Move sideways based on roll angle
        float rollAngle = transform.eulerAngles.z;
        if (rollAngle > 180)
        {
            rollAngle -= 360; // Convert to -180 to 180
        }
        float lateralVelocity = -Mathf.Sin(Mathf.Deg2Rad * rollAngle) * sideDriftMultiplier * 0.5f;
        Vector3 targetVelocity = rb.linearVelocity + transform.right * lateralVelocity;
        targetVelocity.x *= 0.95f;
        targetVelocity.x = Mathf.Clamp(targetVelocity.x, -maxSpeed * 0.3f, maxSpeed * 0.5f);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 2f);
        

        // Apply Drag (air resistance)
        float dragForce = boostedDrag * currentSpeed  * currentSpeed ;
        rb.AddForce(-velocity.normalized * dragForce, ForceMode.Force);

        
        // Limit Speed
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, boostedMaxSpeed);
        currentSpeed = rb.linearVelocity.magnitude;
        
        Debug.Log($"Altitude: {transform.position.y}");
        
    }

    void HandleInput()
    {
        // **Get Current Rotation**
        Vector3 currentRotation = transform.eulerAngles;

        // Convert rotation to -180 to +180 range
        float pitch = (currentRotation.x > 180) ? currentRotation.x - 360 : currentRotation.x;
        float roll = (currentRotation.z > 180) ? currentRotation.z - 360 : currentRotation.z;
        float yaw = currentRotation.y;

        // **Pitch (Nose Up/Down)**
        float targetPitch = -moveInput.y * maxPitch;
        pitch = Mathf.Lerp(pitch, targetPitch, Time.deltaTime * pitchSpeed);

        // **Roll (Tilt Left/Right)**
        float targetRoll = -moveInput.x * maxRoll; // "A" rolls left, "D" rolls right
        roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * rollSpeed);
        
        // **Yaw Rotation (Turn Left/Right)**
        float yawChange = moveInput.x * yawSpeed * Time.deltaTime;
        yaw += yawChange;
        
        if (moveInput.x == 0)
        {
            roll = Mathf.Lerp(roll, 0, Time.deltaTime * resetSpeed);
        }
        
        if (Mathf.Abs(moveInput.x) < 0.1f)  
        {
            yaw = Mathf.Lerp(yaw, currentRotation.y, Time.deltaTime * resetSpeed * 2f);
        }
        

        // **Clamp Rotation to Limits**
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        roll = Mathf.Clamp(roll, -maxRoll, maxRoll);

        // **Apply Rotation**
        transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        
    }
}

using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] private float _baseSteeringSpeed;
    
    [Header("Camera")] 
    [SerializeField] private float _shoulderMaxOffset;
    [SerializeField] private float _timeToShoulderOffset;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private float _maxPovOffset;
    [SerializeField] private float _timeToPovOffset;
    [SerializeField] private float _timeToNeutral;
    [SerializeField] private Transform _pov;
    private Cinemachine3rdPersonFollow _3rdPersonFollow;
    
    [Header("Acceleration")] 
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private float _timeToAccelerate;
    [SerializeField] private float _brakeForce;
    [SerializeField] private float _naturalDeceleration;
    private float _acceleration;
    
    [Header("Speed")]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _minSpeed;
    private float _currentSpeedMultiplier;
    private float _groundSpeedMultiplier;
    private const float BASE_SPEED_MULTIPLIER = 1.0f;
    private const float BASE_GROUND_SPEED_MULTIPLIER = 1.0f;
    
    // Boosting
    private Coroutine _currentBoostCoroutine;
    
    [Header("Drifting")]
    [SerializeField] private float _baseGrip;
    [SerializeField] private float _driftGrip;
    [SerializeField] private float _steeringDriftMultiplier;
    private bool _isDrifting;
    private bool _beginDrifting;
    private bool _endDrifting;

    [Header("GroundCheck settings")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private float _gravity;
    [SerializeField] private float _groundAlignementSpeed;
    private bool _isOnGround;
    
    [Header("Current inputs")]
    public bool canMove;
    private Vector2 _directionInput;
    
    // Internal components 
    private Rigidbody _rb;
    private PlayerInputManager _playerInputManager;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerInputManager = GetComponent<PlayerInputManager>();
        _3rdPersonFollow = _virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        _currentSpeedMultiplier = BASE_SPEED_MULTIPLIER;
        _groundSpeedMultiplier = BASE_GROUND_SPEED_MULTIPLIER;
    }

    void Update()
    {
        // Pretend there are no inputs
        if (!canMove)
        {
            _directionInput = Vector2.zero;
            _beginDrifting = false;
            _endDrifting = false;
        }
        // Read inputs
        else
        {
            _directionInput = _playerInputManager.directionInput;
            if (_playerInputManager.driftPressed)
            {
                _beginDrifting = true;
            }
            else if (_playerInputManager.driftReleased)
            {
                _endDrifting = true;
            }
        }
    }

    public void Boost(float speedIncrease, float decayTime)
    {
        if (_currentBoostCoroutine != null)
        {
            StopCoroutine(_currentBoostCoroutine);
        }
        _currentBoostCoroutine = StartCoroutine(BoostCoroutine(speedIncrease, decayTime));
    }

    private IEnumerator BoostCoroutine(float speedIncrease, float decayTime)
    {
        _currentSpeedMultiplier = speedIncrease;
        // Force the player to boost forward
        _rb.velocity = _maxSpeed * _currentSpeedMultiplier * transform.forward;
        float remainingTime = decayTime;
        // Return to normal speed over time
        while (remainingTime > 0)
        {
            _currentSpeedMultiplier = Mathf.Lerp(BASE_SPEED_MULTIPLIER, speedIncrease, remainingTime/decayTime);
            remainingTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // Previous loop probably won't reset the value properly
        _currentSpeedMultiplier = BASE_SPEED_MULTIPLIER;
    }
    
    private void FixedUpdate()
    {
        // Used to know if the car is going forward or backward
        Vector3 localVelocity = transform.InverseTransformDirection(_rb.velocity);
        // Rounding bug
        if (_rb.velocity == Vector3.zero)
        {
            localVelocity = Vector3.zero;
        }
        
        // Check drifting state
        if (_beginDrifting)
        {
            _isDrifting = true;
            _beginDrifting = false;
        }
        
        // Steer with inputs
        float steeringSpeed = _isDrifting ? _baseSteeringSpeed * _steeringDriftMultiplier : _baseSteeringSpeed;
        transform.eulerAngles += _directionInput.x * Mathf.Sign(localVelocity.z) * steeringSpeed * Time.fixedDeltaTime * transform.up;
        
        #region Moving
        float targetSpeed;
        float maxForwardSpeed = _maxSpeed * _currentSpeedMultiplier * _groundSpeedMultiplier;
        float minBackwardSpeed = _minSpeed * _currentSpeedMultiplier * _groundSpeedMultiplier;

        // Check if we're going forward and reached max forward velocity
        if (_directionInput.y > 0 && localVelocity.z <= maxForwardSpeed)
        {
            float accelerationVariated = _maxAcceleration * _groundSpeedMultiplier;
            _acceleration += accelerationVariated * Time.fixedDeltaTime / _timeToAccelerate;
            _acceleration = Mathf.Clamp(_acceleration, -accelerationVariated, accelerationVariated);
            targetSpeed = maxForwardSpeed;
        }
        // Check if we're going backward and reached max backward velocity
        else if (_directionInput.y < 0 && localVelocity.z >= minBackwardSpeed)
        {
            _acceleration = _brakeForce;
            targetSpeed = minBackwardSpeed;
        }
        // Natural friction if we don't want to move
        else
        {
            _acceleration = _naturalDeceleration;
            targetSpeed = 0f;
        }
        
        // Ignore vertical movement
        Vector3 groundVelocity = Vector3.ProjectOnPlane(_rb.velocity, transform.up);
        float currentSpeed = groundVelocity.magnitude * Mathf.Sign(localVelocity.z);
        float newForwardSpeed =
            Mathf.MoveTowards(currentSpeed, targetSpeed, _acceleration * Time.fixedDeltaTime);
        
        // Grip represents how fast does the velocity direction follow the car orientation (1 = instantaneous, 0 = never)
        float currentGrip = _isDrifting ? _driftGrip : _baseGrip;
        
        // This makes the car drift 
        Vector3 moveDirection = Vector3.Lerp(groundVelocity.normalized * Mathf.Sign(localVelocity.z), transform.forward, currentGrip);
        
        // Final velocity computation
        _rb.velocity = moveDirection * newForwardSpeed;
        #endregion
        
        // Check drifting after moving to override velocity and make a kind of boost forward
        if (_endDrifting)
        {
            _rb.velocity = transform.forward * _rb.velocity.magnitude;
            _isDrifting = false;
            _endDrifting = false;
        }
        
        #region Adapt to ground
        // Ground check
        _isOnGround = Physics.Raycast(_groundCheck.position, -transform.up, out var hit, _groundCheckDistance, _groundLayer);
        
        if (_isOnGround)
        {
            // Align to ground
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, _groundAlignementSpeed * Time.fixedDeltaTime);
            // Ground modifiers
            if (hit.transform.TryGetComponent<Ground>(out var ground))
            {
                _groundSpeedMultiplier = ground.speedMultiplier;
            }
            else
            {
                _groundSpeedMultiplier = BASE_GROUND_SPEED_MULTIPLIER;
            }
        }
        else
        {
            // Bring down the car to the ground
            _rb.velocity += _gravity * Vector3.down;
            _groundSpeedMultiplier = BASE_GROUND_SPEED_MULTIPLIER;
        }
        #endregion
        
        #region Camera turning
        // Decide if we have to reset through normal position or if we're going to the side
        float timeTo = (_directionInput.x == 0 || Mathf.Sign(_directionInput.x) != Mathf.Sign(_pov.localPosition.x)) ? _timeToNeutral : _timeToPovOffset;
        // Move POV to accomodate for new trajectory
        float newX = Mathf.MoveTowards(
            _pov.localPosition.x,
            -_directionInput.x * _maxPovOffset,
            _maxPovOffset * Time.fixedDeltaTime/timeTo);
        _pov.localPosition.Set(newX, _pov.localPosition.y, _pov.localPosition.z);
        
        // Shoulder offset to see the car turning
        timeTo = (_directionInput.x == 0 || Mathf.Sign(_directionInput.x) != Mathf.Sign(_3rdPersonFollow.ShoulderOffset.x)) ? _timeToNeutral : _timeToShoulderOffset;
        _3rdPersonFollow.ShoulderOffset.x = Mathf.MoveTowards(
            _3rdPersonFollow.ShoulderOffset.x,
            _directionInput.x*_shoulderMaxOffset,
            _shoulderMaxOffset*Time.fixedDeltaTime/timeTo);
        #endregion
    }

    public void Reset()
    {
        _rb.velocity = Vector3.zero;
        _isDrifting = false;
    }
}

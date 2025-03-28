using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    private Rigidbody _rb;
    private PlayerInputManager _playerInputManager;
    [Header("Steering settings")]
    [SerializeField] private float _baseSteeringSpeed;
    private float _steeringSpeed;
    [Header("Camera settings")] 
    [SerializeField] private float _shoulderMaxOffset;
    [SerializeField] private float _timeToShoulderOffset;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    private Cinemachine3rdPersonFollow _3rdPersonFollow;
    [SerializeField] private float _maxPovOffset;
    [SerializeField] private float _timeToPovOffset;
    [SerializeField] private float _timeToNeutral;
    [SerializeField] private Transform _pov;
    
    [Header("Acceleration settings")] 
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private float _timeToAccelerate;
    [SerializeField] private float _brakeForce;
    [SerializeField] private float _naturalDeceleration;
    private float _acceleration;
    
    [Header("Speed settings")]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _minSpeed;
    private float _currentSpeedMultiplier;
    private float _groundSpeedMultiplier;
    private const float BASE_SPEED_MULTIPLIER = 1.0f;
    private const float BASE_GROUND_SPEED_MULTIPLIER = 1.0f;
    private Coroutine _currentBoostCoroutine;
    
    [Header("Drifting settings")]
    private bool _isDrifting;
    private bool _beginDrifting;
    private bool _endDrifting;
    [SerializeField] private float _baseGrip;
    [SerializeField] private float _driftGrip;
    [SerializeField] private float _steeringDriftMultiplier;
    private float _currentGrip;

    [Header("GroundCheck settings")]
    [SerializeField] private LayerMask _groundSpeedLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private float _gravity;
    private bool _isOnGround;
    
    [Header("Current inputs")]
    public bool canMove;
    private Vector2 _directionInput;

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
        if (!canMove)
        {
            _directionInput = Vector2.zero;
            _beginDrifting = false;
            _endDrifting = false;
        }
        // Reading inputs
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
        var remainingTime = decayTime;
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

        // Ground check
        _isOnGround = Physics.Raycast(_groundCheck.position, -transform.up, out var hit, _groundCheckDistance, _groundLayer);
        
        // Steer with inputs
        _steeringSpeed = _isDrifting ? _baseSteeringSpeed * _steeringDriftMultiplier : _baseSteeringSpeed;
        transform.eulerAngles += _directionInput.x * Mathf.Sign(localVelocity.z) * _steeringSpeed * Time.fixedDeltaTime * transform.up;
        
        #region Moving
        float targetSpeed;
        float maxForwardSpeed = _maxSpeed * _currentSpeedMultiplier * _groundSpeedMultiplier;
        float minBackwardSpeed = _minSpeed * _currentSpeedMultiplier * _groundSpeedMultiplier;

        // Check if we reached max forward velocity
        if (_directionInput.y > 0 && localVelocity.z <= maxForwardSpeed)
        {
            float accelerationVariated = _maxAcceleration * _groundSpeedMultiplier;
            _acceleration += accelerationVariated * Time.fixedDeltaTime / _timeToAccelerate;
            _acceleration = Mathf.Clamp(_acceleration, -accelerationVariated, accelerationVariated);
            targetSpeed = maxForwardSpeed;
        }
        // Check if we reached max backward velocity
        else if (_directionInput.y < 0 && localVelocity.z >= minBackwardSpeed)
        {
            _acceleration = _brakeForce;
            targetSpeed = minBackwardSpeed;
        }
        // Natural friction
        else
        {
            _acceleration = _naturalDeceleration;
            targetSpeed = 0f;
        }
        // Ignore vertical speed if we're airborne
        Vector3 relevantVelocity = _isOnGround ? _rb.velocity : new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        float currentSpeed = relevantVelocity.magnitude * Mathf.Sign(localVelocity.z);
        float newForwardSpeed =
            Mathf.MoveTowards(currentSpeed, targetSpeed, _acceleration * Time.fixedDeltaTime);

        // Grip represents how fast does the velocity direction follow the car orientation (1 = instantaneous, 0 = never)
        _currentGrip = _isDrifting ? _driftGrip : _baseGrip;
        // We ignore vertical movement
        Vector3 groundVelocity = Vector3.ProjectOnPlane(_rb.velocity, transform.up).normalized;
        Vector3 moveDirection = Vector3.Lerp(groundVelocity * Mathf.Sign(localVelocity.z), transform.forward, _currentGrip);
        _rb.velocity = moveDirection * newForwardSpeed;
        #endregion
        
        // Check drifting after moving to override velocity
        if (_endDrifting)
        {
            _rb.velocity = transform.forward * _rb.velocity.magnitude;
            _isDrifting = false;
            _endDrifting = false;
        }
        
        #region Adapt to ground
        if (_isOnGround)
        {
            // Align to ground
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, _baseSteeringSpeed * Time.fixedDeltaTime);
            // Ground modifiers
            Ground groundBelow = hit.transform.gameObject.GetComponent<Ground>();
            if (groundBelow != null)
            {
                _groundSpeedMultiplier = groundBelow.speedVariator;
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
        // Move POV to accomodate for new trajectory
        var timeTo = (_directionInput.x == 0 || Mathf.Sign(_directionInput.x) != Mathf.Sign(_pov.localPosition.x)) ? _timeToNeutral : _timeToPovOffset;
        var newX = Mathf.MoveTowards(
            _pov.localPosition.x,
            -_directionInput.x * _maxPovOffset,
            _maxPovOffset * Time.fixedDeltaTime/timeTo);
        var newPos = _pov.localPosition;
        newPos.x = newX;
        _pov.localPosition = newPos;
        
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

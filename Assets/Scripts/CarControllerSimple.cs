using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CarControllerSimple : MonoBehaviour
{
    private Rigidbody _rb;
    [Header("Steering settings")]
    [SerializeField] private float _steeringSpeed;
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
    [SerializeField] private float _breakForce;
    [SerializeField] private float _naturalDeceleration;
    private float _acceleration;
    
    [Header("Speed settings")]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _minSpeed;
    private float _currentSpeedMultiplier;
    private float _groundSpeedVariator;
    private const float BASE_SPEED_MULTIPLIER = 1.0f;
    private const float BASE_GROUND_SPEED_MULTIPLIER = 1.0f;

    [Header("GroundCheck settings")]
    [SerializeField] private LayerMask _groundSpeedLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private float _gravity;
    private bool _isOnGround;
    
    public Vector2 directionInput;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _3rdPersonFollow = _virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        directionInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    public void Boost(float speedIncrease, float decayTime)
    {
        StartCoroutine(BoostCoroutine(speedIncrease, decayTime));
    }

    public IEnumerator BoostCoroutine(float speedIncrease, float decayTime)
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
      
        #region Moving
        float targetSpeed;
        if (directionInput.y > 0)
        {
            float accelerationVariated = _maxAcceleration * _groundSpeedVariator;
            _acceleration += accelerationVariated * Time.fixedDeltaTime/_timeToAccelerate;
            _acceleration = Mathf.Clamp(_acceleration, -accelerationVariated, accelerationVariated);
            targetSpeed = _maxSpeed * _groundSpeedVariator;
        }
        else if (directionInput.y < 0)
        {
            _acceleration = _breakForce;
            targetSpeed = _minSpeed;
        }
        else
        {
            _acceleration = _naturalDeceleration;
            targetSpeed = 0f;
        }
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        float newForwardSpeed = Mathf.MoveTowards(localVelocity.z, targetSpeed, _acceleration * Time.fixedDeltaTime);
        _rb.velocity = transform.forward * newForwardSpeed;
        #endregion
        
        #region Ground check
        _isOnGround = Physics.Raycast(_groundCheck.position, -transform.up, out var hit, _groundCheckDistance, _groundLayer);
        if (_isOnGround)
        {
            // Align to ground
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, _steeringSpeed * Time.fixedDeltaTime);
            // Ground modifiers
            Ground groundBelow = hit.transform.gameObject.GetComponent<Ground>();
            if (groundBelow != null)
            {
                _groundSpeedVariator = groundBelow.speedVariator;
            }
            else
            {
                _groundSpeedVariator = BASE_GROUND_SPEED_MULTIPLIER;
            }
        }
        else
        {
            // Bring down the car to the ground
            _rb.velocity += _gravity * Vector3.down;
            _groundSpeedVariator = BASE_GROUND_SPEED_MULTIPLIER;
        }
        #endregion
        
        
        #region Steering + camera turning
        // Steer with inputs
        transform.eulerAngles += directionInput.x * Mathf.Sign(localVelocity.z) * _steeringSpeed * Time.fixedDeltaTime * transform.up;
        
        // Move POV to accomodate for new trajectory
        var timeTo = (directionInput.x == 0 || Mathf.Sign(-directionInput.x) != Mathf.Sign(_pov.localPosition.x)) ? _timeToNeutral : _timeToPovOffset;
        var newX = Mathf.MoveTowards(
            _pov.localPosition.x,
            -directionInput.x * _maxPovOffset,
            _maxPovOffset * Time.fixedDeltaTime/timeTo);
        var newPos = _pov.localPosition;
        newPos.x = newX;
        _pov.localPosition = newPos;
        
        // Shoulder offset to see the car turning
        timeTo = (directionInput.x == 0 || Mathf.Sign(directionInput.x) != Mathf.Sign(_3rdPersonFollow.ShoulderOffset.x)) ? _timeToNeutral : _timeToShoulderOffset;
        _3rdPersonFollow.ShoulderOffset.x = Mathf.MoveTowards(
            _3rdPersonFollow.ShoulderOffset.x,
            directionInput.x*_shoulderMaxOffset,
            _shoulderMaxOffset*Time.fixedDeltaTime/timeTo);
        #endregion
    }

    private void OnGUI()
    {
        GUILayout.Label($"x: {_rb.velocity.x:F}, y: {_rb.velocity.y:F}, z: {_rb.velocity.z:F}");
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        GUILayout.Label(localVelocity.ToString());
        GUILayout.Label($"_groundSpeedVariator : {_groundSpeedVariator:F}, _acceleration : {_acceleration:F}");
        GUILayout.Label($"_isOnground : {_isOnGround}");
    }
}

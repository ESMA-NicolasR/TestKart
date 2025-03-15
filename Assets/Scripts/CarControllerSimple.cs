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
    public float steering;
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

    [Header("GroundCheck settings")]
    [SerializeField] private LayerMask _groundSpeedLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance;
    private float _groundSpeedVariator;
    [SerializeField] private float _gravity;
    [SerializeField] private Transform _groundCheck1;
    [SerializeField] private Transform _groundCheck2;
    [SerializeField] private Transform _frontCheck;
    [SerializeField] private bool _frontOnGround;
    [SerializeField] private bool _rearOnGround;
    [SerializeField] private bool _hasWallInFront;
    
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
        var prevMaxSpeed = _maxSpeed;
        var newMaxSpeed = _maxSpeed*speedIncrease;
        _maxSpeed = newMaxSpeed;
        _rb.velocity = transform.forward * newMaxSpeed;
        while (decayTime > 0)
        {
            _maxSpeed = Mathf.Lerp(prevMaxSpeed, newMaxSpeed, decayTime);
            decayTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _maxSpeed = prevMaxSpeed;
    }
    
    private void FixedUpdate()
    {
        // Check ground
        bool hasHit = Physics.Raycast(transform.position, -transform.up, out var info, _groundCheckDistance, _groundSpeedLayer);
        if (hasHit)
        {
            Ground groundBelow = info.transform.gameObject.GetComponent<Ground>();
            if (groundBelow != null)
            {
                _groundSpeedVariator = groundBelow.speedVariator;
            }
            else
            {
                _groundSpeedVariator = 1;
            }
        }
        else
        {
            _groundSpeedVariator = 1;
        }

        
        // Moving
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
        
        // Gravity
        _frontOnGround = Physics.Raycast(_groundCheck1.position, -transform.up, out var infoFront, 0.1f, _groundLayer);
        _rearOnGround = Physics.Raycast(_groundCheck2.position, -transform.up, out var infoRear, 0.1f, _groundLayer);
        if (_frontOnGround || _rearOnGround)
        {
            // We good
        }
        else
        {
            _rb.velocity += _gravity * Vector3.down;
        }
        
        // Align to ground
        transform.rotation = Quaternion.FromToRotation(transform.up, infoFront.normal) * transform.rotation;
        
        // Steering
        transform.eulerAngles += directionInput.x * Mathf.Sign(localVelocity.z) * steering * Time.fixedDeltaTime * transform.up;
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
    }

    private void OnGUI()
    {
        GUILayout.Label($"x: {_rb.velocity.x:F}, y: {_rb.velocity.y:F}, z: {_rb.velocity.z:F}");
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        GUILayout.Label(localVelocity.ToString());
        GUILayout.Label($"_groundSpeedVariator : {_groundSpeedVariator:F}, _acceleration : {_acceleration:F}");
        GUILayout.Label($"_frontOnGround : {_frontOnGround}, _rearOnGround : {_rearOnGround}, _hasWallInFront:{_hasWallInFront}");
    }
}

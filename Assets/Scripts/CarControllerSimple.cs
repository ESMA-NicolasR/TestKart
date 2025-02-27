using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
    private float _speed;
    
    [Header("Power up settings")]
    [SerializeField] private float _mushroomIncreasedSpeed;
    [SerializeField] private float _mushroomDecayTime;
    private bool _isBoosting;
    private bool _canBoost;
    [SerializeField] private GameObject _bananaPrefab;
    private bool _canBanana;
    [SerializeField] private Transform _objectDrop;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_canBoost && !_isBoosting)
            {
                _canBoost = false;
                StartCoroutine(Boost(_mushroomIncreasedSpeed, _mushroomDecayTime));
            }else if (_canBanana)
            {
                _canBanana = false;
                PopBanana();
            }
        }
        
    }

    public void GetPowerUp()
    {
        if (Random.Range(0, 1) == 0)
        {
            _canBoost = true;
        }
        else
        {
            _canBanana = true;
        }
    }

    public void PopBanana()
    {
        Instantiate(_bananaPrefab, _objectDrop.position, transform.rotation);
    }

    public IEnumerator Boost(float speedIncrease, float decayTime)
    {
        _isBoosting = true;
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
        _isBoosting = false;
    }
    
    private void FixedUpdate()
    {
        
        /*_rb.velocity += directionInput.y * _acceleration * Time.fixedDeltaTime * transform.forward;
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        var xVelocity = localVelocity.x;
        var yVelocity = localVelocity.y;
        var zVelocity = localVelocity.z;
        zVelocity = Mathf.Clamp(zVelocity, -maxSpeed, maxSpeed);
        localVelocity = new Vector3(xVelocity, yVelocity, zVelocity);
        _rb.velocity = _rb.transform.TransformDirection(localVelocity);*/
        // Look up and down
        var xAngle = transform.eulerAngles.x;
        if (xAngle > 180) xAngle -= 360;
        xAngle = Mathf.Clamp(xAngle, -40, 40);
        var yAngle = transform.rotation.eulerAngles.y;
        var zAngle = 0f;
        transform.eulerAngles = new Vector3(xAngle, yAngle, zAngle);
        directionInput.x *= Mathf.Sign(directionInput.y);
        // Steering
        _rb.transform.eulerAngles += directionInput.x * steering * Time.fixedDeltaTime * transform.up;
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
        
        // Moving
        float targetSpeed;
        if (directionInput.y > 0)
        {
            _acceleration += _maxAcceleration * Time.fixedDeltaTime/_timeToAccelerate;
            targetSpeed = _maxSpeed;
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
        /*
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        var xVelocity = localVelocity.x;
        var yVelocity = localVelocity.y;
        var zVelocity = localVelocity.z;
        zVelocity = Mathf.Clamp(zVelocity, _minSpeed, _maxSpeed);
        localVelocity = new Vector3(xVelocity, yVelocity, zVelocity);
        _rb.velocity = _rb.transform.TransformDirection(localVelocity);*/

        //_rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
    }

    private void OnGUI()
    {
        GUILayout.Label($"x: {_rb.velocity.x:F}, y: {_rb.velocity.y:F}, z: {_rb.velocity.z:F}");
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        GUILayout.Label(localVelocity.ToString());
        GUILayout.Label($"_speed : {_speed:F}, _acceleration : {_acceleration:F}");
    }
}

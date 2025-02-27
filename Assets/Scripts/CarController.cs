using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CarController : MonoBehaviour
{
    private Rigidbody _rb;
    [Header("Steering settings")] 
    public float steering;
    
    [Header("Acceleration settings")] 
    [SerializeField] private AnimationCurve _accelerationCurve;
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private float _timeToAccelerate;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _timeToDecay;
    private float _acceleration;
    private float _accelerationInterpolator;
    
    [Header("Speed settings")]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _minSpeed;
    private float _speed;
    
    [Header("Power up settings")]
    [SerializeField] private float _mushroomIncreasedSpeed;
    [SerializeField] private float _mushroomDecayTime;
    private bool isBoosting;
    private bool canBoost;

    public Vector2 directionInput;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        directionInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting && canBoost)
        {
            canBoost = false;
            StartCoroutine(Boost(_mushroomIncreasedSpeed, _mushroomDecayTime));
        }
        
    }

    public IEnumerator Boost(float speedIncrease, float decayTime)
    {
        isBoosting = true;
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
        isBoosting = false;
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
        // Steering
        //_rb.AddTorque(directionInput.x * steering * transform.up, ForceMode.Acceleration);
        _rb.transform.eulerAngles += directionInput.x * steering * Time.fixedDeltaTime * transform.up;
        
        // Moving
        _accelerationInterpolator += directionInput.y * Time.fixedDeltaTime / _timeToAccelerate;
        _accelerationInterpolator = Mathf.Clamp01(_accelerationInterpolator);
        if (directionInput.y > 0)
        { // Accelerate
            _acceleration = _accelerationCurve.Evaluate(_accelerationInterpolator) * _maxAcceleration;
        }else if (directionInput.y < 0)
        { // Decelerate
            _acceleration = -_deceleration;
        }
        else
        { // Natural drag
            _acceleration = 0f;
            _accelerationInterpolator -= Time.fixedDeltaTime/_timeToDecay;
        }

        _rb.velocity += _acceleration * Time.fixedDeltaTime * transform.forward;
        _rb.velocity = transform.forward * Mathf.Clamp(_rb.transform.InverseTransformDirection(_rb.velocity).z, _minSpeed, _maxSpeed);
        
        
        //_rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
    }

    private void OnGUI()
    {
        GUILayout.Label($"x: {_rb.velocity.x:F}, y: {_rb.velocity.y:F}, z: {_rb.velocity.z:F}");
        var localVelocity = _rb.transform.InverseTransformDirection(_rb.velocity);
        GUILayout.Label(localVelocity.ToString());
        GUILayout.Label($"_speed : {_speed:F}, _acceleration : {_acceleration:F}, _accelerationInterpolator : {_accelerationInterpolator:F}");
    }
}

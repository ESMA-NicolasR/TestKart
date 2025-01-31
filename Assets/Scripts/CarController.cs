using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CarController : MonoBehaviour
{
    [Header("Steering settings")] 
    private Rigidbody _rb;
    public float steering;
    
    [Header("Acceleration settings")] 
    private float _acceleration;
    [SerializeField] private AnimationCurve _accelerationCurve;
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private float _timeToAccelerate;
    [SerializeField] private float _deceleration;
    private float _accelerationInterpolator;
    
    [Header("Speed settings")]
    private float _speed;
    public float maxSpeed;
    
    [Header("Power up settings")]
    [SerializeField] private float _mushroomIncreasedSpeed;
    private bool isBoosting;

    public Vector2 directionInput;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        directionInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting)
        {
            StartCoroutine(Boost());
        }
        
    }

    public IEnumerator Boost()
    {
        isBoosting = true;
        var decayTime = 1.0f;
        var prevMaxSpeed = maxSpeed;
        var newMaxSpeed = maxSpeed*10.0f;
        maxSpeed = newMaxSpeed;
        _rb.velocity = transform.forward * newMaxSpeed;
        while (decayTime > 0)
        {
            maxSpeed = Mathf.Lerp(prevMaxSpeed, newMaxSpeed, decayTime);
            decayTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        maxSpeed = prevMaxSpeed;
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
        // Steering
        var xAngle = transform.eulerAngles.x;
        if (xAngle > 180) xAngle -= 360;
        xAngle = Mathf.Clamp(xAngle, -40, 40);
        var yAngle = transform.rotation.eulerAngles.y;
        var zAngle = 0f;
        transform.eulerAngles = new Vector3(xAngle, yAngle, zAngle);
        _rb.transform.eulerAngles += directionInput.x * steering * Time.fixedDeltaTime * transform.up;
        
        // Moving
        _accelerationInterpolator += directionInput.y * Time.fixedDeltaTime/_timeToAccelerate;
        _accelerationInterpolator = Mathf.Clamp01(_accelerationInterpolator);
        _acceleration = _accelerationCurve.Evaluate(_accelerationInterpolator)*_maxAcceleration;
        _rb.velocity += _acceleration * Time.fixedDeltaTime * transform.forward;
        _rb.velocity = transform.forward * Mathf.Clamp(_rb.velocity.magnitude, 0f, maxSpeed);
        
        
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

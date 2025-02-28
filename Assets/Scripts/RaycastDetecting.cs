using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDetecting : MonoBehaviour
{
    private bool _doRaycast;
    public float maxRaycastRange;
    public LayerMask raycastLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _doRaycast = true;
        }
        
    }

    private void FixedUpdate()
    {
        if (_doRaycast)
        {
            
            
        }
    }
}

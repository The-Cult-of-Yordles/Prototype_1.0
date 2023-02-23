using System;
using UnityEngine;

public class ProceduralWalking : MonoBehaviour
{
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform beckRight;
    
    private void Start()
    {
        Vector3 dir = DecideRayCastDir();
        // decide ideal position by cast rays
        // initialize leg position to ideal position
        // initialize body position
        // initialize leg normal to body forward position
        // initialize body rotation
    }

    private void Update()
    {
        HandleBodyMovement();
        HandleLegMovement();
        HandleBodyRotation();
    }

    void HandleBodyMovement()
    {
        // give command based on input
        // move body based on command
    }

    void HandleLegMovement()
    {
        // do multiple raycast to get ideal point
        // check leg distance from ideal point
        // move leg if to far from ideal point, with damping
        // zig zag pattern
        DecideRayCastDir();
    }

    void HandleBodyRotation()
    {
        // rotate body based on average of up and forward direction of four legs
        // translate body based on average position of four legs
    }

    Vector3 DecideRayCastDir()
    {
        return Vector3.one;
    }

    Vector3 HandleRayCastHit(Vector3 start, Vector3 dir)
    {
        return Vector3.one;
    }

    void WalkForward()
    {
        
    }

    void WalkBackward()
    {
        
    }
    
    void WalkLeft()
    {
        
    }
    
    void WalkRight()
    {
        
    }
}

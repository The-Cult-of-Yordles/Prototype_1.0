using Unity.Mathematics;
using UnityEngine;

public class ProceduralWalking : MonoBehaviour
{
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backRight;
    

    [SerializeField] private Rigidbody rigid;
    [SerializeField] private GameObject debugSphere;

    private float idealForward = 2;
    private float idealBackward = 1.2f;
    private float idealSide = 1.5f;

    private float maxLegReach = 1.4f;
    private float normalBodyHeight = 1.5f;
    
    private float stepSize = 1.8f;
    private float anticipateOffsetFront = 0.7f;
    private float anticipateOffsetBack = 0.7f;

    private Vector3 lastFrontLeft;
    private Vector3 lastFrontRight;
    private Vector3 lastBackLeft;
    private Vector3 lastBackRight;
    
    // walking

    private void Start()
    {
        Vector3 dir = DecideRayCastDir();
        // decide ideal position by cast rays
        // initialize leg position to ideal position
        // initialize body position
        // initialize leg normal to body forward position
        // initialize body rotation

        
        // -right is forward
        // forward is up 
        // -up is right 
        

    }

    private void Update()
    {
        HandleBodyMovement();
        HandleLegMovement();
        HandleBodyRotation();
    }

    void HandleBodyMovement()
    {
        rigid.velocity = -transform.right * (400 * Time.deltaTime);
    }

    void HandleLegMovement()
    {
        // do multiple raycast to get ideal point
        // check leg distance from ideal point
        // move leg if to far from ideal point, with damping
        // zig zag pattern
        Vector3 idealFrontLeft = transform.position +
                                 -transform.right * idealForward +
                                 transform.up * idealSide;
        Vector3 frontLeftLeg = HandleRayCastHit(idealFrontLeft, -transform.forward);
        if ((frontLeftLeg - lastFrontLeft).magnitude > stepSize + 0.2)
        {
            frontLeft.transform.position = frontLeftLeg +
                                           rigid.velocity.normalized * anticipateOffsetFront;
            lastFrontLeft = frontLeftLeg;
        }
        
        Vector3 idealFrontRight = transform.position +
                                 -transform.right * idealForward +
                                 -transform.up * idealSide;
        Vector3 frontRightLeg = HandleRayCastHit(idealFrontRight, -transform.forward);
        if ((frontRightLeg - lastFrontRight).magnitude > stepSize)
        {
            frontRight.transform.position = frontRightLeg +
                                           rigid.velocity.normalized * anticipateOffsetFront;
            lastFrontRight = frontRightLeg;
        }
        
        Vector3 idealBackLeft = transform.position +
                                 transform.right * idealBackward +
                                 transform.up * idealSide;
        Vector3 backLeftLeg = HandleRayCastHit(idealBackLeft, -transform.forward);
        if ((backLeftLeg - lastBackLeft).magnitude > stepSize)
        {
            backLeft.transform.position = backLeftLeg +
                                           rigid.velocity.normalized * anticipateOffsetBack;
            lastBackLeft = backLeftLeg;
        }
        
        Vector3 idealBackRight = transform.position +
                                 transform.right * idealBackward +
                                 -transform.up * idealSide;
        Vector3 backRightLeg = HandleRayCastHit(idealBackRight, -transform.forward);
        if ((backRightLeg - lastBackRight).magnitude > stepSize)
        {
            backRight.transform.position = backRightLeg +
                                           rigid.velocity.normalized * anticipateOffsetBack;
            lastBackRight = backRightLeg;
        }
    }

    void HandleBodyRotation()
    {
        // rotate body based on average of up and forward direction of four legs
        // translate body based on average position of four legs
    }

    Vector3 DecideRayCastDir()
    {
        // down, forward, right, left, back
        return Vector3.one;
    }

    Vector3 HandleRayCastHit(Vector3 start, Vector3 dir)
    {
        Ray ray = new Ray();
        RaycastHit hit;
        ray.origin = start;
        ray.direction = dir;
        Physics.Raycast(ray, out hit);
        return hit.point;
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

    void DebugS(Vector3 pos)
    {
        Instantiate(debugSphere, pos, quaternion.Euler(0,0,0));
    }
}

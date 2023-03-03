using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ProceduralWalking : MonoBehaviour
{
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backRight;
    

    [SerializeField] private GameObject debugSphere;

    private float idealForward = 1.8f;
    private float idealBackward = 1f;
    private float idealSide = 1.5f;

    private float maxLegReach = 4f;
    private float normalBodyHeight = 0.8f;
    
    private float stepSize = 3f;

    private float walkRate = 0.3f;
    private float stepHeight = 0.8f;

    private int legCycleIteration = 30;

    private bool frontLeftBackLeftRunning = false;
    private bool backRightFrontRightRunning = false;
    
    // walking

    private void Start()
    {
        Vector3 dir = DecideRayCastDir();
        // decide ideal position by cast rays
        // initialize leg position to ideal position
        // initialize body position
        // initialize leg normal to body forward position
        // initialize body rotation


        StartCoroutine("FrontLeftBackLeft");
        // -right is forward
        // forward is up 
        // -up is right 
    }

    private void Update()
    {
        HandleBodyMovement();
        HandleLegMovement();
    }

    void HandleBodyMovement()
    {
        Vector3 p = backLeft.position + (frontLeft.position - backLeft.position) / 2.3f;
        Vector3 q = backRight.position + (frontRight.position - backRight.position) / 2.3f;
        Vector3 pos = (p + q) / 2;
        pos.y += normalBodyHeight;
        transform.position = pos;

        Vector3 n0 = GetRayCastNormal(frontLeft.position, Vector3.down);
        Vector3 n1 = GetRayCastNormal(frontRight.position, Vector3.down);
        Vector3 n2 = GetRayCastNormal(backLeft.position, Vector3.down);
        Vector3 n3 = GetRayCastNormal(backRight.position, Vector3.down);
        Vector3 n = ((n0 + n1 + n2 + n3) / 4).normalized;



    }

    void HandleLegMovement()
    {
        // do multiple raycast to get ideal point
        // check leg distance from ideal point
        // move leg if to far from ideal point, with damping
        // zig zag pattern
        if (!frontLeftBackLeftRunning)
        {
            frontLeftBackLeftRunning = true;
            StartCoroutine("FrontLeftBackLeft");
        }

        if (!backRightFrontRightRunning)
        {
            backRightFrontRightRunning = true;
            StartCoroutine("BackRightFrontRight");
        }
        
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
        start.y += 10;
        ray.origin = start;
        ray.direction = dir;
        Physics.Raycast(ray, out hit);
        return hit.point;
    }

    Vector3 GetRayCastNormal(Vector3 start, Vector3 dir)
    {
        Ray ray = new Ray();
        RaycastHit hit;
        start.y += 10;
        ray.origin = start;
        ray.direction = dir;
        Physics.Raycast(ray, out hit);
        return hit.normal;
    }

    IEnumerator FrontLeftBackLeft()
    {
        Vector3 start = frontLeft.position;
        Vector3 idealFrontLeft = transform.position -
                                 transform.right * idealForward +
                                 transform.up * idealSide;
        Vector3 end = HandleRayCastHit(
            idealFrontLeft - transform.right * stepSize, 
            Vector3.down
            );


        Vector3 mid = start + (end - start) / 2;
        mid.y += stepHeight;

        float lerpStep = walkRate / 2 / legCycleIteration;
        
        float lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            frontLeft.transform.position =
                Vector3.Lerp(start, mid, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }

        lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            frontLeft.transform.position =
                Vector3.Lerp(mid, end, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }
        
        // start backleft leg
        start = backLeft.position;
        Vector3 idealBackLeft = transform.position +
                                 transform.right * idealBackward +
                                 transform.up * idealSide;
        end = HandleRayCastHit(idealBackLeft - transform.right * stepSize, Vector3.down);

        mid = start + (end - start) / 2;
        mid.y += stepHeight;

        lerpStep = walkRate / 2 / legCycleIteration;
        
        lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            backLeft.transform.position =
                Vector3.Lerp(start, mid, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }

        lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            backLeft.transform.position =
                Vector3.Lerp(mid, end, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }

        frontLeftBackLeftRunning = false;
    }
    
    IEnumerator BackRightFrontRight()
    {
        Vector3 start = backRight.position;
        Vector3 idealBackRight = transform.position +
                                 transform.right * idealBackward -
                                 transform.up * idealSide;
        Vector3 end = HandleRayCastHit(idealBackRight - transform.right * stepSize, Vector3.down);

        Vector3 mid = start + (end - start) / 2;
        mid.y += stepHeight;

        float lerpStep = walkRate / 2 / legCycleIteration;
        
        float lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            backRight.transform.position =
                Vector3.Lerp(start, mid, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }

        lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            backRight.transform.position =
                Vector3.Lerp(mid, end, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }
        
        start = frontRight.position;
        Vector3 idealFrontRight = transform.position -
                                 transform.right * idealForward -
                                 transform.up * idealSide;
        end = HandleRayCastHit(idealFrontRight - transform.right * stepSize, Vector3.down);

        mid = start + (end - start) / 2;
        mid.y += stepHeight;

        lerpStep = walkRate / 2 / legCycleIteration;
        
        lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            frontRight.transform.position =
                Vector3.Lerp(start, mid, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }

        lerpVal = lerpStep;
        for (int i = 0; i < legCycleIteration; i++)
        {
            frontRight.transform.position =
                Vector3.Lerp(mid, end, lerpVal);
            lerpVal += lerpStep * 2 / walkRate;
            yield return new WaitForSeconds(lerpStep);
        }
        // start front right leg
        backRightFrontRightRunning = false;
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

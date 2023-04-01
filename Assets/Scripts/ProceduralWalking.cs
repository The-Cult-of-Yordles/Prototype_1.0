using System.Collections;
using UnityEngine;

public class ProceduralWalking : MonoBehaviour
{
    // targets
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backRight;
    
    // landing points
    private Vector3 idealFrontLeft;
    private Vector3 idealBackLeft;
    private Vector3 idealFrontRight;
    private Vector3 idealBackRight;

    // config
    private float maxLegReach = 5f;
    private float normalBodyHeight = 2.5f;
    
    private float walkRate = 0.01f;
    private int legCycleIteration = 15;

    private bool frontLeftBackLeftRunning = false;
    private bool backRightFrontRightRunning = false;

    private TestRig testRig;
    
    private void Start()
    {
        StartCoroutine("FrontLeftBackLeft");
        testRig = FindObjectOfType<TestRig>();
        // -right is forward
        // forward is up 
        // -up is right 
    }

    private void Update()
    {
        idealFrontLeft = testRig._fl;
        idealFrontRight = testRig._fr;
        idealBackLeft = testRig._bl;
        idealBackRight = testRig._br;
        
        HandleBodyMovement();
        HandleBodyRotation();
        HandleLegMovement();
    }

    void HandleBodyMovement()
    {
        Vector3 pos = (frontLeft.position + frontRight.position + backRight.position + backLeft.position) / 4f;
        pos += transform.up * normalBodyHeight;
        transform.position = pos;
    }

    void HandleBodyRotation()
    {
        Vector3 fr = frontRight.transform.position;
        Vector3 br = backRight.transform.position;
        Vector3 fl = frontLeft.transform.position;
        Vector3 bl = backLeft.transform.position;

        Vector3 normal = (
            Vector3.Cross(fl - bl, fr - fl).normalized +
            Vector3.Cross(br - fr, bl - br).normalized).normalized;
        
        Debug.DrawRay(transform.position, normal * 10, Color.green);

        Vector3 forward =
        (
            Vector3.Cross(normal, fl - fr).normalized +
            Vector3.Cross(normal, bl - br).normalized +
            (fl - bl).normalized +
            (fr - br).normalized
        ).normalized;
        
        Debug.DrawRay(transform.position, forward * 10, Color.blue);
        
        Quaternion rotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;

        // Vector3 right = Vector3.Cross(normal, forward);
        // Debug.DrawRay(transform.position, right * 10, Color.yellow);

        transform.rotation = rotation;
        
        Quaternion r = Quaternion.FromToRotation(transform.forward, forward) * transform.rotation;

        transform.rotation = r;
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

    IEnumerator FrontLeftBackLeft()
    {
        Vector3 start = frontLeft.position;
        Vector3 end = idealFrontLeft;
        float lerpStep = 1f / legCycleIteration;
        
        float lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            frontLeft.transform.position =
                Vector3.Lerp(start, end, lerpVal);
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }

        // start backleft leg
        start = backLeft.position;
        end = idealBackLeft;
        lerpStep = 1f / legCycleIteration;
        lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            backLeft.transform.position =
                Vector3.Lerp(start, end, lerpVal);
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }

        frontLeftBackLeftRunning = false;
    }
    
    IEnumerator BackRightFrontRight()
    {
        Vector3 start = backRight.position;
        Vector3 end = idealBackRight;
        float lerpStep = 1f / legCycleIteration;
        
        float lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            backRight.transform.position =
                Vector3.Lerp(start, end, lerpVal);
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }
        
        start = frontRight.position;
        end = idealFrontRight;
        lerpStep = 1f / legCycleIteration;
        lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            frontRight.transform.position =
                Vector3.Lerp(start, end, lerpVal);
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }
        // start front right leg
        backRightFrontRightRunning = false;
    }
}

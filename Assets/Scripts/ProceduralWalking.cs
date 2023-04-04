using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWalking : MonoBehaviour
{
    // targets
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backRight;

    // config
    private float maxLegReach = 5f;
    private float normalBodyHeight = 2f;
    
    private float walkRate = 0.01f;
    private int legCycleIteration = 15;
    private float walkHeight = 1.5f;

    private bool frontLeftBackLeftRunning = false;
    private bool backRightFrontRightRunning = false;

    private TestRig testRig;

    private Transform[] legs;
    private Vector3[] landings = new Vector3[4];

    private bool currCouroutineLegMoving = false;
    private Stack<int> c = new Stack<int>();

    private void Start()
    {
        testRig = FindObjectOfType<TestRig>();
        legs = new Transform[]
        {
            frontLeft,
            frontRight,
            backRight,
            backLeft
        };
        StartCoroutine("MoveLeg");
    }

    private void Update()
    {
        landings[0] = testRig._fl;
        landings[1] = testRig._fr;
        landings[2] = testRig._br;
        landings[3] = testRig._bl;
        
        HandleBodyMovement();
        HandleBodyRotation();
        // HandleLegMovement();
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

    IEnumerator MoveLeg()
    {
        bool flip = true;
        while (true)
        {
            for (int i = 0; i < legs.Length; i++)
            {
                bool curr = i % 2 == 0;

                if (curr == flip)
                {
                    continue;
                }

                c.Push(1);
                StartCoroutine("MoveOneLeg", i);
            }

            flip = !flip;
            yield return new WaitUntil(() => c.Count == 0);
        }
    }

    IEnumerator MoveOneLeg(int i)
    {
        Vector3 start = legs[i].transform.position;
        Vector3 end = landings[i];
        Vector3 mid = start + (end - start) / 2f + transform.up * walkHeight;
        
        if ((start - end).magnitude < 1)
        {
            c.Pop();
            yield break;
        }
        
        float lerpVal = 0;
        while (lerpVal < 1f)
        {
            lerpVal += 1f / legCycleIteration;

            legs[i].transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(start, mid, lerpVal),
                    Vector3.Lerp(mid, end, lerpVal),
                    lerpVal
                );
            yield return new WaitForSeconds(walkRate);
        }
        
        c.Pop();
    }
    
    IEnumerator FrontLeftBackLeft()
    {
        Vector3 start = frontLeft.position;
        Vector3 end = testRig._fl;
        Vector3 mid = start + (end - start) + transform.up * walkHeight;
        float lerpStep = 1f / legCycleIteration;

        if ((start - end).magnitude < 1)
        {
            frontLeftBackLeftRunning = false;
            yield break;
        }
        
        float lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            frontLeft.transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(start, mid, lerpVal),
                    Vector3.Lerp(mid, end, lerpVal),
                    lerpVal
                );
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }

        // start backleft leg
        start = backLeft.position;
        end = testRig._bl;
        mid = start + (end - start) + transform.up * walkHeight;
        lerpStep = 1f / legCycleIteration;
        lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            backLeft.transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(start, mid, lerpVal),
                    Vector3.Lerp(mid, end, lerpVal),
                    lerpVal
                );
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }

        frontLeftBackLeftRunning = false;
    }
    
    IEnumerator BackRightFrontRight()
    {
        Vector3 start = backRight.position;
        Vector3 end = testRig._br;
        Vector3 mid = start + (end - start) + transform.up * walkHeight;
        float lerpStep = 1f / legCycleIteration;
        
        if ((start - end).magnitude < 1)
        {
            backRightFrontRightRunning = false;
            yield break;
        }
        
        float lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            backRight.transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(start, mid, lerpVal),
                    Vector3.Lerp(mid, end, lerpVal),
                    lerpVal
                );
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }
        
        start = frontRight.position;
        end = testRig._fr;
        mid = start + (end - start) + transform.up * walkHeight;
        lerpStep = 1f / legCycleIteration;
        lerpVal = lerpStep;
        while (lerpVal < 1)
        {
            frontRight.transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(start, mid, lerpVal),
                    Vector3.Lerp(mid, end, lerpVal),
                    lerpVal
                );
            lerpVal += lerpStep;
            yield return new WaitForSeconds(walkRate);
        }
        // start front right leg
        backRightFrontRightRunning = false;
    }
    
}

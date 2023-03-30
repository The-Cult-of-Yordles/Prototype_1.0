using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TestRig : MonoBehaviour
{
    [SerializeField] private Transform frontRightJoint;
    [SerializeField] private Transform frontLeftJoint;
    [SerializeField] private Transform backRightJoint;
    [SerializeField] private Transform backLeftJoint;
    
    [SerializeField] InputAction forward;
    [SerializeField] InputAction backward;
    [SerializeField] InputAction left;
    [SerializeField] InputAction right;
    [SerializeField] InputAction turnLeft;
    [SerializeField] InputAction turnRight;

    [SerializeField] private GameObject ds0;
    [SerializeField] private GameObject ds1;
    [SerializeField] private GameObject ds2;
    [SerializeField] private GameObject ds3;
    [SerializeField] private GameObject ds4;

    public Vector3 _fr;
    public Vector3 _fl;
    public Vector3 _br;
    public Vector3 _bl;

    public float speed;
    public float turnSpeed;
    private float fixedHeight = 2f;
    private Vector3[] idealPoints;

    [SerializeField] private float jointReach;

    private Quaternion previousRotation;

    private Vector3[] rayDirs = new Vector3[]
    {
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        
        new Vector3(1, 1, 1),
        new Vector3(-1, 1, 1),
        new Vector3(1, -1, 1),
        new Vector3(1, 1, -1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, -1),
        new Vector3(-1, 1, -1),
        new Vector3(-1, -1, -1),
        
        new Vector3(1, 1, 0),
        new Vector3(1, 0, 1),
        new Vector3(0, 1, 1),
        
        new Vector3(-1, 1, 0),
        new Vector3(1, -1, 0),
        
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, -1),
        
        new Vector3(0, -1, 1),
        new Vector3(0, 1, -1),
        
        new Vector3(-1, -1, 0),
        new Vector3(-1, 0, -1),
        new Vector3(0, -1, -1),
    };

    void Start()
    {
        idealPoints = new Vector3[5];
        previousRotation = transform.rotation;
    }

    void Update()
    {
        int f = forward.ReadValue<float>() > 0.01f? 1 : 0;
        int b = backward.ReadValue<float>() > 0.01f? 1 : 0;
        int r = right.ReadValue<float>() > 0.01f? 1 : 0;
        int l = left.ReadValue<float>() > 0.01f? 1 : 0;

        Vector3 _forward = transform.forward * l;
        Vector3 _backward = -transform.forward * r;
        Vector3 _right = transform.right * f;
        Vector3 _left = -transform.right * b;

        Vector3 all = (_forward + _backward + _right + _left).normalized;
        
        bool rLeft = turnRight.ReadValue<float>() > 0.01f;
        bool rRight = turnLeft.ReadValue<float>() > 0.01f;

        
        UpdateRayDir();

        Vector3 p = GetClosestPoint(transform.position);
        ds0.transform.position = p;
        Vector3 upNormal = (transform.position - p).normalized;
        Vector3 averageNormal = upNormal;

        RaycastHit h0;
        RaycastHit h1;
        RaycastHit h2;
        RaycastHit h3;

        Ray r0 = new Ray(frontLeftJoint.position, -transform.up.normalized * 3);
        Ray r1 = new Ray(frontRightJoint.position, -transform.up.normalized * 3);
        Ray r2 = new Ray(backLeftJoint.position, -transform.up.normalized * 3);
        Ray r3 = new Ray(backRightJoint.position, -transform.up.normalized * 3);

        Physics.Raycast(r0, out h0);
        Physics.Raycast(r1, out h1);
        Physics.Raycast(r2, out h2);
        Physics.Raycast(r3, out h3);

        float dist = 5;
        if ((h0.point - frontLeftJoint.position).magnitude < dist)
        {
            averageNormal += (frontLeftJoint.position - h0.point).normalized;
        }
        if ((h1.point - frontRightJoint.position).magnitude < dist)
        {
            averageNormal += (frontRightJoint.position - h1.point).normalized;
        }
        if ((h2.point - backLeftJoint.position).magnitude < dist)
        {
            averageNormal += (backLeftJoint.position - h2.point).normalized;
        }
        if ((h3.point - backRightJoint.position).magnitude < dist)
        {
            averageNormal += (backRightJoint.position - h3.point).normalized;
        }

        averageNormal = averageNormal.normalized;
        
        // this handles rotation based on input
        if (rRight)
        {
            transform.rotation = quaternion.Euler(-transform.up * (Time.deltaTime * turnSpeed)) * transform.rotation;
        } else if (rLeft)
        {
            transform.rotation = quaternion.Euler(transform.up * (Time.deltaTime * turnSpeed)) * transform.rotation;
        }
        
        Quaternion rotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;
        // this handles rotation based on surface
        transform.rotation = rotation;

        // this handles fixed height
        transform.position = p + (transform.position - p).normalized * fixedHeight;
        // this handles movement
        transform.position += all * (speed * Time.deltaTime);
        
        HandleLandingPoints();
        
        // this handles rotation lerp
        previousRotation = transform.rotation;
    }

    private void HandleLandingPoints()
    {
        Vector3 fl = GetClosestPoint(frontLeftJoint.position, jointReach, 0);
        Vector3 fr = GetClosestPoint(frontRightJoint.position, jointReach, 0);
        Vector3 bl = GetClosestPoint(backLeftJoint.position, jointReach, 0);
        Vector3 br = GetClosestPoint(backRightJoint.position, jointReach, 0);

        _fr = fr;
        _fl = fl;
        _br = br;
        _bl = bl;

        ds1.transform.position = fl;
        ds2.transform.position = fr;
        ds3.transform.position = bl;
        ds4.transform.position = br;
    }


    private Vector3 GetClosestPoint(Vector3 jointPos, float maxRadius, int x)
    {
        Vector3 closest = Vector3.zero;
        float dist = 100;
        foreach (var dir in rayDirs)
        {
            RaycastHit hit;
            Ray ray = new Ray(jointPos, dir.normalized * maxRadius);
            if (Physics.Raycast(ray, out hit))
            {
                float d = (jointPos - hit.point).magnitude;
                if (d < dist && (hit.point - transform.position).magnitude > 2.5)
                {
                    dist = d;
                    closest = hit.point;
                }
            }
            Debug.DrawRay(jointPos, dir.normalized * maxRadius, Color.red);

        }
        return closest;
    }

    private Vector3 GetClosestPoint(Vector3 jointPos, float maxRadius = 3)
    {
        Collider[] c = Physics.OverlapSphere(jointPos, maxRadius);
        if (c.Length < 1)
        {
            Debug.Log("sphere not collide");
            return jointPos;
        }
        
        Vector3 p = c[0].ClosestPoint(jointPos);
        float closest = (transform.position - p).magnitude;
        foreach (var collider in c)
        {
            Vector3 _p = collider.ClosestPoint(transform.position);
            float closer = (transform.position - _p).magnitude;
            if (closer < closest + 0.1f)
            {
                p = _p;
            }
        }
        return p;
    }

    private void UpdateRayDir()
    {
        var position = transform.position;

        idealPoints[0] = position - transform.up;
        idealPoints[1] = position - transform.right + transform.forward - transform.up;
        idealPoints[2] = position + transform.right + transform.forward - transform.up;
        idealPoints[3] = position - transform.right - transform.forward - transform.up;
        idealPoints[4] = position + transform.right - transform.forward - transform.up;
    }
    
    private void OnEnable()
    {
        forward.Enable();
        backward.Enable();
        left.Enable();
        right.Enable();
        turnLeft.Enable();
        turnRight.Enable();
    }

    private void OnDisable()
    {
        forward.Disable();
        backward.Disable();
        left.Disable();
        right.Disable();
        turnLeft.Disable();
        turnRight.Disable();
    }
}

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

    [SerializeField] GameObject debugSphere;

    [SerializeField] private GameObject ds0;
    [SerializeField] private GameObject ds1;
    [SerializeField] private GameObject ds2;
    [SerializeField] private GameObject ds3;
    [SerializeField] private GameObject ds4;
    [SerializeField] private GameObject ds5;
    [SerializeField] private GameObject ds6;
    [SerializeField] private GameObject ds7;
    [SerializeField] private GameObject ds8;
    

    public float speed;
    public float turnSpeed;
    private List<GameObject> spheres;
    private float fixedHeight = 2f;
    private Vector3[] idealPoints;

    [SerializeField] private float jointReach;

    private Quaternion previousRotation;

    void Start()
    {
        idealPoints = new Vector3[5];
        spheres = new List<GameObject>();
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

        if (rRight)
        {
            // lol rotations are non-commutative
            transform.rotation = quaternion.Euler(-transform.up * (Time.deltaTime * turnSpeed)) * transform.rotation;
        } else if (rLeft)
        {
            transform.rotation = quaternion.Euler(transform.up * (Time.deltaTime * turnSpeed)) * transform.rotation;
        }
        
        for (int i = 0; i < spheres.Count; i++)
        {
            GameObject g = spheres[i];
            spheres[i] = null;
            Destroy(g);
        }
        spheres.Clear();
        
        UpdateRayDir();

        Vector3 p = GetClosestPoint(transform.position);
        DebugSphere(p, 1);
        Vector3 upNormal = (transform.position - p).normalized;
        
        // Quaternion _r = Quaternion.LookRotation(Vector3.Cross(transform.right,upNormal), upNormal);
        Quaternion rotation = Quaternion.FromToRotation(transform.up, upNormal) * transform.rotation;
        transform.rotation = Quaternion.Lerp(previousRotation, rotation, 0.05f);

        transform.position = p + (transform.position - p).normalized * fixedHeight;
        transform.position += all * (speed * Time.deltaTime);
        
        HandleLandingPoints();
        previousRotation = transform.rotation;
    }

    private void HandleLandingPoints()
    {
        Vector3 fl = GetClosestPoint(frontLeftJoint.position, jointReach);
        Vector3 fr = GetClosestPoint(frontRightJoint.position, jointReach);
        Vector3 bl = GetClosestPoint(backLeftJoint.position, jointReach);
        Vector3 br = GetClosestPoint(backRightJoint.position, jointReach);
        
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
    
    void DebugSphere(Vector3 pos, float scale)
    {
        GameObject s = Instantiate(debugSphere, pos, quaternion.Euler(0,0,0));
        s.transform.localScale = Vector3.one * scale;
        spheres.Add(s);
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

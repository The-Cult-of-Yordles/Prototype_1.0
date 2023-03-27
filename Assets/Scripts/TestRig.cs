using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TestRig : MonoBehaviour
{
    [SerializeField] InputAction forward;
    [SerializeField] InputAction backward;
    [SerializeField] InputAction left;
    [SerializeField] InputAction right;
    [SerializeField] InputAction turnLeft;
    [SerializeField] InputAction turnRight;

    [SerializeField] GameObject debugSphere;

    public float speed;
    public float turnSpeed;
    private List<GameObject> spheres;
    private float fixedHeight = 2f;
    private Vector3[] idealPoints;

    void Start()
    {
        idealPoints = new Vector3[5];
        spheres = new List<GameObject>();
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
            transform.rotation *= quaternion.Euler(-transform.up * (Time.deltaTime * turnSpeed));
        } else if (rLeft)
        {
            transform.rotation *= quaternion.Euler(transform.up * (Time.deltaTime * turnSpeed));
        }
        
        for (int i = 0; i < spheres.Count; i++)
        {
            GameObject g = spheres[i];
            spheres[i] = null;
            Destroy(g);
        }
        spheres.Clear();
        
        UpdateRayDir();
        HandleRaycast();

        Collider[] c = Physics.OverlapSphere(transform.position, 20);
        Vector3 p = c[0].ClosestPoint(transform.position);
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
        DebugSphere(p, 1);
        Vector3 upNormal = (transform.position - p).normalized;
        
        Quaternion _r = Quaternion.LookRotation(Vector3.Cross(transform.right,upNormal), upNormal);
        transform.rotation = _r;

        transform.position = p + (transform.position - p).normalized * fixedHeight;
        transform.position += all * (speed * Time.deltaTime);
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
    
    void HandleRaycast()
    {
        DebugSphere(idealPoints[0], 0.3f);
        DebugSphere(idealPoints[1], 0.3f);
        DebugSphere(idealPoints[2], 0.3f);
        DebugSphere(idealPoints[3], 0.3f);
        DebugSphere(idealPoints[4], 0.3f);
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

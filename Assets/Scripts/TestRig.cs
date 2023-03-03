using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestRig : MonoBehaviour
{
    [SerializeField] InputAction forward;
    [SerializeField] InputAction backward;
    [SerializeField] InputAction left;
    [SerializeField] InputAction right;
    [SerializeField] InputAction turnLeft;
    [SerializeField] InputAction turnRight;

    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject debugSphere;

    public float speed;
    public float turnSpeed;
    private List<GameObject> spheres;

    private float fixedHeight;

    private Vector3 d0;
    private Vector3 d1;
    private Vector3 d2;
    private Vector3 d3;
    private Vector3 d4;

    private Vector3 upNormal;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        rb.velocity = all * (speed * Time.deltaTime);
        
        bool rLeft = turnRight.ReadValue<float>() > 0.01f;
        bool rRight = turnLeft.ReadValue<float>() > 0.01f;

        if (rRight)
        {
            transform.Rotate(new Vector3(0, -1 * (Time.deltaTime * turnSpeed), 0));
        }
        if (rLeft)
        {
            transform.Rotate(new Vector3(0, 1 * (Time.deltaTime * turnSpeed), 0));
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

        transform.rotation = Quaternion.LookRotation(transform.forward, upNormal);
    }

    private void UpdateRayDir()
    {
        var position = transform.position;

        d0 = position - transform.up;
        d1 = position - transform.right + transform.forward - transform.up;
        d2 = position + transform.right + transform.forward - transform.up;
        d3 = position - transform.right - transform.forward - transform.up;
        d4 = position + transform.right - transform.forward - transform.up;
    }
    
    void HandleRaycast()
    {
        Vector3 p = transform.position;
        Ray r0 = new Ray(p, (d0 - p) * 2);
        Ray r1 = new Ray(p, (d1 - p) * 2);
        Ray r2 = new Ray(p, (d2 - p) * 2);
        Ray r3 = new Ray(p, (d3 - p) * 2);
        Ray r4 = new Ray(p, (d4 - p) * 2);

        RaycastHit h0;
        RaycastHit h1;
        RaycastHit h2;
        RaycastHit h3;
        RaycastHit h4;

        Physics.Raycast(r0, out h0);
        Physics.Raycast(r1, out h1);
        Physics.Raycast(r2, out h2);
        Physics.Raycast(r3, out h3);
        Physics.Raycast(r4, out h4);

        Vector3 p0 = h0.point;
        Vector3 p1 = h1.point;
        Vector3 p2 = h2.point;
        Vector3 p3 = h3.point;
        Vector3 p4 = h4.point;

        Vector3 n0 = Vector3.Cross(p0 - p2, p1 - p0).normalized;
        Vector3 n1 = Vector3.Cross(p0 - p1, p3 - p0).normalized;
        Vector3 n2 = Vector3.Cross(p0 - p4, p2 - p0).normalized;
        Vector3 n3 = Vector3.Cross(p0 - p3, p4 - p0).normalized;

        upNormal = (n0 + n1 + n2 + n3).normalized;
        
        Debug.DrawRay(transform.position, n0, Color.red);
        Debug.DrawRay(transform.position, n1, Color.red);
        Debug.DrawRay(transform.position, n2, Color.red);
        Debug.DrawRay(transform.position, n3, Color.red);
        Debug.DrawRay(transform.position, upNormal * 10, Color.blue);
        
        DebugSphere(d0, 0.3f);
        DebugSphere(d1, 0.3f);
        DebugSphere(d2, 0.3f);
        DebugSphere(d3, 0.3f);
        DebugSphere(d4, 0.3f);
        
        DebugSphere(p0, 0.3f);
        DebugSphere(p1, 0.3f);
        DebugSphere(p2, 0.3f);
        DebugSphere(p3, 0.3f);
        DebugSphere(p4, 0.3f);

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

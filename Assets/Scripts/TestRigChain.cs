using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TestRigChain : MonoBehaviour
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

    public Vector3 frPrevious;
    public Vector3 flPrevious;
    public Vector3 brPrevious;
    public Vector3 blPrevious;

    public float speed;
    public float turnSpeed;
    private float fixedHeight = 2f;
    private Vector3[] idealPoints;

    [SerializeField] private float jointReach;

    [SerializeField] private int segmentCount;
    [SerializeField] private GameObject segment;
    [SerializeField] private GameObject joint;
    private List<GameObject> segments;
    private List<GameObject> joints;

    private float gap = 1f;

    private int res = 10;
    void Start()
    {
        idealPoints = new Vector3[5];
        InitializeSegments();
    }

    void InitializeSegments()
    {
        segments = new List<GameObject>();
        joints = new List<GameObject>();
        Vector3 curr = transform.position;
        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(Instantiate(segment));
            joints.Add(Instantiate(joint));
            joints[i].transform.position = curr - Vector3.forward * gap;
            segments[i].transform.position = joints[i].transform.position - segments[i].transform.forward * gap;
            curr = segments[i].transform.position;
        }
    }

    void UpdateSegments()
    {
        Vector3 curr = transform.position;
        Vector3 forwardCurr = transform.forward;
        for (int i = 0; i < segmentCount; i++)
        {
            Transform currTransform = segments[i].transform;
            Vector3 p = GetClosestPointRayCast(currTransform.position);
            
            // fixed height
            Vector3 idealHeight = p + (currTransform.position - p).normalized * fixedHeight;
            
            // update normal and forward
            Vector3 upNormal = (currTransform.position - p).normalized;
            Vector3 forward = (joints[i].transform.position - currTransform.position).normalized;
            currTransform.rotation = Quaternion.LookRotation(forward, upNormal);
            
            // update position
            joints[i].transform.position = curr - forwardCurr * gap;

            if ((curr - currTransform.position).magnitude > gap * 2.5f)
            {
                /// works but need improvement
                Vector3 offset = (currTransform.forward + (idealHeight - currTransform.position).normalized).normalized
                    * (30f * Time.deltaTime);
                currTransform.position += offset;
            }
               
            
            // update
            curr = currTransform.position;
            forwardCurr = currTransform.forward;
        }
        
    }

    void Update()
    {
        UpdateSegments();
        
        int f = forward.ReadValue<float>() > 0.01f? 1 : 0;
        int b = backward.ReadValue<float>() > 0.01f? 1 : 0;
        int r = right.ReadValue<float>() > 0.01f? 1 : 0;
        int l = left.ReadValue<float>() > 0.01f? 1 : 0;

        Vector3 _forward = transform.forward * f;
        Vector3 _backward = -transform.forward * b;
        Vector3 _right = transform.right * r;
        Vector3 _left = -transform.right * l;

        Vector3 all = (_forward + _backward + _right + _left).normalized;
        
        bool rLeft = turnRight.ReadValue<float>() > 0.01f;
        bool rRight = turnLeft.ReadValue<float>() > 0.01f;

        
        UpdateRayDir();

        Vector3 p = GetClosestPointRayCast(transform.position);
        ds0.transform.position = p;
        Vector3 upNormal = (transform.position - p).normalized;
        Vector3 averageNormal = upNormal;


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
        
    }

    private void HandleLandingPoints()
    {
        Vector3 fl = GetLandingPoint(frontLeftJoint.position, flPrevious, jointReach);
        Vector3 fr = GetLandingPoint(frontRightJoint.position, frPrevious, jointReach);
        Vector3 bl = GetLandingPoint(backLeftJoint.position, blPrevious, jointReach);
        Vector3 br = GetLandingPoint(backRightJoint.position, brPrevious, jointReach);
        
        flPrevious = fl;
        frPrevious = fr;
        blPrevious = bl;
        brPrevious = br;

        _fr = fr;
        _fl = fl;
        _br = br;
        _bl = bl;
    }


    private Vector3 GetLandingPoint(Vector3 jointPos, Vector3 previous, float maxRadius)
    {
        RaycastHit hit;
        
        // first raycast from body to joint to see if there is collision
        Vector3 f = jointPos - transform.position;
        if (Physics.Raycast(transform.position, f, out hit, f.magnitude))
        {
            return hit.point;
        }
        
        // then spherecast from joint position
        float initial = 0.1f;
        while (initial < maxRadius)
        {
            if (Physics.SphereCast(jointPos, initial, -transform.up, out hit, initial))
            {
                return hit.point;
            }
            
            initial += 0.1f;
        }
        return previous;
    }
    private Vector3 GetClosestPointRayCast(Vector3 jointPos, float maxRadius = 3)
    {
        float step = 2f / res;
        RaycastHit hit;
        float closest = 100f;
        Vector3 point = Vector3.zero;
        
        // xz
        for (float i = -1f; i < 1f; i+= step)
        {
            for (float j = -1f; j < 1f; j+= step)
            {
                Vector3 a = Vector3.up + new Vector3(i, 0, j);
                Vector3 b = Vector3.down + new Vector3(i, 0, j);
                
                if (Physics.Raycast(jointPos, a, out hit, 3f))
                {
                    Debug.DrawRay(jointPos, a.normalized * maxRadius, Color.red);
                    if ((hit.point - jointPos).magnitude < closest)
                    {
                        point = hit.point;
                        closest = (hit.point - jointPos).magnitude;
                    }
                }
                if (Physics.Raycast(jointPos, b, out hit, 3f))
                {
                    Debug.DrawRay(jointPos, b.normalized * maxRadius, Color.red);
                    if ((hit.point - jointPos).magnitude < closest)
                    {
                        point = hit.point;
                        closest = (hit.point - jointPos).magnitude;
                    }
                }
                
            }
        }
        
        // xy
        for (float i = -1f; i < 1f; i+= step)
        {
            for (float j = -1f; j < 1f; j+= step)
            {
                Vector3 a = Vector3.forward + new Vector3(i, j, 0);
                Vector3 b = Vector3.back + new Vector3(i, j, 0);
                
                if (Physics.Raycast(jointPos, a, out hit, 3f))
                {
                    Debug.DrawRay(jointPos, a.normalized * maxRadius, Color.blue);
                    if ((hit.point - jointPos).magnitude < closest)
                    {
                        point = hit.point;
                        closest = (hit.point - jointPos).magnitude;
                    }
                }
                if (Physics.Raycast(jointPos, b, out hit, 3f))
                {
                    Debug.DrawRay(jointPos, b.normalized * maxRadius, Color.blue);
                    if ((hit.point - jointPos).magnitude < closest)
                    {
                        point = hit.point;
                        closest = (hit.point - jointPos).magnitude;
                    }
                }
                
            }
        }
        
        // yz
        for (float i = -1f; i < 1f; i+= step)
        {
            for (float j = -1f; j < 1f; j+= step)
            {
                Vector3 a = Vector3.left + new Vector3(0, i, j);
                Vector3 b = Vector3.right + new Vector3(0, i, j);
                
                if (Physics.Raycast(jointPos, a, out hit, 3f))
                {
                    Debug.DrawRay(jointPos, a.normalized * maxRadius, Color.yellow);
                    if ((hit.point - jointPos).magnitude < closest)
                    {
                        point = hit.point;
                        closest = (hit.point - jointPos).magnitude;
                    }
                }
                if (Physics.Raycast(jointPos, b, out hit, 3f))
                {
                    Debug.DrawRay(jointPos, b.normalized * maxRadius, Color.yellow);
                    if ((hit.point - jointPos).magnitude < closest)
                    {
                        point = hit.point;
                        closest = (hit.point - jointPos).magnitude;
                    }
                }
                
            }
        }

        ds1.transform.position = jointPos;
        return point;
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

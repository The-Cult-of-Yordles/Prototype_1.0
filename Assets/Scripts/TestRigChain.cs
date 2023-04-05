using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TestRigChain : MonoBehaviour
{
    [SerializeField] InputAction forward;
    [SerializeField] InputAction backward;
    [SerializeField] InputAction left;
    [SerializeField] InputAction right;
    [SerializeField] InputAction turnLeft;
    [SerializeField] InputAction turnRight;
    [SerializeField] InputAction sprint;

    public float speed;
    public float turnSpeed;
    private float fixedHeight = 1f;

    [SerializeField] private float jointReach;

    [SerializeField] private int segmentCount;
    [SerializeField] private GameObject segment;
    [SerializeField] private GameObject joint;
    private List<GameObject> segments;
    private List<GameObject> joints;
    private List<GameObject> targetsLeft;
    private List<GameObject> targetsRight;

    private float _speed;
    private float gap = 1f;
    private int res = 8;
    private float frequency = 0.1f;
    
    void Start()
    {
        InitializeSegments();
        StartCoroutine(UpdateSegmentLegs());
    }


    void Update()
    {
        HandleInputsAndHead();
        UpdateSegments();
    }


    void InitializeSegments()
    {
        segments = new List<GameObject>();
        joints = new List<GameObject>();
        targetsRight = new List<GameObject>();
        targetsLeft = new List<GameObject>();
        
        Vector3 curr = transform.position;
        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(Instantiate(segment));
            joints.Add(Instantiate(joint));
            joints[i].transform.position = curr - Vector3.forward * gap;
            segments[i].transform.position = joints[i].transform.position - segments[i].transform.forward * gap;
            curr = segments[i].transform.position;


            MoveSegmentLegs script = segments[i].GetComponent<MoveSegmentLegs>();
            script.leftTarget.transform.parent = null;
            script.rightTarget.transform.parent = null;
            targetsLeft.Add(script.leftTarget);
            targetsRight.Add(script.rightTarget);
        }
    }
    

    IEnumerator UpdateSegmentLegs()
    {
        int inc0 = 0;
        int inc1 = 4;
        int inc2 = 8;
        while (true)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (i == inc0 || i == inc1 || i == inc2)
                {
                    MoveSegmentLegs(
                        targetsLeft[i].transform, targetsRight[i].transform, segments[i].transform);
                }
                
            }
            
            inc0 ++;
            inc1 ++;
            inc2 ++;
            
            if (inc0 >= segmentCount) inc0 = 0;
            if (inc1 >= segmentCount) inc1 = 0;
            if (inc2 >= segmentCount) inc2 = 0;
            
            yield return new WaitForSeconds(frequency);
        }
    }
    
    void MoveSegmentLegs(Transform left, Transform right, Transform body)
    {
        
        Vector3 ls = left.position;
        Vector3 le = body.position - body.right * 2f - body.up * 1f + body.forward * 2.5f;
        
        Vector3 rs = right.position;
        Vector3 re = body.position + body.right * 2f - body.up * 1f + body.forward * 2.5f;

        // test if the leg move this round
        bool l = (le - ls).magnitude > 3.5f;
        bool r = (re - rs).magnitude > 3.5f;
        
        if (l)
        {
            left.position = le;
        }

        if (r)
        {
            right.position = re;
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
            
            // update normal and forward
            Vector3 upNormal = (currTransform.position - p).normalized;
            Vector3 forward = (joints[i].transform.position - currTransform.position).normalized;
            currTransform.rotation = Quaternion.LookRotation(forward, upNormal);
            
            // ideal height
            Vector3 idealHeight = p + (currTransform.position - p).normalized * fixedHeight;
            
            // update position
            joints[i].transform.position = curr - forwardCurr * gap;
            Vector3 offset = Vector3.zero;
            
            if ((joints[i].transform.position - currTransform.position).magnitude > gap * 1.5f)
            {
                offset += (joints[i].transform.position - currTransform.position) * (20f * Time.deltaTime);
            }

            if ((currTransform.position - idealHeight).magnitude > 0.5f)
            {
                offset += (idealHeight - currTransform.position).normalized * 0.2f;

            }
            
            currTransform.position += offset;
            Vector3 t = currTransform.position + offset;
            currTransform.position = Vector3.Lerp(currTransform.position, t, 0.3f);
            
            
            // update
            curr = currTransform.position;
            forwardCurr = currTransform.forward;
        }
        
    }
    
    private void HandleInputsAndHead()
    {
        int f = forward.ReadValue<float>() > 0.01f ? 1 : 0;
        int b = backward.ReadValue<float>() > 0.01f ? 1 : 0;
        int r = right.ReadValue<float>() > 0.01f ? 1 : 0;
        int l = left.ReadValue<float>() > 0.01f ? 1 : 0;
        int a = sprint.ReadValue<float>() > 0.01f ? 1 : 0;

        if (a > 0f)
        {
            _speed = speed * 2;
            frequency = 0.05f;
        }
        else
        {
            _speed = speed;
            frequency = 0.1f;
        }

        Vector3 _forward = transform.forward * f;
        Vector3 _backward = -transform.forward * b;
        Vector3 _right = transform.right * r;
        Vector3 _left = -transform.right * l;

        Vector3 all = (_forward + _backward + _right + _left).normalized;

        bool rLeft = turnRight.ReadValue<float>() > 0.01f;
        bool rRight = turnLeft.ReadValue<float>() > 0.01f;

        Vector3 p = GetClosestPointRayCast(transform.position);
        Vector3 upNormal = (transform.position - p).normalized;

        // this handles rotation based on input
        if (rRight)
        {
            transform.rotation = quaternion.Euler(-transform.up * (Time.deltaTime * turnSpeed)) * transform.rotation;
        }
        else if (rLeft)
        {
            transform.rotation = quaternion.Euler(transform.up * (Time.deltaTime * turnSpeed)) * transform.rotation;
        }

        // this handles rotation based on surface
        Quaternion rotation = Quaternion.FromToRotation(transform.up, upNormal) * transform.rotation;
        transform.rotation = rotation;

        // this handles fixed height
        transform.position = p + (transform.position - p).normalized * fixedHeight;
        // this handles movement
        transform.position += all * (_speed * Time.deltaTime);
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

        return point;
    }

    private void OnEnable()
    {
        forward.Enable();
        backward.Enable();
        left.Enable();
        right.Enable();
        turnLeft.Enable();
        turnRight.Enable();
        sprint.Enable();
    }

    private void OnDisable()
    {
        forward.Disable();
        backward.Disable();
        left.Disable();
        right.Disable();
        turnLeft.Disable();
        turnRight.Disable();
        sprint.Disable();
    }
}

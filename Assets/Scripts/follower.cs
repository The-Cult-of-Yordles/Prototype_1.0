using UnityEngine;

public class follower : MonoBehaviour
{
    [SerializeField] private GameObject following;
    void Update()
    {
        transform.position = following.transform.position - transform.forward * 2;
        transform.RotateAround(following.transform.position, Vector3.up, 1f);
    }
}

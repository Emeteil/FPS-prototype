using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] public Transform target;

    private void Start()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (target != null)
            transform.position = target.position;
    }
}
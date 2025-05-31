using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundButton : MonoBehaviour
{
    [SerializeField] private Transform button;
    [SerializeField] private float speedPush = 1f;
    [SerializeField] private List<string> tags;
    [SerializeField] private Switch _switch;
    [SerializeField] private Vector3 triggerSize = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 triggerOffset = Vector3.zero;

    [SerializeField]
    private Vector3 startPosition = new Vector3(0, 0, 0);
    [SerializeField]
    private Vector3 endPosition = new Vector3(0, -0.05f, 0);

    private List<Collider> touchingObjects = new List<Collider>();

    void Update()
    {
        CheckTouchingObjects();

        if (touchingObjects.Count > 0)
        {
            button.localPosition = Vector3.MoveTowards(button.localPosition, endPosition, speedPush * Time.deltaTime);
            _switch.toggle = true;
        }
        else
        {
            button.localPosition = Vector3.MoveTowards(button.localPosition, startPosition, speedPush * Time.deltaTime);
            _switch.toggle = false;
        }
    }

    private void CheckTouchingObjects()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + triggerOffset, triggerSize / 2, Quaternion.identity);

        touchingObjects.RemoveAll(collider => collider == null || !colliders.Contains(collider));

        foreach (Collider collider in colliders)
            if (tags.Contains(collider.gameObject.tag) && !touchingObjects.Contains(collider))
                touchingObjects.Add(collider);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + triggerOffset, triggerSize);
    }
}

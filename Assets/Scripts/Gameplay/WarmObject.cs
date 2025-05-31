using UnityEngine;

public class WarmObject : MonoBehaviour
{
    [SerializeField] private float distance = 3.0f;

    private void Start()
    {
        BodyTempController.Instance.AddWarmObj(transform, distance);
    }

    private void OnDestroy() =>
        BodyTempController.Instance.RemoveWarmObj(transform);

    private void OnDisable() =>
        BodyTempController.Instance.RemoveWarmObj(transform);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
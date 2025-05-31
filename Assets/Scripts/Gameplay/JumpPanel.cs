using UnityEngine;

public class JumpPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private string PlayerTag = "Player";

    private void OnCollisionEnter(Collision other)
    {
        if (
            other.gameObject.tag == PlayerTag &&
            other.gameObject.GetComponent<PlayerMovment>().state == PlayerMovment.MovementState.crouching
        ) return;

        other.rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}

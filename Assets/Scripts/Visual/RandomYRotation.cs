using UnityEngine;

public class RandomYRotation : MonoBehaviour
{
    private void Start()
    {
        float randomYRotation = Random.Range(0f, 360f);

        Vector3 currentRotation = transform.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(currentRotation.x, randomYRotation, currentRotation.z);
    }
}
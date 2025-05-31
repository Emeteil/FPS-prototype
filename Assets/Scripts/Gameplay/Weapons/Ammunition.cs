using UnityEngine;

public class Ammunition : MonoBehaviour
{
    [SerializeField] private int ammo = 30;

    public int Ammo => ammo;

    public void PickUp()
    {
        GrabUp.Instance.ReleaseObject();
        Destroy(gameObject);
    }
}
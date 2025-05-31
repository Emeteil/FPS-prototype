using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryItemData item;
    [SerializeField] private int count = 1;

    public void OnInteract()
    {
        if (item == null) return;

        if (InventorySystem.Instance.AddItem(item, count: count))
        {
            GrabUp.Instance.ReleaseObject();
            Destroy(gameObject);
        }
    }
}
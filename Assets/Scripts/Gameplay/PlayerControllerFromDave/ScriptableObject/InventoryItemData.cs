using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemData", menuName = "Inventory/Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    public GameObject playerPrefab;
    public int maxStack = 1;
}
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDatabase", menuName = "Inventory/Database")]
public class InventoryDatabase : ScriptableObject
{
    public InventoryItemData[] itemDataAssets;
}
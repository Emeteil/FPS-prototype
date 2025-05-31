using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    [HideInInspector] public InventoryItemData itemType;
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    [NonSerialized] public GameObject playerObject;
    public int maxStack = 1;
}

[Serializable]
public class InventorySlot
{
    public InventoryItem itemType;
    public int count;

    public bool IsEmpty => itemType == null || count <= 0;
    public bool IsFull => !IsEmpty && count >= itemType.maxStack;
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private InventoryDatabase inventoryDatabase;
    private Dictionary<InventoryItemData, InventoryItem> itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();

    [SerializeField] private int slotCount = 5;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Transform hands;
    [SerializeField] private Camera playerCamera;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private int activeSlotIndex = 0;

    private bool _block = false;
    private bool _ignorePause = false;

    public event Action OnInventoryChanged;
    public event Action<int> OnActiveSlotChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeItemDictionary();
        InitializeSlots();
    }

    private void Start()
    {
        Pause.Instance.AddScript(this);
    }

    private void InitializeItemDictionary()
    {
        itemDictionary.Clear();
        if (inventoryDatabase == null || inventoryDatabase.itemDataAssets == null)
        {
            Debug.LogError("Inventory database is not assigned or empty!");
            return;
        }

        foreach (var data in inventoryDatabase.itemDataAssets)
        {
            if (data == null) continue;

            InventoryItem item = new InventoryItem
            {
                itemType = data,
                itemName = data.itemName,
                icon = data.icon,
                worldPrefab = data.worldPrefab,
                maxStack = data.maxStack
            };

            InitializeItem(hands, ref item.playerObject, data.playerPrefab);

            if (!itemDictionary.ContainsKey(item.itemType))
                itemDictionary.Add(item.itemType, item);
        }
    }

    public void AddItemType(InventoryItemData inventoryItemData)
    {
        inventoryDatabase.itemDataAssets.Append(inventoryItemData);

        InventoryItem item = new InventoryItem
        {
            itemType = inventoryItemData,
            itemName = inventoryItemData.itemName,
            icon = inventoryItemData.icon,
            worldPrefab = inventoryItemData.worldPrefab,
            maxStack = inventoryItemData.maxStack
        };

        InitializeItem(hands, ref item.playerObject, inventoryItemData.playerPrefab);

        if (!itemDictionary.ContainsKey(item.itemType))
            itemDictionary.Add(item.itemType, item);
    }

    private void InitializeItem(Transform handsParent, ref GameObject playerObject, GameObject worldPrefab)
    {
        if (playerObject == null && worldPrefab != null)
        {
            playerObject = Instantiate(worldPrefab, handsParent);
            playerObject.SetActive(false);
        }
    }

    private void InitializeSlots()
    {
        slots.Clear();
        for (int i = 0; i < slotCount; i++)
            slots.Add(new InventorySlot());
    }

    private void Update()
    {
        if (_block) return;

        HandleSlotSelection();
        HandleItemThrow();
    }

    private void HandleSlotSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1;
            ChangeActiveSlot(activeSlotIndex + direction);
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                ChangeActiveSlot(i);
        }
    }

    private void HandleItemThrow()
    {
        if (Input.GetKeyDown(KeyCode.G) && !slots[activeSlotIndex].IsEmpty)
            ThrowItem(activeSlotIndex);
    }

    public void ChangeActiveSlot(int newIndex)
    {
        if (newIndex < 0) newIndex = slotCount - 1;
        if (newIndex >= slotCount) newIndex = 0;

        if (!slots[activeSlotIndex].IsEmpty && slots[activeSlotIndex].itemType.playerObject != null)
            slots[activeSlotIndex].itemType.playerObject.SetActive(false);

        activeSlotIndex = newIndex;

        if (!slots[activeSlotIndex].IsEmpty && slots[activeSlotIndex].itemType.playerObject != null)
            slots[activeSlotIndex].itemType.playerObject.SetActive(true);

        OnActiveSlotChanged?.Invoke(activeSlotIndex);
    }

    public bool AddItem(InventoryItemData itemType, int count = 1)
    {
        if (!itemDictionary.TryGetValue(itemType, out InventoryItem item))
        {
            Debug.LogWarning($"Item with ID {itemType} not found in dictionary");
            return false;
        }

        if (item.maxStack > 1)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.itemType.itemType == itemType && !slot.IsFull)
                {
                    int canAdd = Mathf.Min(item.maxStack - slot.count, count);
                    slot.count += canAdd;
                    count -= canAdd;

                    if (count <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.itemType = item;
                slot.count = Mathf.Min(count, item.maxStack);
                count -= slot.count;

                if (slot == slots[activeSlotIndex] && item.playerObject != null)
                    item.playerObject.SetActive(true);

                if (count <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return false;
    }

    public void RemoveItem(int slotIndex, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        if (slots[slotIndex].IsEmpty) return;

        slots[slotIndex].count -= count;
        if (slots[slotIndex].count <= 0)
        {
            if (slotIndex == activeSlotIndex && slots[slotIndex].itemType.playerObject != null)
                slots[slotIndex].itemType.playerObject.SetActive(false);

            slots[slotIndex].itemType = null;
            slots[slotIndex].count = 0;
        }

        OnInventoryChanged?.Invoke();
    }

    public void ThrowItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        if (slots[slotIndex].IsEmpty) return;

        var item = slots[slotIndex].itemType;

        if (item.worldPrefab != null && throwOrigin != null && playerCamera != null)
        {
            GameObject thrownItem = Instantiate(item.worldPrefab,
                throwOrigin.position,
                Quaternion.identity);

            Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        }

        RemoveItem(slotIndex);
    }

    public void Block(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = true;
        _block = true;

        if (!pause && !slots[activeSlotIndex].IsEmpty && slots[activeSlotIndex].itemType.playerObject != null)
            slots[activeSlotIndex].itemType.playerObject.SetActive(false);
    }

    public void Unblock(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = false;
        _block = false;

        if (!pause && !slots[activeSlotIndex].IsEmpty && slots[activeSlotIndex].itemType.playerObject != null)
            slots[activeSlotIndex].itemType.playerObject.SetActive(true);
    }

    public InventoryItem GetActiveItem()
    {
        return slots[activeSlotIndex].IsEmpty ? null : slots[activeSlotIndex].itemType;
    }

    public int GetActiveSlotIndex() => activeSlotIndex;

    public List<InventorySlot> GetAllSlots() => new List<InventorySlot>(slots);

    public InventoryItem GetItemById(InventoryItemData itemType)
    {
        itemDictionary.TryGetValue(itemType, out InventoryItem item);
        return item;
    }

    public int GetItemCount(InventoryItemData itemType)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.itemType.itemType == itemType)
                total += slot.count;
        }
        return total;
    }
}
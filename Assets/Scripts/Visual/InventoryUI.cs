using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color normalSlotColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] private Color activeSlotColor = Color.white;
    [SerializeField] private int slotBorderWidth = 4;
    [SerializeField] private float iconScale = 0.85f;

    [Header("References")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite emptySlotSprite;

    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private int lastActiveSlot = 0;

    private void Start()
    {
        InitializeUI();
        InventorySystem.Instance.OnInventoryChanged += UpdateAllSlots;
        InventorySystem.Instance.OnActiveSlotChanged += UpdateActiveSlot;
    }

    private void InitializeUI()
    {
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);
        
        slotUIs.Clear();

        int slotCount = InventorySystem.Instance.GetAllSlots().Count;
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            
            Image slotImage = slotObj.GetComponent<Image>();
            slotImage.color = normalSlotColor;

            slotUIs.Add(slotUI);
        }

        UpdateAllSlots();
        UpdateActiveSlot(InventorySystem.Instance.GetActiveSlotIndex());
    }

    private void UpdateAllSlots()
    {
        List<InventorySlot> slots = InventorySystem.Instance.GetAllSlots();
        
        for (int i = 0; i < slots.Count; i++)
            UpdateSlot(i, slots[i]);
    }

    private void UpdateSlot(int slotIndex, InventorySlot slot)
    {
        if (slotIndex < 0 || slotIndex >= slotUIs.Count) return;

        InventorySlotUI slotUI = slotUIs[slotIndex];
        
        if (slot.IsEmpty)
        {
            slotUI.SetIcon(emptySlotSprite);
            slotUI.SetCount(0);
        }
        else
        {
            slotUI.SetIcon(slot.itemType.icon);
            slotUI.SetCount(slot.count);
            
            slotUI.SetIconScale(iconScale);
        }
    }

    private void UpdateActiveSlot(int activeSlotIndex)
    {
        if (lastActiveSlot >= 0 && lastActiveSlot < slotUIs.Count)
            slotUIs[lastActiveSlot].GetComponent<Image>().color = normalSlotColor;

        if (activeSlotIndex >= 0 && activeSlotIndex < slotUIs.Count)
        {
            slotUIs[activeSlotIndex].GetComponent<Image>().color = activeSlotColor;
            lastActiveSlot = activeSlotIndex;
        }
    }
}
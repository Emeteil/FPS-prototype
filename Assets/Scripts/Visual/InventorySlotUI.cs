using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject activeSlotIndicator;
    [SerializeField] private Text countText;

    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    public void SetCount(int count)
    {
        if (countText != null)
            countText.text = count > 1 ? count.ToString() : "";
    }

    public void SetIconScale(float scale)
    {
        if (iconImage != null)
            iconImage.transform.localScale = Vector3.one * scale;
    }

    public void SetActive(bool active)
    {
        activeSlotIndicator.SetActive(active);
    }
}
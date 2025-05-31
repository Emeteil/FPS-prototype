using UnityEngine;

public class IInteractableTest : MonoBehaviour, IInteractable
{
    public void OnHoverEnter() => Debug.Log("Наведено на объект");
    public void OnHoverExit() => Debug.Log("Убрано наведение с объекта");
    public void OnInteract() => Debug.Log("Нажали F (однократное нажатие)");

    public void OnInteractClamped() => Debug.Log("Зажали F (клавиша зажата)");
    public void OnInteractClampedStop() => Debug.Log("Отпустили F (клавиша отпущена)");

    private void Start()
    {
        FindObjectOfType<PlayerInteraction>().cheakAnyKey = true;
    }
    
    public void OnInteractKey(KeyCode customKey) => Debug.Log($"Нажали {customKey}");
    public void OnInteractKeyClamped(KeyCode customKey) => Debug.Log($"Зажимают {customKey}");
    public void OnInteractKeyClampedStop(KeyCode customKey) => Debug.Log($"Отпустили {customKey}");
}
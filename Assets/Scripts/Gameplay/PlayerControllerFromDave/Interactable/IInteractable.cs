using UnityEngine;

public interface IInteractable
{
    void OnHover() {}
    void OnHoverEnter() {}
    void OnHoverExit() {}
    void OnInteract() {}
    void OnInteractClamped() {}
    void OnInteractClampedStop() {}

    void OnInteractKey(KeyCode customKey) {}
    void OnInteractKeyClamped(KeyCode customKey) {}
    void OnInteractKeyClampedStop(KeyCode customKey) {}
}
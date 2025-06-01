using UnityEngine;
using UnityEngine.Events;

public class CutsceneManager : MonoBehaviour, IControllable
{
    [SerializeField] private Camera _camera;

    [SerializeField] private UnityEvent onControllerEnabled;
    [SerializeField] private UnityEvent onControllerDisabled;
    [SerializeField] private UnityEvent onControllerEnabledAfter;
    
    public void OnControllerEnabled()
    {
        onControllerEnabled?.Invoke();
    }

    public void OnControllerDisabled()
    {
        onControllerDisabled?.Invoke();
    }

    public void OnControllerEnabledAfter()
    {
        onControllerEnabledAfter?.Invoke();
    }

    public Transform GetCameraTransform()
    {
        return _camera.transform;
    }

    public Camera GetCamera()
    {
        return _camera;
    }

    public CursorState GetPreferredCursorState()
    {
        return CursorState.ForceLocked;
    }
}

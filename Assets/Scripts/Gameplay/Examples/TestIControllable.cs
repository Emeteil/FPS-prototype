using UnityEngine;

public class TestIControllable : MonoBehaviour, IControllable, IInteractable
{
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private Camera blackboardCamera;
    private InteractableText interactableText;

    [HideInInspector] public bool _block = false;
    private bool _ignorePause = false;

    public void Block(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = true;
        _block = true;
    }

    public void Unblock(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = false;
        _block = false;
    }

    private void Start()
    {
        interactableText = GetComponent<InteractableText>();
    }

    private void Update()
    {
        if (!ControllerManager.Instance.IsCurrentController(this)) return;

        if (Input.GetKeyDown(KeyCode.F) && !ControllerManager.Instance.IsTransitioning)
            ControllerManager.Instance.SwitchToDefault(mode: ControllerSwitchMode.Deferred); 
            // Включить стандартный контролер, потом проиграть анимацию
    }

    public void OnInteract()
    {
        if (ControllerManager.Instance.IsCurrentController(this)) return;

        ControllerManager.Instance.SwitchController(this, mode: ControllerSwitchMode.Deferred);
        // Включить новый контролер, потом проиграть анимацию
    }

    public void OnControllerEnabled()
    {
        interactableText.OnHoverExit();
        ControllerManager.Instance.SetGlobalCursorState(CursorState.ForceUnlocked);
    }

    public void OnControllerDisabled()
    {
        interactableText.OnHoverExit();
    }

    public Transform GetCameraTransform()
    {
        return cameraPoint;
    }

    public Camera GetCamera()
    {
        return blackboardCamera;
    }

    public CursorState GetPreferredCursorState()
    {
        return CursorState.ForceUnlocked;
    }
}
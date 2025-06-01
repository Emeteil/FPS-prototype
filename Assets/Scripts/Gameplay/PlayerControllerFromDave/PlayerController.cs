using UnityEngine;

public class PlayerController : MonoBehaviour, IControllable
{
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private MeshRenderer meshRenderer;
    
    private Camera _camera;
    private PlayerMovment playerMovment;
    private PlayerInteraction playerInteraction;
    private InventorySystem inventorySystem;
    private GrabUp grabUp;

    private void Awake()
    {
        playerMovment = GetComponent<PlayerMovment>();
        playerInteraction = GetComponent<PlayerInteraction>();
        inventorySystem = GetComponent<InventorySystem>();
        grabUp = GetComponent<GrabUp>();

        _camera = GetComponentInChildren<Camera>();
    }

    public void OnControllerEnabled()
    {
        playerMovment.Unblock();
        playerCamera.Unblock();
        grabUp.Unblock();
        
        ControllerManager.Instance.SetGlobalCursorState(CursorState.ForceLocked);
    }

    public void OnControllerEnabledAfter()
    {
        playerInteraction.Unblock();
        inventorySystem.Unblock();

        if (meshRenderer != null)
            meshRenderer.enabled = true;
    }

    public void OnControllerDisabled()
    {
        grabUp.ReleaseObject();
        grabUp.Block();
        inventorySystem.Block();
        playerMovment.Block();
        playerCamera.Block();

        playerInteraction.Block();

        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    public Transform GetCameraTransform()
    {
        return playerCamera.transform;
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
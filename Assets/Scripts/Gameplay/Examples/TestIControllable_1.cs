using UnityEngine;

public class TestIControllable_1 : MonoBehaviour, IControllable, IInteractable
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private Transform doorPoint;
    [SerializeField] private Camera blackboardCamera;
    private InteractableText interactableText;

    private GameObject _playerObj;
    private bool _blockMove = true;

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

    public void OnInteract()
    {
        if (ControllerManager.Instance.IsCurrentController(this)) return;

        interactableText.OnHoverExit();
        // Выключить старый контролер, проиграть анимацию, потом включить новый
        ControllerManager.Instance.SwitchController(this);
    }

    private void Start()
    {
        interactableText = GetComponent<InteractableText>();
        _playerObj = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (_block) return;
        if (!ControllerManager.Instance.IsCurrentController(this)) return;

        if (Input.GetKeyDown(KeyCode.F))
            ControllerManager.Instance.SwitchToDefault(mode: ControllerSwitchMode.Deferred);

        if (_blockMove) return;

        _playerObj.transform.position = doorPoint.position;

        float moveInput = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        float rotationInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * rotationInput * rotationSpeed * Time.deltaTime);
    }

    public void OnControllerEnabled()
    {
        _blockMove = false;
        _playerObj.transform.position = doorPoint.position;
        interactableText.OnHoverExit();
        ControllerManager.Instance.SetGlobalCursorState(CursorState.ForceLocked);
    }

    public void OnControllerDisabled()
    {
        _blockMove = true;
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
        return CursorState.ForceLocked;
    }
}
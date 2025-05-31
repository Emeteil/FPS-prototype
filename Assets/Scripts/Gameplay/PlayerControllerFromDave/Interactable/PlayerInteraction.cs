using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] public bool cheakAnyKey = false;

    private Transform cameraTransform;

    private IInteractable[] currentInteractables;
    private GameObject lastInteractedObject;

    private bool _block = false;
    private bool _ignorePause = false;
    
    public static PlayerInteraction Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

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

    public float InteractionDistance => interactionDistance;
    public KeyCode InteractionKey => interactionKey;

    private void Start()
    {
        Pause.Instance.AddScript(this);
        if (mainCamera == null)
            mainCamera = Camera.main;

        cameraTransform = mainCamera.transform;
    }

    private void Update()
    {
        if (_block) return;

        RaycastHit hit;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out hit, interactionDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            ResetInteractables();
            return;
        }

        if (lastInteractedObject != hit.collider.gameObject)
        {
            ResetInteractables();
            lastInteractedObject = hit.collider.gameObject;
            currentInteractables = lastInteractedObject.GetComponents<IInteractable>();

            foreach (var interactable in currentInteractables)
                interactable?.OnHoverEnter();
        }

        foreach (var interactable in currentInteractables)
            interactable?.OnHover();

        if (Input.GetKeyDown(interactionKey))
        {
            foreach (var interactable in currentInteractables)
                interactable?.OnInteract();
        }

        if (Input.GetKey(interactionKey))
        {
            foreach (var interactable in currentInteractables)
                interactable?.OnInteractClamped();
        }

        if (Input.GetKeyUp(interactionKey))
        {
            foreach (var interactable in currentInteractables)
                interactable?.OnInteractClampedStop();
        }

        if (cheakAnyKey)
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    foreach (var interactable in currentInteractables)
                        interactable?.OnInteractKey(keyCode);
                }

                if (Input.GetKey(keyCode))
                {
                    foreach (var interactable in currentInteractables)
                        interactable?.OnInteractKeyClamped(keyCode);
                }

                if (Input.GetKeyUp(keyCode))
                {
                    foreach (var interactable in currentInteractables)
                        interactable?.OnInteractKeyClampedStop(keyCode);
                }
            }
    }

    private void ResetInteractables()
    {
        if (currentInteractables == null) return;

        foreach (var interactable in currentInteractables)
            interactable?.OnHoverExit();

        currentInteractables = null;
        lastInteractedObject = null;
    }
}
using UnityEngine;

public class GrabUp : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform pullTarget;
    [SerializeField] private float pullSpeed = 10f;
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private string[] grabTags = { "Grab" };

    [HideInInspector]
    public bool _block = false;
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

    public static GrabUp Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private Rigidbody grabbedObject;

    private void Start()
    {
        Pause.Instance.AddScript(this);
    }

    private void Update()
    {
        HandleInput();
        if (grabbedObject)
            MoveObject();
    }

    private void HandleInput()
    {
        if (_block) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedObject)
                ReleaseObject();
            else
                TryGrabObject();
        }
    }

    private void TryGrabObject()
    {
        RaycastHit hit;
        if (
            Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, raycastDistance,
                ~0, QueryTriggerInteraction.Ignore) &&
            System.Array.Exists(grabTags, tag => hit.collider.CompareTag(tag))
        )
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb)
                grabbedObject = rb;
        }
    }

    public void ReleaseObject()
    {
        if (!grabbedObject) return;

        grabbedObject = null;
    }

    private void MoveObject()
    {
        Vector3 direction = pullTarget.position - grabbedObject.position;
        grabbedObject.velocity = direction * pullSpeed;
    }
}

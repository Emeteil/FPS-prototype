using UnityEngine;

public class CameraFOVManager : MonoBehaviour
{
    public static CameraFOVManager Instance { get; private set; }
    
    [Header("Настройки камеры")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float defaultFOV = 60f;
    
    private float targetFOV;
    private float fovChangeSpeed = 15f;
    private FOVRequest currentRequest;
    
    private class FOVRequest
    {
        public float fov;
        public float speed;
        public int priority;
        public object owner;
        
        public FOVRequest(float fov, float speed, int priority, object owner)
        {
            this.fov = fov;
            this.speed = speed;
            this.priority = priority;
            this.owner = owner;
        }
    }
    
    public const int PRIORITY_LOW = 1;       // Например, для бега
    public const int PRIORITY_NORMAL = 2;    // Обычные действия
    public const int PRIORITY_HIGH = 3;      // Важные действия (прицеливание)
    public const int PRIORITY_CRITICAL = 4;  // Критически важные действия

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        targetFOV = defaultFOV;
        currentRequest = new FOVRequest(defaultFOV, fovChangeSpeed, 0, this);
    }

    private void Update()
    {
        if (playerCamera.fieldOfView != targetFOV)
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                fovChangeSpeed * Time.deltaTime
            );
        }
    }

    public void RequestFOVChange(float aimFOV, float speed, int priority, object owner)
    {
        if (currentRequest == null || priority > currentRequest.priority || 
            (priority == currentRequest.priority && owner == currentRequest.owner))
        {
            currentRequest = new FOVRequest(aimFOV, speed, priority, owner);
            targetFOV = aimFOV;
            fovChangeSpeed = speed;
        }
    }

    public void ReleaseFOVRequest(object owner)
    {
        if (currentRequest != null && currentRequest.owner == owner)
        {
            currentRequest = new FOVRequest(defaultFOV, currentRequest.speed, 0, this);
            targetFOV = defaultFOV;
            fovChangeSpeed = currentRequest.speed;
        }
    }

    public void ResetFOV(float speed = 8f)
    {
        currentRequest = new FOVRequest(defaultFOV, speed, 0, this);
        targetFOV = defaultFOV;
        fovChangeSpeed = speed;
    }

    public void SetDefaultFOV(float newFOV)
    {
        defaultFOV = newFOV;
        if (currentRequest.priority == 0)
            targetFOV = defaultFOV;
    }
}
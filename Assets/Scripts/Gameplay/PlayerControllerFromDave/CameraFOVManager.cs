using UnityEngine;

public class CameraFOVManager : MonoBehaviour
{
    public static CameraFOVManager Instance { get; private set; }
    
    [Header("Настройки камеры")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float defaultFOV = 60f;
    
    private float targetFOV;
    private float fovChangeSpeed = 15f;
    private int activeAimRequests = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        targetFOV = defaultFOV;
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

    public void RequestAim(float aimFOV, float speed = 15f)
    {
        activeAimRequests++;
        targetFOV = aimFOV;
        fovChangeSpeed = speed;
    }

    public void ReleaseAim()
    {
        activeAimRequests = Mathf.Max(0, activeAimRequests - 1);
        
        if (activeAimRequests == 0)
        {
            ResetFOV();
        }
    }

    public void ResetFOV(float speed = 8f)
    {
        targetFOV = defaultFOV;
        fovChangeSpeed = speed;
    }

    public void SetDefaultFOV(float newFOV)
    {
        defaultFOV = newFOV;
        if (activeAimRequests == 0)
        {
            targetFOV = defaultFOV;
        }
    }
}
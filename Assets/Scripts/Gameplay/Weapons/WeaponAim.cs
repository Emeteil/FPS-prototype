using UnityEngine;

[RequireComponent(typeof(FirearmSystem))]
public class WeaponAim : MonoBehaviour
{
    [Header("Настройки прицеливания")]
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float aimSpeed = 15f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
    
    [Header("Эффекты")]
    [SerializeField] private Animator animator;
    [SerializeField] private string aimAnimParam = "Aiming";
    [SerializeField] private GameObject scopeOverlay;
    
    private bool isAiming = false;
    private FirearmSystem weapon;

    private void Awake()
    {
        weapon = GetComponent<FirearmSystem>();
    }

    private void Update()
    {
        HandleAimInput();
    }

    private void HandleAimInput()
    {
        if (Input.GetKeyDown(aimKey))
            StartAiming();
        else if (Input.GetKeyUp(aimKey))
            StopAiming();
    }

    public void StartAiming()
    {
        if (isAiming || weapon == null) return;
        
        isAiming = true;
        CameraFOVManager.Instance.RequestAim(aimFOV, aimSpeed);
        
        if (animator != null)
            animator.SetBool(aimAnimParam, true);
        
        if (scopeOverlay != null)
            scopeOverlay.SetActive(true);
    }

    public void StopAiming()
    {
        if (!isAiming) return;
        
        isAiming = false;
        CameraFOVManager.Instance.ReleaseAim();
        
        if (animator != null)
            animator.SetBool(aimAnimParam, false);
        
        if (scopeOverlay != null)
            scopeOverlay.SetActive(false);
    }

    private void OnDisable()
    {
        if (isAiming)
        {
            isAiming = false;
            CameraFOVManager.Instance.ReleaseAim();
            
            if (animator != null)
                animator.SetBool(aimAnimParam, false);
            
            if (scopeOverlay != null)
                scopeOverlay.SetActive(false);
        }
    }
}
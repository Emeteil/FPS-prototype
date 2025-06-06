using UnityEngine;

[RequireComponent(typeof(FirearmSystem))]
public class WeaponAim : MonoBehaviour
{
    [Header("Настройки прицеливания")]
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float aimSpeed = 15f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float speedModifierWalk = 0.7f;
    [SerializeField] private float speedModifierSprint = 0.65f;
    [SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
    
    [Header("Эффекты")]
    [SerializeField] private Animator animator;
    [SerializeField] private string aimAnimParam = "Aiming";
    [SerializeField] private GameObject scopeOverlay;

    private const string SPEED_ID = "aiming";

    private bool isAiming = false;
    private FirearmSystem weapon;

    private float aimSensitivityMulti;

    private void Awake()
    {
        weapon = GetComponent<FirearmSystem>();

        aimSensitivityMulti = aimFOV / CameraFOVManager.Instance.DefaultFOV;
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

    private void AddSpeedModifiers()
    {
        PlayerMovment.Instance.AddSpeedModifier(
            SPEED_ID,
            new PlayerMovment.SpeedModifier(speedModifierWalk, true),
            PlayerMovment.SpeedType.Walk
        );
        PlayerMovment.Instance.AddSpeedModifier(
            SPEED_ID,
            new PlayerMovment.SpeedModifier(speedModifierSprint, true),
            PlayerMovment.SpeedType.Sprint
        );
    }

    private void RemoveSpeedModifiers()
    {
        PlayerMovment.Instance.RemoveSpeedModifier(SPEED_ID, PlayerMovment.SpeedType.Walk);
        PlayerMovment.Instance.RemoveSpeedModifier(SPEED_ID, PlayerMovment.SpeedType.Sprint);
    }

    public void StartAiming()
    {
        if (isAiming || weapon == null) return;

        isAiming = true;
        AddSpeedModifiers();
        CameraFOVManager.Instance.RequestFOVChange(aimFOV, aimSpeed, CameraFOVManager.PRIORITY_HIGH, this);
        PlayerCamera.Instance.AddSensitivityModifier(
            SPEED_ID, 
            new PlayerCamera.SensitivityModifier(aimSensitivityMulti, true)
        );

        if (animator != null)
            animator.SetBool(aimAnimParam, true);

        if (scopeOverlay != null)
            scopeOverlay.SetActive(true);
    }

    public void StopAiming()
    {
        if (!isAiming) return;
        
        isAiming = false;
        RemoveSpeedModifiers();
        CameraFOVManager.Instance.ReleaseFOVRequest(this);
        PlayerCamera.Instance.RemoveSensitivityModifier(SPEED_ID);
        
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
            CameraFOVManager.Instance.ReleaseFOVRequest(this);
            RemoveSpeedModifiers();
            
            if (animator != null)
                animator.SetBool(aimAnimParam, false);
            
            if (scopeOverlay != null)
                scopeOverlay.SetActive(false);
        }
    }
}
using System;
using System.Collections;
using UnityEngine;

public enum ControllerSwitchMode
{
    Immediate,          // Мгновенное переключение
    SmoothTransition,   // Плавный переход
    Deferred            // Переключение после завершения анимации
}

public enum CursorState
{
    Default,            // Состояние по умолчанию (зависит от контроллера)
    ForceLocked,        // Принудительно заблокирован
    ForceUnlocked       // Принудительно разблокирован
}

public interface IControllable
{
    void OnControllerEnabled();
    void OnControllerDisabled();
    void OnControllerEnabledAfter() {}
    Transform GetCameraTransform();
    Camera GetCamera();
    CursorState GetPreferredCursorState();
}

[DefaultExecutionOrder(-100)]
public class ControllerManager : MonoBehaviour
{
    public static ControllerManager Instance { get; private set; }

    [SerializeField] private MonoBehaviour defaultController;
    [SerializeField] private float transitionDuration = 2.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private IControllable currentController;
    private IControllable targetController;
    private Coroutine transitionCoroutine;
    private Camera transitionCamera;
    private GameObject transitionCameraObj;
    private bool isTransitioning;
    private CursorState globalCursorState = CursorState.Default;

    public event Action<IControllable> OnControllerEnabled;
    public event Action<IControllable> OnControllerDisabled;
    public event Action<IControllable, IControllable> OnControllerSwitched;
    public event Action OnTransitionStarted;
    public event Action OnTransitionCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (defaultController != null && defaultController is IControllable)
            SwitchController(defaultController as IControllable, ControllerSwitchMode.Immediate);
    }

    public void SwitchController(
        IControllable newController,
        ControllerSwitchMode mode = ControllerSwitchMode.SmoothTransition,
        Action onComplete = null,
        float _transitionDuration = -1,
        AnimationCurve _transitionCurve = null
    )
    {
        if (newController == null || currentController == newController) 
        {
            onComplete?.Invoke();
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            CleanupTransition();
        }
            
        targetController = newController;

        if (_transitionDuration == -1)
            _transitionDuration = transitionDuration;
        
        if (_transitionCurve == null)
            _transitionCurve = transitionCurve;

        switch (mode)
            {
                case ControllerSwitchMode.Immediate:
                    PerformImmediateSwitch();
                    onComplete?.Invoke();
                    break;

                case ControllerSwitchMode.SmoothTransition:
                    transitionCoroutine = StartCoroutine(SmoothTransition(
                        onComplete,
                        _transitionDuration,
                        _transitionCurve
                    ));
                    break;

                case ControllerSwitchMode.Deferred:
                    transitionCoroutine = StartCoroutine(DeferredTransition(
                        onComplete,
                        _transitionDuration,
                        _transitionCurve
                    ));
                    break;
            }
    }

    public void SwitchToDefault(
        ControllerSwitchMode mode = ControllerSwitchMode.SmoothTransition,
        Action onComplete = null,
        float _transitionDuration = -1,
        AnimationCurve _transitionCurve = null
    )
    {
        if (defaultController != null && defaultController is IControllable)
            SwitchController(
                defaultController as IControllable, mode,
                onComplete,
                _transitionDuration,
                _transitionCurve
            );
    }

    public void SetGlobalCursorState(CursorState state)
    {
        globalCursorState = state;
        UpdateCursorState();
    }

    public bool IsCurrentController(IControllable controller)
    {
        return currentController == controller;
    }

    public bool IsTransitioning => isTransitioning;

    private void PerformImmediateSwitch()
    {
        Camera currentCam = currentController?.GetCamera();

        if (currentController != null)
        {
            currentController.OnControllerDisabled();
            OnControllerDisabled?.Invoke(currentController);
            if (currentCam != null) currentCam.enabled = false;
        }

        Camera targetCam = targetController?.GetCamera();

        var previousController = currentController;
        currentController = targetController;
        targetController = null;

        currentController.OnControllerEnabledAfter();
        currentController.OnControllerEnabled();
        OnControllerEnabled?.Invoke(currentController);
        OnControllerSwitched?.Invoke(previousController, currentController);
        if (targetCam != null) targetCam.enabled = true;

        UpdateCursorState();
    }

    private IEnumerator SmoothTransition(
        Action onComplete,
        float transitionDuration = -1,
        AnimationCurve transitionCurve = null
    )
    {
        isTransitioning = true;
        OnTransitionStarted?.Invoke();

        Camera currentCam = currentController?.GetCamera();

        SetupTransitionCamera(currentController);

        if (currentController != null)
        {
            currentController.OnControllerDisabled();
            OnControllerDisabled?.Invoke(currentController);
            if (currentCam != null) currentCam.enabled = false;
        }

        Transform startTransform = transitionCameraObj.transform;
        Transform endTransform = targetController.GetCameraTransform();
        Camera targetCam = targetController.GetCamera();

        float elapsedTime = 0f;
        Vector3 startPos = startTransform.position;
        Quaternion startRot = startTransform.rotation;
        float startFOV = transitionCamera.fieldOfView;

        float targetFOV = targetCam.fieldOfView;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsedTime / transitionDuration);

            transitionCameraObj.transform.position = Vector3.Lerp(startPos, endTransform.position, t);
            transitionCameraObj.transform.rotation = Quaternion.Slerp(startRot, endTransform.rotation, t);
            transitionCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            yield return null;
        }

        var previousController = currentController;
        currentController = targetController;
        targetController = null;

        currentController.OnControllerEnabledAfter();
        currentController.OnControllerEnabled();
        OnControllerEnabled?.Invoke(currentController);
        OnControllerSwitched?.Invoke(previousController, currentController);
        targetCam.enabled = true;

        CompleteTransition(onComplete);
    }

    private IEnumerator DeferredTransition(
        Action onComplete,
        float transitionDuration = -1,
        AnimationCurve transitionCurve = null
    )
    {
        isTransitioning = true;
        OnTransitionStarted?.Invoke();

        currentController.OnControllerDisabled();

        var previousController = currentController;
        currentController = targetController;
        targetController = null;

        currentController.OnControllerEnabled();
        OnControllerEnabled?.Invoke(currentController);
        OnControllerSwitched?.Invoke(previousController, currentController);

        SetupTransitionCamera(previousController);

        Transform startTransform = transitionCameraObj.transform;
        Transform endTransform = currentController.GetCameraTransform();

        float elapsedTime = 0f;
        Vector3 startPos = startTransform.position;
        Quaternion startRot = startTransform.rotation;
        float startFOV = transitionCamera.fieldOfView;
        float targetFOV = currentController.GetCamera().fieldOfView;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsedTime / transitionDuration);

            transitionCameraObj.transform.position = Vector3.Lerp(startPos, endTransform.position, t);
            transitionCameraObj.transform.rotation = Quaternion.Slerp(startRot, endTransform.rotation, t);
            transitionCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            yield return null;
        }

        currentController.OnControllerEnabledAfter();

        CompleteTransition(onComplete);
    }

    private void SetupTransitionCamera(IControllable sourceController)
    {
        if (sourceController == null) return;

        transitionCameraObj = new GameObject("Transition Camera");
        transitionCamera = transitionCameraObj.AddComponent<Camera>();
        
        Camera sourceCam = sourceController.GetCamera();
        Transform sourceTransform = sourceController.GetCameraTransform();

        transitionCameraObj.transform.position = sourceTransform.position;
        transitionCameraObj.transform.rotation = sourceTransform.rotation;
        transitionCamera.fieldOfView = sourceCam.fieldOfView;
        transitionCamera.nearClipPlane = sourceCam.nearClipPlane;
        transitionCamera.farClipPlane = sourceCam.farClipPlane;
        transitionCamera.cullingMask = sourceCam.cullingMask;
        
        sourceCam.enabled = false;
    }

    private void CompleteTransition(Action onComplete)
    {
        CleanupTransition();

        if (currentController != null)
            currentController.GetCamera().enabled = true;

        isTransitioning = false;
        OnTransitionCompleted?.Invoke();
        onComplete?.Invoke();
    }

    private void CleanupTransition()
    {
        if (transitionCameraObj != null)
        {
            Destroy(transitionCameraObj);
            transitionCameraObj = null;
            transitionCamera = null;
        }

        transitionCoroutine = null;
    }

    private void UpdateCursorState()
    {
        if (currentController == null) return;

        CursorState effectiveState = globalCursorState == CursorState.Default ? 
            currentController.GetPreferredCursorState() : globalCursorState;

        switch (effectiveState)
        {
            case CursorState.ForceLocked:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
                
            case CursorState.ForceUnlocked:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
                
            default:
                break;
        }
    }
}
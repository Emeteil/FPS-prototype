using UnityEngine;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance { get; private set; }

    [Header("Base Sensitivity")]
    [SerializeField] private float baseSensitivityX = 150f;
    [SerializeField] private float baseSensitivityY = 150f;

    private Dictionary<string, SensitivityModifier> sensitivityModifiers = new Dictionary<string, SensitivityModifier>();

    public float sensitivityX { get; private set; }
    public float sensitivityY { get; private set; }

    public Transform orientation;

    float RotationX;
    float RotationY;

    private bool _block = false;
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateActualSensitivity();
    }

    private void Start()
    {
        Pause.Instance.AddScript(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (_block) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * sensitivityY;

        RotationY += mouseX;
        RotationX -= mouseY;
        RotationX = Mathf.Clamp(RotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(RotationX, RotationY, 0);
        orientation.rotation = Quaternion.Euler(0, RotationY, 0);
    }

    private void UpdateActualSensitivity()
    {
        sensitivityX = CalculateModifiedSensitivity(baseSensitivityX);
        sensitivityY = CalculateModifiedSensitivity(baseSensitivityY);
    }

    private float CalculateModifiedSensitivity(float baseSensitivity)
    {
        if (sensitivityModifiers.Count == 0) return baseSensitivity;

        float result = baseSensitivity;
        foreach (var modifier in sensitivityModifiers.Values)
        {
            if (modifier.isMultiplicative)
                result *= modifier.value;
            else
                result += modifier.value;
        }
        return result;
    }

    public void AddSensitivityModifier(string id, SensitivityModifier modifier)
    {
        sensitivityModifiers[id] = modifier;
        UpdateActualSensitivity();
    }

    public void RemoveSensitivityModifier(string id)
    {
        if (sensitivityModifiers.ContainsKey(id))
        {
            sensitivityModifiers.Remove(id);
            UpdateActualSensitivity();
        }
    }

    public struct SensitivityModifier
    {
        public float value;
        public bool isMultiplicative;

        public SensitivityModifier(float value, bool isMultiplicative)
        {
            this.value = value;
            this.isMultiplicative = isMultiplicative;
        }
    }
}
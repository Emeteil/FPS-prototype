using System.Collections.Generic;
using UnityEngine;

public class CameraShakeSystem : MonoBehaviour
{
    public static CameraShakeSystem Instance { get; private set; }
    public bool enableShake = false;

    [System.Serializable]
    public class ShakeProfile
    {
        public string profileName = "New Shake";
        public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public float duration = 0.5f;
        public Vector3 positionStrength = Vector3.one * 0.1f;
        public Vector3 rotationStrength = Vector3.one * 2f;
        public float noiseFrequency = 10f;

        [System.NonSerialized] public Vector3 noiseOffset;
    }

    [Header("Основные настройки")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private float maxShakeIntensity = 2f;

    [Header("Тряска от движения")]
    public bool enableMovementShake = true;
    public AnimationCurve movementShakeCurve = AnimationCurve.Linear(0, 0, 10, 1);
    public Vector3 movementPositionStrength = new Vector3(0.1f, 0.2f, 0.05f);
    public Vector3 movementRotationStrength = new Vector3(1f, 1f, 0.5f);

    [Header("Профили тряски")]
    [SerializeField] private List<ShakeProfile> shakeProfiles = new List<ShakeProfile>();

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private readonly Dictionary<string, ShakeInstance> activeShakes = new Dictionary<string, ShakeInstance>();
    private float currentSpreadInfluence = 0f;

    private class ShakeInstance
    {
        public ShakeProfile profile;
        public float elapsedTime;
        public Vector3 noiseOffset;
    }

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (!enabled) return;

        if (cameraTransform == null) cameraTransform = transform;
        originalPosition = cameraTransform.localPosition;
        originalRotation = cameraTransform.localRotation;

        foreach (var profile in shakeProfiles)
        {
            profile.noiseOffset = new Vector3(
                Random.Range(0f, 100f),
                Random.Range(0f, 100f),
                Random.Range(0f, 100f));
        }
    }

    private void Start()
    {
        Pause.Instance.AddScript(this);
    }

    private void Update()
    {
        if (_block) return;

        cameraTransform.localPosition = originalPosition;
        cameraTransform.localRotation = originalRotation;

        if (!enableShake) return;

        ApplyActiveShakes();
        ApplyMovementShake();
        ApplySpreadInfluence();
    }

    public void SetSpreadInfluence(float normalizedSpread)
    {
        currentSpreadInfluence = Mathf.Clamp01(normalizedSpread);
    }

    private void ApplySpreadInfluence()
    {
        if (currentSpreadInfluence <= 0) return;

        float intensity = currentSpreadInfluence * 0.5f;
        Vector3 posOffset = new Vector3(
            (Mathf.PerlinNoise(Time.time * 20f, 0) - 0.5f) * intensity,
            (Mathf.PerlinNoise(0, Time.time * 20f) - 0.5f) * intensity,
            0);

        cameraTransform.localPosition += posOffset;
        currentSpreadInfluence = Mathf.MoveTowards(currentSpreadInfluence, 0f, Time.deltaTime);
    }

    private void ApplyMovementShake()
    {
        if (!enableMovementShake || playerRigidbody == null) return;

        float speed = playerRigidbody.velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(speed / 10f);
        float intensity = movementShakeCurve.Evaluate(normalizedSpeed);

        if (intensity <= 0) return;

        Vector3 posOffset = new Vector3(
            (Mathf.PerlinNoise(Time.time * 2f, 0) - 0.5f) * movementPositionStrength.x * intensity,
            (Mathf.PerlinNoise(0, Time.time * 2f) - 0.5f) * movementPositionStrength.y * intensity,
            (Mathf.PerlinNoise(Time.time * 2f, Time.time * 3f) - 0.5f) * movementPositionStrength.z * intensity
        );

        Vector3 rotOffset = new Vector3(
            (Mathf.PerlinNoise(Time.time * 2f, 100) - 0.5f) * movementRotationStrength.x * intensity,
            (Mathf.PerlinNoise(100, Time.time * 2f) - 0.5f) * movementRotationStrength.y * intensity,
            (Mathf.PerlinNoise(Time.time * 2f, Time.time * 4f + 100) - 0.5f) * movementRotationStrength.z * intensity
        );

        cameraTransform.localPosition += posOffset;
        cameraTransform.localRotation *= Quaternion.Euler(rotOffset);
    }

    private void ApplyActiveShakes()
    {
        List<string> toRemove = new List<string>();

        foreach (var kvp in activeShakes)
        {
            var instance = kvp.Value;
            float progress = instance.elapsedTime / instance.profile.duration;
            float intensity = instance.profile.intensityCurve.Evaluate(progress);

            Vector3 posNoise = new Vector3(
                Mathf.PerlinNoise(Time.time * instance.profile.noiseFrequency + instance.profile.noiseOffset.x, 0),
                Mathf.PerlinNoise(0, Time.time * instance.profile.noiseFrequency + instance.profile.noiseOffset.y),
                Mathf.PerlinNoise(Time.time * instance.profile.noiseFrequency * 0.7f + instance.profile.noiseOffset.z, 
                                 Time.time * instance.profile.noiseFrequency * 0.7f + instance.profile.noiseOffset.z)
            );

            Vector3 rotNoise = new Vector3(
                Mathf.PerlinNoise(Time.time * instance.profile.noiseFrequency * 1.2f + instance.profile.noiseOffset.x, 100),
                Mathf.PerlinNoise(100, Time.time * instance.profile.noiseFrequency * 1.2f + instance.profile.noiseOffset.y),
                Mathf.PerlinNoise(Time.time * instance.profile.noiseFrequency + instance.profile.noiseOffset.z, 
                                 Time.time * instance.profile.noiseFrequency + instance.profile.noiseOffset.z + 100)
            );

            Vector3 posOffset = new Vector3(
                (posNoise.x - 0.5f) * instance.profile.positionStrength.x * intensity,
                (posNoise.y - 0.5f) * instance.profile.positionStrength.y * intensity,
                (posNoise.z - 0.5f) * instance.profile.positionStrength.z * intensity
            );

            Vector3 rotOffset = new Vector3(
                (rotNoise.x - 0.5f) * instance.profile.rotationStrength.x * intensity,
                (rotNoise.y - 0.5f) * instance.profile.rotationStrength.y * intensity,
                (rotNoise.z - 0.5f) * instance.profile.rotationStrength.z * intensity
            );

            float totalIntensity = (posOffset.magnitude + rotOffset.magnitude) / 2f;
            if (totalIntensity > maxShakeIntensity)
            {
                float scale = maxShakeIntensity / totalIntensity;
                posOffset *= scale;
                rotOffset *= scale;
            }

            cameraTransform.localPosition += posOffset;
            cameraTransform.localRotation *= Quaternion.Euler(rotOffset);

            instance.elapsedTime += Time.deltaTime;
            if (instance.elapsedTime >= instance.profile.duration)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
            activeShakes.Remove(key);
    }

    public void TriggerShake(string profileName, float intensityScale = 1f)
    {
        var profile = shakeProfiles.Find(p => p.profileName == profileName);
        if (profile == null)
        {
            Debug.LogWarning($"Shake profile '{profileName}' not found!");
            return;
        }

        if (activeShakes.ContainsKey(profileName))
        {
            activeShakes[profileName].elapsedTime = 0f;
        }
        else
        {
            var instance = new ShakeInstance
            {
                profile = profile,
                elapsedTime = 0f
            };
            activeShakes.Add(profileName, instance);
        }
    }
}
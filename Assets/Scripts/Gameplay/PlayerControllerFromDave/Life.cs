using System;
using UnityEngine;
using UnityEngine.Events;

public class Life : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent<float> OnHealthChanged;

    [NonSerialized] public bool dead = false;
    [HideInInspector] public bool _block = false;
    private bool _ignorePause = false;

    public static Life Instance { get; private set; }

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

    private void Update()
    {        
        if (_block) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            Kill();
            return;
        }

        if (!dead) return;
        if (!Input.anyKeyDown) return;

        SceneChanger.Instance.ChangeScene();
    }

    public bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (currentHealth <= 0 || dead) return false;

        currentHealth -= damage;
        if (currentHealth <= 0)
            currentHealth = 0;
        
        OnDamageTaken.Invoke(damage);
        OnHealthChanged.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Kill();
            return true;
        }

        return false;
    }

    public void Kill()
    {
        ControllerManager.Instance.SwitchToDefault(ControllerSwitchMode.Immediate, onComplete: () => {
            PlayerMovment.Instance.CrouchDown();
            PlayerMovment.Instance.Block();
            PlayerInteraction.Instance.Block();
            InventorySystem.Instance.Block();

            GrabUp.Instance.ReleaseObject();
            GrabUp.Instance.Block();

            OnDeath.Invoke();
            currentHealth = 0;
            dead = true;
        });
    }

    private const string DECELERATE_ID = "decelerate";
    private const string STRONG_DECELERATE_ID = "strong_decelerate";

    public void Decelerate()
    {
        RemoveAllDecelerationEffects();
        
        PlayerMovment.Instance.AddSpeedModifier(
            DECELERATE_ID,
            new PlayerMovment.SpeedModifier(1f/1.5f, true),
            PlayerMovment.SpeedType.Walk
        );
        
        PlayerMovment.Instance.AddSpeedModifier(
            DECELERATE_ID,
            new PlayerMovment.SpeedModifier(1f, true),
            PlayerMovment.SpeedType.Sprint
        );
    }

    public void StronglyDecelerate()
    {
        RemoveAllDecelerationEffects();
        
        PlayerMovment.Instance.AddSpeedModifier(
            STRONG_DECELERATE_ID,
            new PlayerMovment.SpeedModifier(0.5f, true),
            PlayerMovment.SpeedType.Walk
        );
        
        PlayerMovment.Instance.AddSpeedModifier(
            STRONG_DECELERATE_ID,
            new PlayerMovment.SpeedModifier(0.5f, true),
            PlayerMovment.SpeedType.Sprint
        );
        
        PlayerMovment.Instance.AddSpeedModifier(
            STRONG_DECELERATE_ID,
            new PlayerMovment.SpeedModifier(1f/1.5f, true),
            PlayerMovment.SpeedType.Crouch
        );
    }

    public void ResetDecelerate()
    {
        RemoveAllDecelerationEffects();
    }

    private void RemoveAllDecelerationEffects()
    {
        PlayerMovment.Instance.RemoveSpeedModifier(DECELERATE_ID, PlayerMovment.SpeedType.Walk);
        PlayerMovment.Instance.RemoveSpeedModifier(DECELERATE_ID, PlayerMovment.SpeedType.Sprint);
        PlayerMovment.Instance.RemoveSpeedModifier(DECELERATE_ID, PlayerMovment.SpeedType.Crouch);
        
        PlayerMovment.Instance.RemoveSpeedModifier(STRONG_DECELERATE_ID, PlayerMovment.SpeedType.Walk);
        PlayerMovment.Instance.RemoveSpeedModifier(STRONG_DECELERATE_ID, PlayerMovment.SpeedType.Sprint);
        PlayerMovment.Instance.RemoveSpeedModifier(STRONG_DECELERATE_ID, PlayerMovment.SpeedType.Crouch);
    }
}

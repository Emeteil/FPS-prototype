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

    private float walkSpeed;
    private float sprintSpeed;
    private float crouchSpeed;
    
    private PlayerMovment playerMovment;
    private PlayerInteraction playerInteraction;
    private GrabUp grabUp;

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

    private void Start()
    {
        playerMovment = GetComponent<PlayerMovment>();
        playerInteraction = GetComponent<PlayerInteraction>();
        grabUp = GetComponent<GrabUp>();

        walkSpeed = playerMovment.walkSpeed;
        sprintSpeed = playerMovment.sprintSpeed;
        crouchSpeed = playerMovment.crouchSpeed;
    }

    private void Update()
    {        
        if (_block) return;
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
        ControllerManager.Instance.SwitchToDefault(ControllerSwitchMode.Immediate);
        playerMovment.CrouchDown();
        playerMovment.Block();
        playerInteraction.Block();

        grabUp.ReleaseObject();
        grabUp.Block();

        OnDeath.Invoke();
        currentHealth = 0;
        dead = true;
    }

    public void Decelerate()
    {
        playerMovment.walkSpeed = walkSpeed / 1.5f;
        playerMovment.sprintSpeed = walkSpeed;
        playerMovment.crouchSpeed = crouchSpeed;
    }

    public void StronglyDecelerate()
    {
        playerMovment.walkSpeed = walkSpeed / 2f;
        playerMovment.sprintSpeed = walkSpeed / 2f;
        playerMovment.crouchSpeed = crouchSpeed / 1.5f;
    }

    public void ResetDecelerate()
    {
        playerMovment.walkSpeed = walkSpeed;
        playerMovment.sprintSpeed = sprintSpeed;
        playerMovment.crouchSpeed = crouchSpeed;
    }
}

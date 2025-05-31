using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class DamageProfile
{
    [Header("Основные настройки")]
    [Range(0.1f, 1000f)] public float baseDamage = 10f;
    [Range(0f, 5f)] public float headshotMultiplier = 2f;
    [Range(0f, 5f)] public float limbMultiplier = 0.5f;
    public LayerMask damageLayers;
    public bool penetrateWalls = false;

    [Header("Физика выстрела")]
    [Tooltip("Сила импульса при попадании")]
    public float impactForce = 6f;

    [Tooltip("Максимальная масса объекта для применения силы")]
    public float maxAffectedMass = 50f;

    [Tooltip("Учитывать ли дистанцию при расчете урона")]
    public bool considerDistanceForce = true;
    [Tooltip("Расстояние, на котором учитывается дистанция")]
    public float maxDistanceForce = 50f;

    [Header("Дистанционный урон")]
    [Tooltip("Влияет ли дистанция на урон")]
    public bool useDistanceFalloff = false;

    [Tooltip("Дистанция, на которой урон начинает снижаться")]
    public float minFalloffDistance = 10f;

    [Tooltip("Дистанция, на которой урон достигает минимального значения")]
    public float maxFalloffDistance = 50f;

    [Range(0f, 1f)]
    [Tooltip("Минимальный множитель урона на максимальной дистанции")]
    public float minDamageMultiplier = 0.75f;

    [Tooltip("Кривая зависимости урона от дистанции")]
    public AnimationCurve distanceFalloffCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 0.5f)
    );

    [Header("Рандомизация урона")]
    [Tooltip("Включить случайное отклонение урона")]
    public bool useRandomDamage = true;

    [Tooltip("Максимальное отклонение урона в обе стороны")]
    [Range(0f, 50f)] public float damageRandomization = 2f;

    [Tooltip("Минимальный гарантированный урон (0-1)")]
    [Range(0f, 1f)] public float minDamageCap = 0.5f;

    [Header("Эффекты")]
    public GameObject impactEffect;
    public GameObject bloodEffect;
    public GameObject decalEffect;
}

[System.Serializable]
public class RecoilProfile
{
    [Header("Отдача")]
    public Vector3 recoilKick = new Vector3(0.5f, 1f, 0f);
    public float recoilRecoverySpeed = 5f;
    public float recoilSnappiness = 10f;
    public float maxRecoilAngle = 30f;
}

[System.Serializable]
public class AmmoSystem
{
    [Header("Боеприпасы")]
    public int clipSize = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 2f;
    public bool infiniteAmmo = false;

    [Header("Пополнение")]
    [Tooltip("Максимальный запас патронов (0 = нет лимита)")]
    public int maxReserveAmmo = 0;
}

[System.Serializable]
public class FireMode
{
    public enum WeaponFireMode { Semi, Burst, Auto }
    public WeaponFireMode fireMode = WeaponFireMode.Auto;
    [Range(0.01f, 2f)] public float fireRate = 0.1f;
    [Range(1, 10)] public int burstCount = 3;
    [Range(0.01f, 0.5f)] public float burstDelay = 0.1f;
}

[System.Serializable]
public class SpreadSettings
{
    [Header("Разброс")]
    [Range(0f, 10f)] public float baseSpread = 1f;
    [Range(0f, 10f)] public float maxSpread = 3f;
    [Range(0f, 10f)] public float spreadPerShot = 0.2f;
    [Range(0f, 10f)] public float spreadRecoveryRate = 1f;
    [Range(0f, 5f)] public float movementSpreadMultiplier = 1.5f;
}

[System.Serializable]
public class WeaponEvents
{
    [Header("События")]
    public UnityEvent OnShoot;
    public UnityEvent OnReloadStart;
    public UnityEvent OnReloadEnd;
    public UnityEvent OnAmmoEmpty;
    public UnityEvent OnHit;
    public UnityEvent OnKill;
    public UnityEvent<float> OnDamageDealt; // Передает количество урона
    public UnityEvent<int, int> OnAmmoChanged; // Текущие патроны, резерв
}

public class FirearmSystem : MonoBehaviour
{
    [Header("Основные компоненты")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator weaponAnimator;

    [Header("Настройки")]
    public DamageProfile damageProfile;
    public RecoilProfile recoilProfile;
    public AmmoSystem ammoSystem;
    public FireMode fireMode;
    public SpreadSettings spreadSettings;
    public WeaponEvents weaponEvents;

    [Header("Визуальные эффекты")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject shellEjectEffect;
    [SerializeField] private Transform shellEjectPoint;

    [Header("Эффекты пополнения")]
    public ParticleSystem ammoPickupEffect;
    public AudioClip ammoPickupSound;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private Color debugRayColor = Color.red;
    [SerializeField] private float debugRayDuration = 3f;
    [SerializeField] private bool showHitPoint = true;
    [SerializeField] private Color hitPointColor = Color.yellow;
    [SerializeField] private float hitPointSize = 0.1f;

    private float nextFireTime;
    private float currentSpread;
    private Vector3 currentRecoil;
    private bool isReloading;
    private int shotsInBurst;
    private bool burstFiring;
    private bool _block = false;
    private bool _ignorePause = false;

    private float lastDamageDealt;
    private float lastHitDistance;
    private string lastHitBodyPart;

    private void OnDisable()
    {
        ResetWeaponState();
    }

    private void ResetWeaponState()
    {
        StopAllCoroutines();

        isReloading = false;
        burstFiring = false;

        if (weaponAnimator != null)
        {
            weaponAnimator.speed = 1f;
            weaponAnimator.ResetTrigger("Reload");
            weaponAnimator.ResetTrigger("Fire");
        }

        if (muzzleFlash != null && muzzleFlash.isPlaying)
            muzzleFlash.Stop();
    }

    public void Block(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = true;
        _block = true;

        if (weaponAnimator != null)
        {
            weaponAnimator.speed = 0f;
            weaponAnimator.ResetTrigger("Reload");
        }
    }

    public void Unblock(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = false;
        _block = false;

        if (weaponAnimator != null)
            weaponAnimator.speed = 1f;
    }

    private void Start()
    {
        Pause.Instance.AddScript(this);

        if (playerCamera == null)
            playerCamera = Camera.main;

        UpdateAmmoUI();
    }

    private void Update()
    {
        if (_block) return;

        HandleInput();
        UpdateRecoil();
        UpdateSpread();
    }

    private void HandleInput()
    {
        if (_block || isReloading) return;

        if (Input.GetKeyDown(KeyCode.R) && ammoSystem.currentAmmo < ammoSystem.clipSize && ammoSystem.reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetKeyDown(PlayerInteraction.Instance.InteractionKey))
        {
            TryPickUpAmmo();
            return;
        }

        switch (fireMode.fireMode)
        {
            case FireMode.WeaponFireMode.Semi:
                if (Input.GetButtonDown("Fire1")) TryShoot();
                break;
            case FireMode.WeaponFireMode.Burst:
                if (Input.GetButtonDown("Fire1") && !burstFiring) StartCoroutine(BurstFire());
                break;
            case FireMode.WeaponFireMode.Auto:
                if (Input.GetButton("Fire1")) TryShoot();
                break;
        }
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        if (ammoSystem.currentAmmo <= 0)
        {
            weaponEvents.OnAmmoEmpty.Invoke();
            return;
        }

        Shoot();
        nextFireTime = Time.time + fireMode.fireRate;
    }

    private void TryPickUpAmmo()
    {
        RaycastHit hit;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out hit, PlayerInteraction.Instance.InteractionDistance, ~0, QueryTriggerInteraction.Ignore))
            return;

        if (hit.collider == null) return;

        Ammunition ammunition = hit.collider.GetComponent<Ammunition>();

        if (ammunition == null) return;

        if (AddAmmo(ammunition.Ammo) > 0)
            ammunition.PickUp();
    }

    private IEnumerator BurstFire()
    {
        burstFiring = true;
        shotsInBurst = 0;

        while (shotsInBurst < fireMode.burstCount && ammoSystem.currentAmmo > 0)
        {
            TryShoot();
            shotsInBurst++;
            yield return new WaitForSeconds(fireMode.burstDelay);
        }

        burstFiring = false;
    }

    private void Shoot()
    {
        if (!ammoSystem.infiniteAmmo)
        {
            ammoSystem.currentAmmo--;
            UpdateAmmoUI();
        }

        if (muzzleFlash != null) muzzleFlash.Play();
        if (shellEjectEffect != null && shellEjectPoint != null)
            Instantiate(shellEjectEffect, shellEjectPoint.position, shellEjectPoint.rotation);

        weaponEvents.OnShoot.Invoke();
        if (weaponAnimator != null) weaponAnimator.SetTrigger("Fire");

        ApplyRecoil();

        currentSpread = Mathf.Min(currentSpread + spreadSettings.spreadPerShot, spreadSettings.maxSpread);

        Vector3 shootDirection = CalculateSpread();

        Ray ray = new Ray(playerCamera.transform.position, shootDirection);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, 1000f, damageProfile.damageLayers);

        if (debugMode)
        {
            Vector3 endPoint = hasHit ? hit.point : ray.origin + ray.direction * 1000f;
            DrawWorldRay(ray.origin, endPoint - ray.origin, debugRayColor, debugRayDuration);

            if (hasHit && showHitPoint)
            {
                DrawWorldLine(hit.point - Vector3.up * hitPointSize,
                             hit.point + Vector3.up * hitPointSize,
                             hitPointColor, debugRayDuration);
                DrawWorldLine(hit.point - Vector3.right * hitPointSize,
                             hit.point + Vector3.right * hitPointSize,
                             hitPointColor, debugRayDuration);
                DrawWorldLine(hit.point - Vector3.forward * hitPointSize,
                             hit.point + Vector3.forward * hitPointSize,
                             hitPointColor, debugRayDuration);
            }
        }

        if (hasHit)
            ProcessHit(hit);
    }

    public int AddAmmo(int amount, bool allowOverflow = false)
    {
        if (amount <= 0) return 0;
        if (ammoSystem.infiniteAmmo) return amount;

        int ammoBefore = ammoSystem.reserveAmmo;
        int potentialAmmo = ammoSystem.reserveAmmo + amount;

        if (ammoSystem.maxReserveAmmo > 0 && !allowOverflow)
        {
            potentialAmmo = Mathf.Min(potentialAmmo, ammoSystem.maxReserveAmmo);
        }

        ammoSystem.reserveAmmo = potentialAmmo;
        UpdateAmmoUI();

        if (ammoSystem.reserveAmmo - ammoBefore > 0)
        {
            if (ammoPickupEffect != null)
                ammoPickupEffect.Play();

            if (ammoPickupSound != null)
                AudioSource.PlayClipAtPoint(ammoPickupSound, transform.position);
        }

        return ammoSystem.reserveAmmo - ammoBefore;
    }

    private Vector3 CalculateSpread()
    {
        float movementSpread = IsMoving() ? spreadSettings.movementSpreadMultiplier : 1f;
        float totalSpread = currentSpread * movementSpread;

        Vector2 randomSpread = Random.insideUnitCircle * totalSpread * 0.1f;
        Vector3 direction = playerCamera.transform.forward +
                          playerCamera.transform.right * randomSpread.x +
                          playerCamera.transform.up * randomSpread.y;

        return direction.normalized;
    }

    private bool IsMoving()
    {
        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }

    private void ProcessHit(RaycastHit hit)
    {
        SpawnImpactEffects(hit);

        ApplyImpactForce(hit, damageProfile.impactForce);

        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            float damage = CalculateDamage(hit);

            lastDamageDealt = damage;
            lastHitDistance = Vector3.Distance(playerCamera.transform.position, hit.point);
            lastHitBodyPart = hit.collider.name;

            bool wasKill = damageable.TakeDamage(damage, hit.point, hit.normal);

            weaponEvents.OnHit.Invoke();
            weaponEvents.OnDamageDealt.Invoke(damage);

            if (wasKill) weaponEvents.OnKill.Invoke();
        }
    }

    private void ApplyImpactForce(RaycastHit hit, float baseForce)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.mass > damageProfile.maxAffectedMass) return;

        float forceMultiplier = 1f;

        float totalForce = baseForce * forceMultiplier;

        if (damageProfile.considerDistanceForce)
        {
            float distance = Vector3.Distance(playerCamera.transform.position, hit.point);
            float distanceFalloff = Mathf.Clamp01(1 - distance / damageProfile.maxDistanceForce);
            totalForce *= distanceFalloff;
        }

        Vector3 forceDirection = hit.point - playerCamera.transform.position;
        rb.AddForceAtPosition(
            forceDirection.normalized * totalForce,
            hit.point,
            ForceMode.Impulse
        );

        if (debugMode)
        {
            DrawWorldLine(
                hit.point,
                hit.point + forceDirection.normalized * 2f,
                Color.cyan, 1f
            );
        }
    }

    private float CalculateDamage(RaycastHit hit)
    {
        float damage = damageProfile.baseDamage;

        // TODO: Remake this
        // if (hit.collider.CompareTag("Head"))
        // {
        //     damage *= damageProfile.headshotMultiplier;
        // }
        // else if (hit.collider.CompareTag("Limb"))
        // {
        //     damage *= damageProfile.limbMultiplier;
        // }

        if (damageProfile.useDistanceFalloff && damage > 0)
        {
            float distance = Vector3.Distance(playerCamera.transform.position, hit.point);
            damage *= CalculateDistanceMultiplier(distance);
        }

        if (damageProfile.useRandomDamage && damageProfile.damageRandomization > 0)
        {
            float randomization = Mathf.Min(
                damageProfile.damageRandomization,
                Mathf.Abs(damage)
            );

            damage += Random.Range(-randomization, randomization);

            if (damage > 0)
                damage = Mathf.Max(damage, damageProfile.baseDamage * damageProfile.minDamageCap);
            else if (damage < 0)
                damage = Mathf.Min(damage, -damageProfile.baseDamage * damageProfile.minDamageCap);
        }

        return damage;
    }

    private float CalculateDistanceMultiplier(float distance)
    {
        if (!damageProfile.useDistanceFalloff) return 1f;

        if (damageProfile.distanceFalloffCurve.length == 0)
        {
            float t = Mathf.InverseLerp(
                damageProfile.minFalloffDistance,
                damageProfile.maxFalloffDistance,
                distance
            );
            return Mathf.Lerp(1f, damageProfile.minDamageMultiplier, t);
        }
        else
        {
            float normalizedDistance = Mathf.Clamp01(
                (distance - damageProfile.minFalloffDistance) /
                (damageProfile.maxFalloffDistance - damageProfile.minFalloffDistance)
            );
            return damageProfile.distanceFalloffCurve.Evaluate(normalizedDistance);
        }
    }

    private void SpawnImpactEffects(RaycastHit hit)
    {
        if (damageProfile.impactEffect != null)
        {
            GameObject impact = Instantiate(damageProfile.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f);
        }

        if (hit.collider.GetComponent<IDamageable>() != null && damageProfile.bloodEffect != null)
        {
            GameObject blood = Instantiate(damageProfile.bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(blood, 2f);
        }

        if (damageProfile.decalEffect != null)
        {
            GameObject decal = Instantiate(damageProfile.decalEffect, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(-hit.normal));
            decal.transform.parent = hit.transform;
            Destroy(decal, 10f);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        weaponEvents.OnReloadStart?.Invoke();

        if (weaponAnimator != null)
        {
            weaponAnimator.ResetTrigger("Fire");
            weaponAnimator.SetTrigger("Reload");
        }

        float reloadTimer = 0f;
        while (reloadTimer < ammoSystem.reloadTime)
        {
            reloadTimer += Time.deltaTime;
            yield return null;
        }

        if (!isActiveAndEnabled)
        {
            isReloading = false;
            yield break;
        }

        CompleteReload();
    }

    private void CompleteReload()
    {
        int ammoNeeded = ammoSystem.clipSize - ammoSystem.currentAmmo;
        int ammoToAdd = Mathf.Min(ammoNeeded, ammoSystem.reserveAmmo);

        if (!ammoSystem.infiniteAmmo)
        {
            ammoSystem.currentAmmo += ammoToAdd;
            ammoSystem.reserveAmmo -= ammoToAdd;
        }
        else
        {
            ammoSystem.currentAmmo = ammoSystem.clipSize;
        }

        UpdateAmmoUI();
        isReloading = false;
        weaponEvents.OnReloadEnd?.Invoke();
    }

    private void UpdateAmmoUI()
    {
        weaponEvents.OnAmmoChanged.Invoke(ammoSystem.currentAmmo, ammoSystem.reserveAmmo);
    }

    private void ApplyRecoil()
    {
        currentRecoil += new Vector3(
            -recoilProfile.recoilKick.x,
            Random.Range(-recoilProfile.recoilKick.y, recoilProfile.recoilKick.y),
            Random.Range(-recoilProfile.recoilKick.z, recoilProfile.recoilKick.z)
        );
    }

    private void UpdateRecoil()
    {
        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, recoilProfile.recoilRecoverySpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            Quaternion.Euler(currentRecoil),
            recoilProfile.recoilSnappiness * Time.deltaTime
        );
    }

    private void UpdateSpread()
    {
        if (!Input.GetButton("Fire1"))
        {
            currentSpread = Mathf.Max(currentSpread - spreadSettings.spreadRecoveryRate * Time.deltaTime, spreadSettings.baseSpread);
        }
    }

    public static void DrawWorldRay(Vector3 origin, Vector3 direction, Color color, float duration = 0.02f, float lineWidth = 0.02f)
    {
        DrawWorldLine(origin, origin + direction, color, duration, lineWidth);
    }

    public static void DrawWorldLine(Vector3 start, Vector3 end, Color color, float duration = 0.02f, float lineWidth = 0.02f)
    {
        GameObject lineObj = new GameObject("DebugLine");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        line.startColor = color;
        line.endColor = color;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Destroy(lineObj, duration);
    }

    private void OnGUI()
    {
        if (!debugMode) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        float xPos = 20f;
        float yPos = Screen.height - 60f;
        float width = 300f;
        float height = 40f;

        GUI.Label(new Rect(xPos, yPos, width, height),
                 $"Патроны: {ammoSystem.currentAmmo} / {ammoSystem.reserveAmmo}", style);

        yPos -= 30f;
        GUI.Label(new Rect(xPos, yPos, width, height),
                 $"Разброс: {currentSpread:F2}", style);

        yPos -= 30f;
        GUI.Label(new Rect(xPos, yPos, width, height),
                 $"Режим огня: {fireMode.fireMode}", style);

        yPos -= 30f;
        GUI.Label(new Rect(xPos, yPos, width, height),
                 $"Перезарядка: {(isReloading ? "Да" : "Нет")}", style);

        yPos -= 30f;
        GUI.Label(new Rect(20f, yPos, 300f, 40f),
                $"Последний урон: {lastDamageDealt:F1}", style);

        yPos -= 30f;
        GUI.Label(new Rect(20f, yPos, 300f, 40f),
                $"Дистанция: {lastHitDistance:F1}m", style);

        yPos -= 30f;
        GUI.Label(new Rect(20f, yPos, 300f, 40f),
                $"Объект: {lastHitBodyPart}", style);
    }
}

public interface IDamageable
{
    bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
}
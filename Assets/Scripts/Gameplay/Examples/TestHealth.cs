using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TestHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool HasArmor = false;
    public float armorAmount = 50f;

    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent<float> OnHealthChanged;

    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;
    public float textDisplayTime = 1f;
    public float textRiseSpeed = 1f;
    public Vector3 textOffset = new Vector3(0, 2f, 0);
    public Font textFont;
    public int fontSize = 15;
    public Color textColor = Color.white;

    private Material originalMaterial;
    private Renderer objectRenderer;
    private Coroutine flashCoroutine;
    private List<GameObject> activeDamageTexts = new List<GameObject>();
    private bool isFlashing = false;

    private void Awake()
    {
        objectRenderer = GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
            originalMaterial = new Material(objectRenderer.material);

        if (textFont == null)
            textFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void OnDestroy()
    {
        foreach (var textObj in activeDamageTexts)
        {
            if (textObj != null)
                Destroy(textObj);
        }
        activeDamageTexts.Clear();

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
    }

    public bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (currentHealth <= 0) return false;

        if (HasArmor && armorAmount > 0)
        {
            float armorDamage = damage * 0.5f;
            armorAmount -= armorDamage;
            damage *= 0.5f;
        }

        currentHealth -= damage;
        
        ShowDamageText(damage, hitPoint);
        FlashRed();
        
        OnDamageTaken.Invoke(damage);
        OnHealthChanged.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void ShowDamageText(float damage, Vector3 position)
    {
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.position = position + textOffset;
        textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - Camera.main.transform.position);
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = damage.ToString("F0");
        textMesh.font = textFont;
        textMesh.fontSize = fontSize;
        textMesh.color = textColor;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        activeDamageTexts.Add(textObj);
        StartCoroutine(AnimateDamageText(textObj));
    }

    private IEnumerator AnimateDamageText(GameObject textObj)
    {
        float elapsedTime = 0f;
        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        Color originalColor = textMesh.color;
        Vector3 startPosition = textObj.transform.position;

        while (elapsedTime < textDisplayTime)
        {
            if (textObj == null) yield break;

            float progress = elapsedTime / textDisplayTime;
            textObj.transform.position = startPosition + Vector3.up * textRiseSpeed * progress;
            
            float alpha = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        activeDamageTexts.Remove(textObj);
        Destroy(textObj);
    }

    private void FlashRed()
    {
        if (isFlashing && flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        
        if (objectRenderer != null)
            flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        isFlashing = true;
        Color startColor = objectRenderer.material.color;
        float elapsedTime = 0f;

        while (elapsedTime < flashDuration / 2f)
        {
            objectRenderer.material.color = Color.Lerp(startColor, damageColor, elapsedTime / (flashDuration / 2f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < flashDuration / 2f)
        {
            objectRenderer.material.color = Color.Lerp(damageColor, originalMaterial.color, elapsedTime / (flashDuration / 2f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectRenderer.material.color = originalMaterial.color;
        isFlashing = false;
    }

    private void Die()
    {
        OnDeath.Invoke();
        Destroy(gameObject);
    }
}
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool HasArmor = false;
    public float armorAmount = 50f;

    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken; // Передает количество полученного урона
    public UnityEvent<float> OnHealthChanged; // Передает текущее здоровье (0-1)

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
        OnDamageTaken.Invoke(damage);
        OnHealthChanged.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void Die()
    {
        OnDeath.Invoke();
    }
}
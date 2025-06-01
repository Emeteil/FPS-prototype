using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken; // Передает количество полученного урона
    public UnityEvent<float> OnHealthChanged; // Передает текущее здоровье (0-1)

    public bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (currentHealth <= 0) return false;

        currentHealth -= damage;
        OnDamageTaken.Invoke(damage);
        OnHealthChanged.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
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
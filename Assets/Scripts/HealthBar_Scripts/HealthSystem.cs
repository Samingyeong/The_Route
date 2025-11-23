using UnityEngine;
using System;

namespace StoreGame
{
    /// <summary>
    /// 플레이어의 체력을 관리하는 시스템
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("체력 설정")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        // 이벤트
        public event Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
        public event Action OnDeath;
        public event Action<float> OnDamageTaken; // (damageAmount)
        public event Action<float> OnHealed; // (healAmount)

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public bool IsDead => currentHealth <= 0f;

        void Start()
        {
            // 시작 시 최대 체력으로 설정
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 체력 감소 (데미지)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (IsDead)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float healAmount)
        {
            if (IsDead) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            float actualHeal = currentHealth - previousHealth;

            if (actualHeal > 0f)
            {
                OnHealed?.Invoke(actualHeal);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// 체력을 특정 값으로 설정
        /// </summary>
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 최대 체력 변경
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            if (newMaxHealth <= 0f) return;

            float healthPercentage = HealthPercentage;
            maxHealth = newMaxHealth;
            currentHealth = maxHealth * healthPercentage;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 체력을 최대치로 회복
        /// </summary>
        public void RestoreFullHealth()
        {
            Heal(maxHealth - currentHealth);
        }

        /// <summary>
        /// 테스트용: 데미지 받기 (키보드 H 키)
        /// </summary>
        void Update()
        {
            // 테스트용: H 키로 데미지 받기
            if (Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(10f);
                Debug.Log($"체력: {currentHealth}/{maxHealth}");
            }

            // 테스트용: J 키로 회복
            if (Input.GetKeyDown(KeyCode.J))
            {
                Heal(10f);
                Debug.Log($"체력: {currentHealth}/{maxHealth}");
            }
        }
    }
}


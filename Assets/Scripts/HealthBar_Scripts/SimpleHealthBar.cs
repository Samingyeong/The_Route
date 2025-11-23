using UnityEngine;
using UnityEngine.UI;

namespace StoreGame.UI
{
    /// <summary>
    /// Unity 기본 UI를 사용한 간단한 체력바
    /// </summary>
    public class SimpleHealthBar : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text healthText;

        [Header("체력 시스템")]
        [SerializeField] private HealthSystem healthSystem;

        [Header("설정")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private void Start()
        {
            // HealthSystem 자동 찾기
            if (healthSystem == null)
            {
                healthSystem = FindObjectOfType<HealthSystem>();
            }

            // Slider 자동 찾기
            if (healthSlider == null)
            {
                // 1. 자식에서 찾기
                healthSlider = GetComponentInChildren<Slider>(true);
                
                // 2. 여전히 없으면 직접 자식 중 "Slider" 이름으로 찾기
                if (healthSlider == null)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        if (child.name == "Slider")
                        {
                            healthSlider = child.GetComponent<Slider>();
                            if (healthSlider != null) break;
                        }
                    }
                }
                
                // 3. 마지막 시도: 모든 자식에서 찾기
                if (healthSlider == null)
                {
                    Slider[] sliders = GetComponentsInChildren<Slider>(true);
                    if (sliders != null && sliders.Length > 0)
                    {
                        healthSlider = sliders[0];
                    }
                }
            }

            // Fill Image 자동 찾기
            if (fillImage == null && healthSlider != null)
            {
                if (healthSlider.fillRect != null)
                {
                    fillImage = healthSlider.fillRect.GetComponent<Image>();
                }
            }

            if (healthSystem == null)
            {
                Debug.LogError("[SimpleHealthBar] HealthSystem을 찾을 수 없습니다!", this);
                return;
            }

            if (healthSlider == null)
            {
                Debug.LogError("[SimpleHealthBar] Slider를 찾을 수 없습니다!", this);
                return;
            }

            // 이벤트 구독
            healthSystem.OnHealthChanged += UpdateHealthBar;
            healthSystem.OnDeath += OnPlayerDeath;

            // 초기 설정
            healthSlider.minValue = 0f;
            healthSlider.maxValue = healthSystem.MaxHealth;
            healthSlider.value = healthSystem.CurrentHealth;

            // 초기 체력 표시
            UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);

            // 시작 시 표시/숨김
            gameObject.SetActive(showOnStart);
        }

        private void OnDestroy()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= UpdateHealthBar;
                healthSystem.OnDeath -= OnPlayerDeath;
            }
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthSlider == null) return;

            // Slider 값 업데이트
            healthSlider.value = currentHealth;
            healthSlider.maxValue = maxHealth;

            // 색상 변경 (체력에 따라)
            if (fillImage != null)
            {
                float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
            }

            // 텍스트 업데이트
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
            }
        }

        private void OnPlayerDeath()
        {
            // 사망 시 처리 (필요시)
        }
    }
}


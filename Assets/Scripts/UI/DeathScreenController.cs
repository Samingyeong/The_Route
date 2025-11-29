using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StoreGame;

namespace StoreGame.UI
{
    /// <summary>
    /// 플레이어 사망 시 사망 화면을 표시하고 재시작/메인 메뉴 기능을 제공하는 스크립트
    /// </summary>
    public class DeathScreenController : MonoBehaviour
    {
        [Header("사망 화면 UI")]
        [SerializeField] private GameObject deathScreenPanel; // 사망 화면 패널
        [SerializeField] private CanvasGroup deathScreenCanvasGroup; // 사망 화면 CanvasGroup (페이드 효과용)
        [SerializeField] private Text deathText; // "YOU DIED" 또는 "사망" 텍스트
        [SerializeField] private Button restartButton; // 재시작 버튼
        [SerializeField] private Button mainMenuButton; // 메인 메뉴 버튼

        [Header("플레이어 설정")]
        [SerializeField] private HealthSystem healthSystem; // 플레이어 HealthSystem
        [SerializeField] private MonoBehaviour playerController; // 플레이어 컨트롤러 (사망 시 비활성화)

        [Header("애니메이션 설정")]
        [SerializeField] private float fadeInDuration = 1.5f; // 사망 화면이 나타나는 시간
        [SerializeField] private string deathTextContent = "YOU DIED"; // 사망 텍스트 내용

        [Header("씬 설정")]
        [SerializeField] private string mainMenuSceneName = "MainMenu"; // 메인 메뉴 씬 이름 (없으면 비워두기)
        [SerializeField] private bool restartCurrentScene = true; // 현재 씬 재시작 여부

        [Header("테스트 설정")]
        [SerializeField] private KeyCode testDeathKey = KeyCode.K; // 테스트용 사망 키 (K 키)
        [SerializeField] private bool enableTestKey = true; // 테스트 키 활성화 여부

        private bool isDeathScreenActive = false;

        private void Start()
        {
            // HealthSystem 찾기
            if (healthSystem == null)
            {
                healthSystem = FindObjectOfType<HealthSystem>();
            }

            // HealthSystem의 OnDeath 이벤트 구독
            if (healthSystem != null)
            {
                healthSystem.OnDeath += OnPlayerDeath;
            }
            else
            {
                Debug.LogWarning("[DeathScreenController] HealthSystem을 찾을 수 없습니다.");
            }

            // 초기 상태: 사망 화면 숨김
            if (deathScreenPanel != null)
            {
                deathScreenPanel.SetActive(false);
            }

            if (deathScreenCanvasGroup != null)
            {
                deathScreenCanvasGroup.alpha = 0f;
                deathScreenCanvasGroup.interactable = false;
                deathScreenCanvasGroup.blocksRaycasts = false;
            }

            // 버튼 이벤트 연결
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }

            // 사망 텍스트 설정
            if (deathText != null && !string.IsNullOrEmpty(deathTextContent))
            {
                deathText.text = deathTextContent;
            }
        }

        private void Update()
        {
            // 테스트용 사망 키 (K 키)
            if (enableTestKey && Input.GetKeyDown(testDeathKey))
            {
                if (!isDeathScreenActive && healthSystem != null)
                {
                    Debug.Log($"[DeathScreenController] 테스트 키({testDeathKey})로 사망 처리 시작");
                    // HealthSystem을 통해 사망 처리 (체력을 0으로 만들어서 OnDeath 이벤트 발생)
                    healthSystem.TakeDamage(healthSystem.CurrentHealth);
                }
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (healthSystem != null)
            {
                healthSystem.OnDeath -= OnPlayerDeath;
            }

            // 버튼 이벤트 해제
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
            }
        }

        /// <summary>
        /// 플레이어 사망 시 호출되는 함수
        /// </summary>
        private void OnPlayerDeath()
        {
            if (isDeathScreenActive) return; // 이미 사망 화면이 활성화되어 있으면 중복 실행 방지

            isDeathScreenActive = true;
            Debug.Log("[DeathScreenController] 플레이어 사망 - 사망 화면 표시");

            // 플레이어 컨트롤 비활성화
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // 마우스 커서 잠금 해제
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 사망 화면 표시
            ShowDeathScreen();
        }

        /// <summary>
        /// 사망 화면 표시 (페이드 인)
        /// </summary>
        private void ShowDeathScreen()
        {
            if (deathScreenPanel != null)
            {
                deathScreenPanel.SetActive(true);
            }

            if (deathScreenCanvasGroup != null)
            {
                StartCoroutine(FadeInCoroutine());
            }
            else if (deathScreenPanel != null)
            {
                // CanvasGroup이 없으면 그냥 활성화
                deathScreenPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        private System.Collections.IEnumerator FadeInCoroutine()
        {
            if (deathScreenCanvasGroup == null) yield break;

            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                deathScreenCanvasGroup.alpha = t;
                yield return null;
            }

            deathScreenCanvasGroup.alpha = 1f;
            deathScreenCanvasGroup.interactable = true;
            deathScreenCanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 재시작 버튼 클릭 시 호출
        /// </summary>
        private void OnRestartButtonClicked()
        {
            Debug.Log("[DeathScreenController] 재시작 버튼 클릭됨");

            if (restartCurrentScene)
            {
                // 재시작 플래그 설정 (시네마틱 건너뛰기)
                GameStartSequence.SetRestartingFlag();
                
                // 현재 씬 재시작
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.LogWarning("[DeathScreenController] 재시작 기능이 비활성화되어 있습니다.");
            }
        }

        /// <summary>
        /// 메인 메뉴 버튼 클릭 시 호출
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            Debug.Log("[DeathScreenController] 메인 메뉴 버튼 클릭됨");

            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                Debug.LogWarning("[DeathScreenController] 메인 메뉴 씬 이름이 설정되지 않았습니다.");
            }
        }
    }
}


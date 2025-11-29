using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StoreGame.UI
{
    /// <summary>
    /// 메인 메뉴 화면을 제어하는 스크립트
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("메인 메뉴 UI")]
        [SerializeField] private Button startGameButton; // 게임 시작 버튼
        [SerializeField] private Button quitGameButton; // 게임 종료 버튼 (선택사항)
        [SerializeField] private Button rulesButton; // 룰 설명 버튼

        [Header("룰 설명 UI")]
        [SerializeField] private GameObject rulesPanel; // 룰 설명 패널 (이미지가 들어있는 패널)
        [SerializeField] private Button closeRulesButton; // 룰 설명 닫기 버튼 (선택사항, 패널 안에 있을 수 있음)

        [Header("씬 설정")]
        [SerializeField] private string gameSceneName = "[main]demo_city_night"; // 게임 씬 이름

        private void Start()
        {
            // 마우스 커서 표시
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 룰 설명 패널 초기 상태: 숨김
            if (rulesPanel != null)
            {
                rulesPanel.SetActive(false);
            }

            // 버튼 이벤트 연결
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(OnQuitGameButtonClicked);
            }

            if (rulesButton != null)
            {
                rulesButton.onClick.AddListener(OnRulesButtonClicked);
            }

            if (closeRulesButton != null)
            {
                closeRulesButton.onClick.AddListener(OnCloseRulesButtonClicked);
            }
        }

        private void Update()
        {
            // ESC 키로 룰 설명 패널 닫기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (rulesPanel != null && rulesPanel.activeSelf)
                {
                    HideRulesPanel();
                }
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.RemoveListener(OnQuitGameButtonClicked);
            }

            if (rulesButton != null)
            {
                rulesButton.onClick.RemoveListener(OnRulesButtonClicked);
            }

            if (closeRulesButton != null)
            {
                closeRulesButton.onClick.RemoveListener(OnCloseRulesButtonClicked);
            }
        }

        /// <summary>
        /// 게임 시작 버튼 클릭 시 호출
        /// </summary>
        private void OnStartGameButtonClicked()
        {
            Debug.Log("[MainMenuController] 게임 시작 버튼 클릭됨");

            if (!string.IsNullOrEmpty(gameSceneName))
            {
                // 재시작 플래그 리셋 (첫 게임 시작이므로 시네마틱 재생)
                GameStartSequence.ResetRestartingFlag();

                // 게임 씬으로 이동
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogError("[MainMenuController] 게임 씬 이름이 설정되지 않았습니다!");
            }
        }

        /// <summary>
        /// 게임 종료 버튼 클릭 시 호출
        /// </summary>
        private void OnQuitGameButtonClicked()
        {
            Debug.Log("[MainMenuController] 게임 종료 버튼 클릭됨");

#if UNITY_EDITOR
            // 에디터에서는 플레이 모드 종료
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 빌드에서는 게임 종료
            Application.Quit();
#endif
        }

        /// <summary>
        /// 룰 설명 버튼 클릭 시 호출
        /// </summary>
        private void OnRulesButtonClicked()
        {
            Debug.Log("[MainMenuController] 룰 설명 버튼 클릭됨");
            ShowRulesPanel();
        }

        /// <summary>
        /// 룰 설명 닫기 버튼 클릭 시 호출
        /// </summary>
        private void OnCloseRulesButtonClicked()
        {
            Debug.Log("[MainMenuController] 룰 설명 닫기 버튼 클릭됨");
            HideRulesPanel();
        }

        /// <summary>
        /// 룰 설명 패널 표시
        /// </summary>
        private void ShowRulesPanel()
        {
            if (rulesPanel != null)
            {
                rulesPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 룰 설명 패널 숨김
        /// </summary>
        private void HideRulesPanel()
        {
            if (rulesPanel != null)
            {
                rulesPanel.SetActive(false);
            }
        }
    }
}


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace StoreGame.UI
{
    /// <summary>
    /// 게임 시작 시: 카메라가 이동하며 맵/좀비를 보여주고 -> 플레이어 위치로 이동 ->
    /// 플레이어 시점으로 전환 -> "게임 시작(Start Game)" UI가 등장하는 흐름을 제어하는 스크립트.
    /// 
    /// 씬 구성 가이드:
    /// - CutsceneCamera : 시네마틱용 카메라 (이 스크립트가 자동으로 이동시킴)
    /// - PlayerCamera   : 실제 플레이어 카메라
    /// - FadeImage      : 전체 화면을 덮는 검은색 Image (Canvas 위, alpha 0으로 시작)
    /// - StartGamePanel : "게임 시작" 버튼이 들어있는 패널 (Canvas 안, 처음에는 비활성 또는 alpha 0)
    /// - Waypoints      : 시네마틱 카메라가 이동할 지점들 (빈 GameObject들을 배열로 배치)
    /// 
    /// 이 스크립트를 빈 GameObject(예: GameStartSequenceManager)에 붙이고
    /// 위 객체들을 인스펙터에서 연결해서 사용합니다.
    /// </summary>
    public class GameStartSequence : MonoBehaviour
    {
        [Header("카메라 설정")]
        [SerializeField] private Camera cutsceneCamera;   // 맵 전체를 보여줄 시네마틱 카메라
        [SerializeField] private Camera playerCamera;     // 실제 플레이어 카메라

        [Header("플레이어 설정")]
        [SerializeField] private Transform playerTransform; // 플레이어 위치 (마지막에 카메라가 이동할 목표)
        [SerializeField] private MonoBehaviour playerController; // 플레이어 움직임/조작 스크립트 (시작 시 비활성)

        [Header("시네마틱 경로 설정")]
        [Tooltip("시네마틱 카메라가 이동할 지점들. 빈 GameObject를 만들어서 배치하고 여기에 드래그하세요.")]
        [SerializeField] private Transform[] waypoints; // 카메라가 순회할 지점들
        [SerializeField] private float waypointStayDuration = 2f; // 각 waypoint에서 멈춰서 보여줄 시간
        [SerializeField] private float waypointMoveDuration = 3f; // waypoint 간 이동 시간
        [SerializeField] private float moveToPlayerDuration = 4f; // 마지막에 플레이어 위치로 이동하는 시간

        [Header("페이드 / UI 설정")]
        [SerializeField] private Image fadeImage;         // 전체 화면 검은색 Image (CanvasGroup 대신 Image.color.a 사용)
        [SerializeField] private CanvasGroup startGamePanel; // "게임 시작" UI 패널 (CanvasGroup 권장)

        [Header("시스템 설정")]
        [SerializeField] private StoreGame.HealthSystem healthSystem; // 플레이어 체력 시스템 (시네마틱 동안 비활성화)
        [SerializeField] private GameObject healthBarObject; // 체력바 오브젝트 (시네마틱 동안 숨김)

        [Header("타이밍 설정")]
        [SerializeField] private float fadeDuration = 1.5f;   // 어두워지는 시간
        [SerializeField] private float uiFadeInDuration = 1f; // 게임 시작 UI가 서서히 나타나는 시간

        private bool sequenceStarted = false;
        private bool canStartGame = false;
        private Coroutine cameraSequenceCoroutine;

        // 재시작 여부를 저장하는 static 변수
        private static bool isRestarting = false;

        private void Start()
        {
            // 재시작 중이면 시네마틱 건너뛰고 바로 게임 시작
            if (isRestarting)
            {
                Debug.Log("[GameStartSequence] 재시작 감지 - 시네마틱 건너뛰고 바로 게임 시작");
                SkipToGameStart();
                isRestarting = false; // 플래그 리셋
                return;
            }

            // 안전 장치: 필수 레퍼런스 체크
            if (cutsceneCamera == null)
            {
                Debug.LogWarning("[GameStartSequence] CutsceneCamera가 설정되지 않았습니다.");
                return;
            }

            if (playerCamera == null)
            {
                Debug.LogWarning("[GameStartSequence] PlayerCamera가 설정되지 않았습니다.");
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("[GameStartSequence] PlayerTransform이 설정되지 않았습니다.");
            }

            // 플레이어 컨트롤 비활성화
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // HealthSystem 무적 상태로 설정 (게임 시작 전까지 데미지 받지 않도록)
            if (healthSystem == null && playerTransform != null)
            {
                healthSystem = playerTransform.GetComponent<StoreGame.HealthSystem>();
            }
            if (healthSystem != null)
            {
                healthSystem.SetInvincible(true); // 무적 상태로 설정
                Debug.Log("[GameStartSequence] 플레이어 무적 상태 활성화됨.");
            }

            // 체력바 숨기기
            if (healthBarObject == null)
            {
                // 자동으로 찾기 시도
                GameObject foundHealthBar = GameObject.Find("HealthBar") ?? GameObject.Find("SimpleHealthBar");
                if (foundHealthBar != null)
                {
                    healthBarObject = foundHealthBar;
                }
            }
            if (healthBarObject != null)
            {
                healthBarObject.SetActive(false);
            }

            // 카메라 상태 초기화: 시네마틱 카메라만 켜고, 플레이어 카메라는 끄기
            if (cutsceneCamera != null) cutsceneCamera.enabled = true;
            if (playerCamera != null) playerCamera.enabled = false;

            // 페이드 이미지 초기화 (완전 투명)
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }

            // 시작 UI 패널은 처음에 안 보이게 (alpha 0, interactable X)
            if (startGamePanel != null)
            {
                startGamePanel.alpha = 0f;
                startGamePanel.interactable = false;
                startGamePanel.blocksRaycasts = false;
            }

            // 시네마틱 카메라 초기 위치 설정 (첫 waypoint가 있으면 그곳으로, 없으면 현재 위치 유지)
            if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
            {
                cutsceneCamera.transform.position = waypoints[0].position;
                cutsceneCamera.transform.rotation = waypoints[0].rotation;
            }

            // 시퀀스 시작 (코루틴으로 카메라 이동 시작)
            sequenceStarted = true;
            cameraSequenceCoroutine = StartCoroutine(CameraSequenceCoroutine());
        }

        /// <summary>
        /// 재시작 시 시네마틱을 건너뛰고 바로 게임 시작
        /// </summary>
        private void SkipToGameStart()
        {
            // 카메라 설정: 플레이어 카메라 활성화, 시네마틱 카메라 비활성화
            if (cutsceneCamera != null) cutsceneCamera.enabled = false;
            if (playerCamera != null) playerCamera.enabled = true;

            // 플레이어 컨트롤 활성화
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // HealthSystem 무적 해제
            if (healthSystem == null && playerTransform != null)
            {
                healthSystem = playerTransform.GetComponent<StoreGame.HealthSystem>();
            }
            if (healthSystem != null)
            {
                healthSystem.SetInvincible(false);
            }

            // 체력바 표시
            if (healthBarObject == null)
            {
                GameObject foundHealthBar = GameObject.Find("HealthBar") ?? GameObject.Find("SimpleHealthBar");
                if (foundHealthBar != null)
                {
                    healthBarObject = foundHealthBar;
                }
            }
            if (healthBarObject != null)
            {
                healthBarObject.SetActive(true);
            }

            // 페이드 이미지 투명하게
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }

            // 시작 UI 패널 숨기기
            if (startGamePanel != null)
            {
                startGamePanel.alpha = 0f;
                startGamePanel.interactable = false;
                startGamePanel.blocksRaycasts = false;
            }

            Debug.Log("[GameStartSequence] 재시작 - 시네마틱 건너뛰고 바로 게임 시작됨");
        }

        /// <summary>
        /// 재시작 플래그 설정 (DeathScreenController에서 호출)
        /// </summary>
        public static void SetRestartingFlag()
        {
            isRestarting = true;
        }

        /// <summary>
        /// 재시작 플래그 리셋 (메인 메뉴에서 게임 시작 시 호출)
        /// </summary>
        public static void ResetRestartingFlag()
        {
            isRestarting = false;
        }

        /// <summary>
        /// 카메라가 waypoint들을 순회하고 플레이어 위치로 이동하는 코루틴
        /// </summary>
        private IEnumerator CameraSequenceCoroutine()
        {
            // 1) Waypoint들을 순회
            if (waypoints != null && waypoints.Length > 0)
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] == null) continue;

                    // 현재 waypoint로 이동
                    yield return StartCoroutine(MoveCameraToTarget(
                        cutsceneCamera.transform,
                        waypoints[i].position,
                        waypoints[i].rotation,
                        waypointMoveDuration
                    ));

                    // waypoint에서 잠시 멈춰서 보여줌
                    yield return new WaitForSeconds(waypointStayDuration);
                }
            }

            // 2) 플레이어 위치로 이동
            if (playerTransform != null)
            {
                // 플레이어 위치를 약간 위에서 보도록 (플레이어 머리 위)
                Vector3 targetPosition = playerTransform.position + Vector3.up * 0.5f;
                Quaternion targetRotation = Quaternion.LookRotation((playerTransform.position - targetPosition).normalized);

                yield return StartCoroutine(MoveCameraToTarget(
                    cutsceneCamera.transform,
                    targetPosition,
                    targetRotation,
                    moveToPlayerDuration
                ));
            }

            // 3) 페이드아웃
            yield return StartCoroutine(FadeOutCoroutine(fadeDuration));

            // 4) 플레이어 카메라로 전환
            if (cutsceneCamera != null) cutsceneCamera.enabled = false;
            if (playerCamera != null) playerCamera.enabled = true;

            // 5) 페이드인 (검은 화면에서 서서히 밝아짐)
            yield return StartCoroutine(FadeInCoroutine(fadeDuration));

            // 6) 게임 시작 UI 등장
            yield return StartCoroutine(ShowStartGameUICoroutine(uiFadeInDuration));

            // 시퀀스 완료
            canStartGame = true;
        }

        /// <summary>
        /// 카메라를 목표 위치/회전으로 부드럽게 이동시키는 코루틴
        /// </summary>
        private IEnumerator MoveCameraToTarget(Transform cameraTransform, Vector3 targetPosition, Quaternion targetRotation, float duration)
        {
            Vector3 startPosition = cameraTransform.position;
            Quaternion startRotation = cameraTransform.rotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 부드러운 곡선을 위해 SmoothStep 사용
                t = t * t * (3f - 2f * t);

                cameraTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                yield return null;
            }

            // 정확한 위치로 설정
            cameraTransform.position = targetPosition;
            cameraTransform.rotation = targetRotation;
        }

        /// <summary>
        /// 페이드아웃 (화면이 점점 어두워짐)
        /// </summary>
        private IEnumerator FadeOutCoroutine(float duration)
        {
            if (fadeImage == null) yield break;

            float elapsed = 0f;
            Color c = fadeImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                c.a = t;
                fadeImage.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeImage.color = c;
        }

        /// <summary>
        /// 페이드인 (검은 화면에서 서서히 밝아짐)
        /// </summary>
        private IEnumerator FadeInCoroutine(float duration)
        {
            if (fadeImage == null) yield break;

            float elapsed = 0f;
            Color c = fadeImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                c.a = 1f - t; // 1에서 0으로
                fadeImage.color = c;
                yield return null;
            }

            c.a = 0f;
            fadeImage.color = c;
        }

        /// <summary>
        /// 게임 시작 UI를 서서히 나타나게 하는 코루틴
        /// </summary>
        private IEnumerator ShowStartGameUICoroutine(float duration)
        {
            if (startGamePanel == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                startGamePanel.alpha = t;
                yield return null;
            }

            startGamePanel.alpha = 1f;
            startGamePanel.interactable = true;
            startGamePanel.blocksRaycasts = true;
        }

        /// <summary>
        /// 아무 키나 입력을 감지해서 게임을 시작
        /// </summary>
        private void Update()
        {
            if (canStartGame && Input.anyKeyDown)
            {
                OnClickStartGame();
            }
        }

        /// <summary>
        /// 게임 시작 함수. "Press Any Key" 입력 또는 버튼 클릭 시 호출됨.
        /// </summary>
        public void OnClickStartGame()
        {
            if (!canStartGame)
            {
                Debug.LogWarning("[GameStartSequence] 아직 게임을 시작할 수 없습니다. canStartGame = " + canStartGame);
                return;
            }

            Debug.Log("[GameStartSequence] 게임 시작 입력이 감지되었습니다.");
            
            // HealthSystem을 다시 찾기 (혹시 null일 수 있으니)
            if (healthSystem == null)
            {
                if (playerTransform != null)
                {
                    healthSystem = playerTransform.GetComponent<StoreGame.HealthSystem>();
                }
                if (healthSystem == null)
                {
                    healthSystem = FindObjectOfType<StoreGame.HealthSystem>();
                }
            }

            // 페이드 이미지를 다시 투명하게 만들고,
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }

            // 시작 UI 패널 숨기기
            if (startGamePanel != null)
            {
                startGamePanel.alpha = 0f;
                startGamePanel.interactable = false;
                startGamePanel.blocksRaycasts = false;
            }

            // HealthSystem 무적 해제 (게임 시작 시 데미지 받을 수 있도록)
            if (healthSystem == null && playerTransform != null)
            {
                healthSystem = playerTransform.GetComponent<StoreGame.HealthSystem>();
            }
            
            if (healthSystem == null)
            {
                Debug.LogError("[GameStartSequence] HealthSystem을 찾을 수 없습니다! 무적 해제 실패.");
            }
            else
            {
                healthSystem.SetInvincible(false); // 무적 상태 해제
                Debug.Log($"[GameStartSequence] 플레이어 무적 상태 해제됨. 현재 무적 상태: {healthSystem.IsInvincible}");
                
                // 한 번 더 확인
                if (healthSystem.IsInvincible)
                {
                    Debug.LogError("[GameStartSequence] 경고: 무적 해제가 실패했습니다!");
                }
            }

            // 체력바 다시 보이기
            if (healthBarObject == null)
            {
                // 자동으로 찾기 시도
                GameObject foundHealthBar = GameObject.Find("HealthBar") ?? GameObject.Find("SimpleHealthBar");
                if (foundHealthBar != null)
                {
                    healthBarObject = foundHealthBar;
                }
            }
            if (healthBarObject != null)
            {
                healthBarObject.SetActive(true);
                Debug.Log("[GameStartSequence] 체력바 활성화됨.");
            }

            // 플레이어 조작 활성화
            if (playerController != null)
            {
                playerController.enabled = true;
                Debug.Log("[GameStartSequence] 플레이어 컨트롤러 활성화됨.");
            }
            else
            {
                Debug.LogWarning("[GameStartSequence] PlayerController가 설정되지 않았습니다!");
            }

            sequenceStarted = false;
            canStartGame = false;
            Debug.Log("[GameStartSequence] 게임 시작 완료 - 플레이어 조작이 활성화되었습니다.");
        }
    }
}



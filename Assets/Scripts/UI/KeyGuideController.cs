using UnityEngine;
using UnityEngine.UI;

namespace StoreGame.UI
{
    /// <summary>
    /// Tab 키를 누르고 있는 동안 키 가이드 UI를 표시하는 스크립트
    /// </summary>
    public class KeyGuideController : MonoBehaviour
    {
        [Header("키 가이드 UI 설정")]
        [SerializeField] private GameObject keyGuidePanel; // 키 가이드 패널 (Canvas 안의 Panel 또는 GameObject)
        [SerializeField] private CanvasGroup keyGuideCanvasGroup; // 키 가이드 CanvasGroup (선택사항, 페이드 효과용)

        [Header("입력 설정")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab; // 키 가이드 토글 키 (기본: Tab)

        [Header("애니메이션 설정")]
        [SerializeField] private bool useFadeAnimation = true; // 페이드 애니메이션 사용 여부
        [SerializeField] private float fadeDuration = 0.2f; // 페이드 인/아웃 시간

        private bool isKeyGuideVisible = false;
        private Coroutine fadeCoroutine;

        private void Start()
        {
            // 초기 상태: 키 가이드 숨김
            if (keyGuidePanel != null)
            {
                keyGuidePanel.SetActive(false);
            }

            if (keyGuideCanvasGroup != null)
            {
                keyGuideCanvasGroup.alpha = 0f;
                keyGuideCanvasGroup.interactable = false;
                keyGuideCanvasGroup.blocksRaycasts = false;
            }
        }

        private void Update()
        {
            // Tab 키 입력 감지
            if (Input.GetKey(toggleKey))
            {
                // 키를 누르고 있는 동안
                if (!isKeyGuideVisible)
                {
                    ShowKeyGuide();
                }
            }
            else
            {
                // 키를 떼면
                if (isKeyGuideVisible)
                {
                    HideKeyGuide();
                }
            }
        }

        /// <summary>
        /// 키 가이드 표시
        /// </summary>
        private void ShowKeyGuide()
        {
            isKeyGuideVisible = true;

            if (keyGuidePanel != null)
            {
                keyGuidePanel.SetActive(true);
            }

            if (useFadeAnimation && keyGuideCanvasGroup != null)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeInCoroutine());
            }
            else if (keyGuideCanvasGroup != null)
            {
                keyGuideCanvasGroup.alpha = 1f;
                keyGuideCanvasGroup.interactable = true;
                keyGuideCanvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// 키 가이드 숨김
        /// </summary>
        private void HideKeyGuide()
        {
            isKeyGuideVisible = false;

            if (useFadeAnimation && keyGuideCanvasGroup != null)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeOutCoroutine());
            }
            else
            {
                if (keyGuidePanel != null)
                {
                    keyGuidePanel.SetActive(false);
                }

                if (keyGuideCanvasGroup != null)
                {
                    keyGuideCanvasGroup.alpha = 0f;
                    keyGuideCanvasGroup.interactable = false;
                    keyGuideCanvasGroup.blocksRaycasts = false;
                }
            }
        }

        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        private System.Collections.IEnumerator FadeInCoroutine()
        {
            if (keyGuideCanvasGroup == null) yield break;

            float elapsed = 0f;
            float startAlpha = keyGuideCanvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                keyGuideCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
                yield return null;
            }

            keyGuideCanvasGroup.alpha = 1f;
            keyGuideCanvasGroup.interactable = true;
            keyGuideCanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 페이드 아웃 애니메이션
        /// </summary>
        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            if (keyGuideCanvasGroup == null)
            {
                if (keyGuidePanel != null)
                {
                    keyGuidePanel.SetActive(false);
                }
                yield break;
            }

            float elapsed = 0f;
            float startAlpha = keyGuideCanvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                keyGuideCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            keyGuideCanvasGroup.alpha = 0f;
            keyGuideCanvasGroup.interactable = false;
            keyGuideCanvasGroup.blocksRaycasts = false;

            if (keyGuidePanel != null)
            {
                keyGuidePanel.SetActive(false);
            }
        }
    }
}


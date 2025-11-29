using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요

public class TutorialUI : MonoBehaviour
{
    [Header("기본 UI 요소")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI stepNumberText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Image checkmarkImage;
    
    [Header("완료 화면 UI")]
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TextMeshProUGUI completeText;
    [SerializeField] private Image fadeOverlay; // 화면 전체를 덮는 검은 이미지
    [SerializeField] private GameObject buttonPanel; // 버튼들을 담을 부모 오브젝트
    
    [Header("버튼 설정")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button startButton;
    [SerializeField] private string homeSceneName = "HomeScene"; // 홈 씬 이름
    [SerializeField] private string mainSceneName = "[main]demo_city_night"; // 메인 게임 씬 이름
    
    [Header("애니메이션 설정")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float screenFadeDuration = 1.0f; // 암전 시간
    
    private CanvasGroup panelCanvasGroup;
    
    void Start()
    {
        if (tutorialPanel != null)
        {
            panelCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // 시작할 때 체크마크와 완료 패널은 숨깁니다.
        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(false);
        }
        
        if (completePanel != null)
        {
            completePanel.SetActive(false);
        }
        
        if (buttonPanel != null)
        {
            buttonPanel.SetActive(false);
        }
        
        // 암전용 이미지 초기화 (투명하게, 꺼두기)
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.gameObject.SetActive(false);
        }
        
        // 버튼 리스너 연결
        if (homeButton != null) homeButton.onClick.AddListener(OnHomeButtonClicked);
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
        
        ShowTutorialPanel();
    }
    
    public void UpdateStep(int stepNumber, string instruction)
    {
        if (stepNumberText != null)
        {
            stepNumberText.text = $"Step {stepNumber}/9";
        }
        
        if (instructionText != null)
        {
            instructionText.text = instruction;
        }
        
        // [중요] 새로운 단계가 시작되면 체크마크를 끕니다.
        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(false);
        }
        
        // 튜토리얼 패널이 꺼져있다면 켭니다.
        ShowTutorialPanel();
    }
    
    // [중요] 이 함수가 있어야 합니다!
    public void ShowStepComplete()
    {
        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(true);
            Debug.Log("[UI] Checkmark ON");
        }
    }
    
    public void ShowTutorialComplete()
    {
        // 튜토리얼 진행 패널은 숨깁니다.
        if (tutorialPanel != null)
        {
             tutorialPanel.SetActive(false);
        }

        // 완료 패널을 켭니다.
        if (completePanel != null)
        {
            completePanel.SetActive(true);
            Debug.Log("[UI] Complete Panel ON");
        }
        
        if (completeText != null)
        {
            completeText.text = "Tutorial Complete!";
        }
        
        // 암전 및 버튼 표시 코루틴 시작
        StartCoroutine(ShowEndScreenSequence());
    }
    
    IEnumerator ShowEndScreenSequence()
    {
        // 2초 동안 완료 텍스트 보여주기
        yield return new WaitForSeconds(2.0f);
        
        // 완료 패널 끄기
        if (completePanel != null) completePanel.SetActive(false);
        
        // 화면 암전 시작
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < screenFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / screenFadeDuration);
                fadeOverlay.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadeOverlay.color = Color.black;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // 버튼 패널 표시
        if (buttonPanel != null)
        {
            buttonPanel.SetActive(true);
            // 커서가 보여야 누를 수 있음
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void OnHomeButtonClicked()
    {
        // 홈 씬으로 이동 (씬이 없으면 로그만)
        if (Application.CanStreamedLevelBeLoaded(homeSceneName))
            SceneManager.LoadScene(homeSceneName);
        else
            Debug.Log($"[UI] Home Scene '{homeSceneName}' not found!");
    }
    
    void OnStartButtonClicked()
    {
        // 메인 게임 씬으로 이동
        if (Application.CanStreamedLevelBeLoaded(mainSceneName))
            SceneManager.LoadScene(mainSceneName);
        else
            Debug.Log($"[UI] Main Scene '{mainSceneName}' not found!");
    }
    
    void ShowTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            if (panelCanvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }
    }
    
    void HideTutorialPanel()
    {
        if (panelCanvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        if (panelCanvasGroup == null) yield break;
        
        panelCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 1f;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        if (panelCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        float startAlpha = panelCanvasGroup.alpha;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 0f;
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}

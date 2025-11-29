using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI stepNumberText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Image checkmarkImage;
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TextMeshProUGUI completeText;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float fadeDuration = 0.3f;
    
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
        
        // 3초 후 완료 패널도 숨깁니다.
        Invoke(nameof(HideCompletePanel), 3f);
    }

    void HideCompletePanel()
    {
        if (completePanel != null)
        {
            completePanel.SetActive(false);
        }
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
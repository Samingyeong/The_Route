using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요

public class EndingTrigger : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("Video Player 컴포넌트를 연결하세요.")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("영상이 재생될 Raw Image 오브젝트를 연결하세요.")]
    public GameObject videoUI;

    [Header("Ending UI Settings")]
    [Tooltip("영상이 끝난 후 띄울 패널(검은 배경 + 버튼들)을 연결하세요.")]
    public GameObject endingControlPanel;

    [Tooltip("이동할 메인 메뉴 씬의 이름을 적으세요.")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("이동할 다음 스테이지 씬의 이름을 적으세요.")]
    public string nextStageSceneName = "Stage2";

    [Header("Trigger Settings")]
    [Tooltip("엔딩을 발동시킬 태그 목록")]
    public string[] targetTags = { "Player", "Car" };

    private bool hasTriggered = false;

    private void Start()
    {
        // 1. 게임 시작 시 상태 초기화
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        // 2. 영상 종료 이벤트 연결
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        // 3. 시작할 때 엔딩 패널은 숨김
        if (endingControlPanel != null)
        {
            endingControlPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                PlayEnding();
                break;
            }
        }
    }

    private void PlayEnding()
    {
        hasTriggered = true;

        // 1. 게임 시간 멈춤 (이동 정지)
        Time.timeScale = 0f;

        // 2. 플레이어 조작 및 무기만 비활성화 (카메라는 유지)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // (A) 플레이어 컨트롤러 끄기 (움직임 방지)
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
            
            // (B) HeadBob 끄기
            HeadBob headBob = player.GetComponent<HeadBob>();
            if (headBob != null) headBob.enabled = false;

            // (C) 무기 끄기 (Inventory System 사용하는 경우)
            // DevionGames 인벤토리를 사용 중이므로, 무기를 비활성화하는 가장 확실한 방법은
            // 'WeaponHolder' 같은 부모를 찾거나, 현재 들고 있는 무기를 찾아야 함.
            // 여기서는 단순히 자식 중 'Weapon'이 들어간 오브젝트를 찾거나
            // 'WeaponHolder'라는 이름의 오브젝트를 찾아 비활성화 시도
            Transform weaponHolder = player.transform.Find("WeaponHolder"); 
            if (weaponHolder == null)
            {
                // 없으면 'Main Camera' 밑에 있을 수도 있음 (FPS)
                Transform camTransform = player.transform.Find("Main Camera");
                if (camTransform != null)
                {
                    weaponHolder = camTransform.Find("WeaponHolder");
                }
            }
            
            if (weaponHolder != null) 
            {
                weaponHolder.gameObject.SetActive(false);
            }
            else 
            {
                 // 그래도 없으면 'Gun'이라는 이름이 포함된 자식들을 찾아본다 (단순 무식하지만 효과적)
                 Transform[] allChildren = player.GetComponentsInChildren<Transform>();
                 foreach (Transform t in allChildren)
                 {
                     if (t.name.Contains("Gun") || t.name.Contains("Weapon") || t.name.Contains("Rifle"))
                     {
                         t.gameObject.SetActive(false);
                     }
                 }
            }
        }

        // 3. 좀비 비활성화 (스크립트 기준)
        ShootZombie[] zombies = FindObjectsOfType<ShootZombie>();
        foreach (ShootZombie zombie in zombies)
        {
            if (zombie != null)
            {
                zombie.gameObject.SetActive(false); // 좀비 오브젝트 비활성화
            }
        }

        // 3. 다른 모든 소리 끄기
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudio)
        {
            audio.Stop();
        }

        // 3. 영상 UI 켜기
        if (videoUI != null)
        {
            videoUI.SetActive(true);
        }

        // 4. 비디오 재생
        if (videoPlayer != null)
        {
            videoPlayer.timeReference = VideoTimeReference.Freerun; // 시간 멈춤 무시하고 재생
            videoPlayer.Play();
        }

        Debug.Log("Ending Video Started!");
    }

    // 영상 재생이 끝났을 때 호출되는 함수
    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video Ended. Showing Buttons.");

        // 1. 영상 UI 끄기 (선택 사항: 마지막 장면 보여주고 싶으면 주석 처리)
        if (videoUI != null)
        {
            videoUI.SetActive(false); 
        }

        // 2. 버튼 패널(검은 배경 포함) 켜기
        if (endingControlPanel != null)
        {
            endingControlPanel.SetActive(true);
        }

        // 3. 마우스 커서 보이기 및 잠금 해제
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // [버튼 연결용] 메인 메뉴로 이동
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // 시간 다시 흐르게 설정
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // [버튼 연결용] 다음 스테이지로 이동
    public void LoadNextStage()
    {
        Time.timeScale = 1f; // 시간 다시 흐르게 설정
        SceneManager.LoadScene(nextStageSceneName);
    }
}

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class EndingTrigger : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("Assign the Video Player component here")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("Assign the UI Panel/Canvas containing the RawImage here")]
    public GameObject videoUI;

    [Header("Trigger Settings")]
    [Tooltip("Tags that trigger the ending")]
    public string[] targetTags = { "Player", "Car" };

    private bool hasTriggered = false;

    private void Start()
    {
        // 게임 시작 시 시간과 소리가 정상인지 확인 (재시작 시 멈춤 방지)
        Time.timeScale = 1f;
        AudioListener.pause = false; 
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

        // 1. 게임 시간 멈춤 (플레이어, 좀비, 차 이동 정지)
        Time.timeScale = 0f;

        // 2. 기존 게임 소리 끄기 (배경음, 엔진소리 등)
        // 방법 A: AudioListener를 일시정지 (단, 비디오 소리도 꺼질 수 있으므로 주의)
        // 방법 B: 현재 재생 중인 모든 AudioSource를 찾아 정지시킴
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudio)
        {
            // 비디오 플레이어가 사용할 오디오 소스는 끄면 안 되지만, 
            // 보통 VideoPlayer.Play()가 나중에 호출되므로 일단 다 끕니다.
            audio.Stop();
        }

        // 3. 비디오 UI 켜기
        if (videoUI != null)
        {
            videoUI.SetActive(true);
        }

        // 4. 비디오 재생
        if (videoPlayer != null)
        {
            // 시간이 멈춰도 비디오는 재생되도록 설정
            videoPlayer.timeReference = VideoTimeReference.Freerun;
            
            // 비디오 재생
            videoPlayer.Play();
        }

        Debug.Log("Ending Video Triggered!");
    }
}


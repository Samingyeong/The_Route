using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ZombieAudio : MonoBehaviour
{
    [Header("=== 오디오 클립 설정 (각 3개씩 넣으세요) ===")]
    public AudioClip[] idleClips;   // 평소 울음소리
    public AudioClip[] attackClips; // 공격할 때
    public AudioClip[] hurtClips;   // 맞았을 때
    public AudioClip[] agonyClips;  // 죽을 때 (혹은 기어갈 때)

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // 3D 사운드 설정 (거리에 따라 소리 작아지게)
        audioSource.spatialBlend = 1.0f; 
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 20f; // 20미터 밖에서는 안 들림
    }

    // 랜덤 재생 도우미 함수
    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        
        // 피치(음정)를 살짝 랜덤하게 조절하면 더 자연스러움 (0.9 ~ 1.1)
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        
        int index = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[index]);
    }

    public void PlayIdle()
    {
        // 이미 소리가 나고 있다면 Idle은 굳이 겹쳐서 재생하지 않음 (선택사항)
        if (!audioSource.isPlaying) 
        {
            PlayRandomClip(idleClips);
        }
    }

    public void PlayAttack()
    {
        PlayRandomClip(attackClips);
    }

    public void PlayHurt()
    {
        // 맞은 소리는 중요하므로 즉시 재생
        PlayRandomClip(hurtClips);
    }

    public void PlayAgony() // 죽을 때 사용
    {
        PlayRandomClip(agonyClips);
    }
}
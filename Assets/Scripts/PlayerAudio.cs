using UnityEngine;

namespace StoreGame
{
    /// <summary>
    /// 플레이어의 상태 변화에 따른 소리를 관리하는 클래스
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private AudioSource audioSource;

        [Header("Audio Clips")]
        [Tooltip("다칠 때 재생할 소리들을 이곳에 넣으세요 (3개)")]
        [SerializeField] private AudioClip[] damageClips;

        private void Awake()
        {
            // 같은 오브젝트에 컴포넌트들이 있다면 자동으로 찾아서 할당
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            // HealthSystem의 데미지 이벤트 구독
            if (healthSystem != null)
            {
                healthSystem.OnDamageTaken += PlayRandomDamageSound;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제 (필수: 메모리 누수 방지)
            if (healthSystem != null)
            {
                healthSystem.OnDamageTaken -= PlayRandomDamageSound;
            }
        }

        /// <summary>
        /// 데미지를 입었을 때 호출되는 함수
        /// </summary>
        /// <param name="damageAmount">받은 데미지 양 (이벤트 형식 맞춤)</param>
        private void PlayRandomDamageSound(float damageAmount)
        {
            // 클립이 없으면 실행 안 함
            if (damageClips == null || damageClips.Length == 0) return;

            // 랜덤하게 하나 선택 (0 ~ 배열 길이-1)
            int index = Random.Range(0, damageClips.Length);
            
            // 소리가 너무 기계적으로 들리지 않게 피치(음높이)를 약간 랜덤하게 조절 (선택사항)
            audioSource.pitch = Random.Range(0.9f, 1.1f); 

            // 소리 재생 (PlayOneShot은 소리가 겹쳐도 끊기지 않음)
            audioSource.PlayOneShot(damageClips[index]);
        }
    }
}
using DevionGames.InventorySystem;
using StoreGame;
using UnityEngine;

namespace StoreGame.Items
{
    /// <summary>
    /// C키를 눌러서 인벤토리의 붕대를 빠르게 사용하는 스크립트
    /// </summary>
    [RequireComponent(typeof(AudioSource))] // AudioSource 컴포넌트가 없으면 자동으로 추가
    public class BandageQuickUse : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private float healAmount = 30f;
        [SerializeField] private string windowName = "Inventory";
        [SerializeField] private string bandageItemName = "Bandage"; // 붕대 아이템 이름

        [Header("오디오 설정")]
        [SerializeField] private AudioClip bandageSound; // 붕대 감는 소리 파일 (Inspector에서 할당)

        private HealthSystem healthSystem;
        private AudioSource audioSource;

        void Start()
        {
            // HealthSystem 찾기
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = FindObjectOfType<HealthSystem>();
            }

            if (healthSystem == null)
            {
                Debug.LogWarning("[BandageQuickUse] HealthSystem을 찾을 수 없습니다!");
            }

            // AudioSource 가져오기
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            // C키 입력 감지
            if (Input.GetKeyDown(KeyCode.C))
            {
                TryUseBandage();
            }
        }

        private void TryUseBandage()
        {
            if (healthSystem == null)
            {
                Debug.LogWarning("[BandageQuickUse] HealthSystem이 없습니다!");
                return;
            }

            // 이미 최대 체력이면 사용 불가
            if (healthSystem.CurrentHealth >= healthSystem.MaxHealth)
            {
                Debug.Log("[BandageQuickUse] 이미 최대 체력입니다.");
                return;
            }

            // 인벤토리에서 붕대 찾기
            Item bandageItem = ItemContainer.GetItem(windowName, bandageItemName);
            
            // 이름으로 못 찾으면 "붕대"로 시도
            if (bandageItem == null)
            {
                bandageItem = ItemContainer.GetItem(windowName, "붕대");
            }

            if (bandageItem == null)
            {
                Debug.Log("[BandageQuickUse] 인벤토리에 붕대가 없습니다.");
                return;
            }

            // 체력 회복
            healthSystem.Heal(healAmount);
            Debug.Log($"[BandageQuickUse] {healAmount}만큼 체력을 회복했습니다. 현재 체력: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");

            // --- 소리 재생 코드 추가 ---
            if (bandageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(bandageSound); // 중첩되어도 끊기지 않게 PlayOneShot 사용
            }
            // -----------------------

            // 인벤토리에서 붕대 1개 제거
            if (ItemContainer.RemoveItem(windowName, bandageItem, 1))
            {
                Debug.Log("[BandageQuickUse] 붕대를 사용했습니다.");
            }
            else
            {
                Debug.LogWarning("[BandageQuickUse] 붕대를 제거하지 못했습니다.");
            }
        }
    }
}
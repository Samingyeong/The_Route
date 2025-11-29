using DevionGames.InventorySystem;
using DevionGames.InventorySystem.ItemActions;
using DevionGames;
using StoreGame;
using UnityEngine;

namespace StoreGame.Items
{
    /// <summary>
    /// 붕대를 사용하여 체력을 회복하는 액션
    /// </summary>
    [DevionGames.IconAttribute("Item")]
    [ComponentMenu("Inventory System/Heal With Bandage")]
    [System.Serializable]
    public class BandageUseAction : ItemAction
    {
        [SerializeField]
        private float healAmount = 30f; // 회복량

        [SerializeField]
        private string windowName = "Inventory";

        public override void OnStart()
        {
            Debug.Log("[BandageUseAction] OnStart 호출됨");
        }

        public override ActionStatus OnUpdate()
        {
            Debug.Log("[BandageUseAction] OnUpdate 호출됨");
            
            if (item == null)
            {
                Debug.LogWarning("[BandageUseAction] Item이 설정되지 않았습니다.");
                return ActionStatus.Failure;
            }

            Debug.Log($"[BandageUseAction] Item: {item.DisplayName}");

            // HealthSystem 찾기
            HealthSystem healthSystem = Object.FindObjectOfType<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogWarning("[BandageUseAction] HealthSystem을 찾을 수 없습니다.");
                return ActionStatus.Failure;
            }

            Debug.Log($"[BandageUseAction] HealthSystem 찾음. 현재 체력: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");

            // 이미 최대 체력이면 사용 불가
            if (healthSystem.CurrentHealth >= healthSystem.MaxHealth)
            {
                Debug.Log("[BandageUseAction] 이미 최대 체력입니다.");
                return ActionStatus.Failure;
            }

            // 체력 회복
            healthSystem.Heal(healAmount);
            Debug.Log($"[BandageUseAction] {healAmount}만큼 체력을 회복했습니다. 현재 체력: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");

            // 인벤토리에서 아이템 1개 제거
            bool removed = ItemContainer.RemoveItem(windowName, item, 1);
            Debug.Log($"[BandageUseAction] RemoveItem 결과: {removed}, Window: {windowName}, Item: {item.DisplayName}");
            
            if (removed)
            {
                Debug.Log("[BandageUseAction] 성공!");
                return ActionStatus.Success;
            }

            Debug.LogWarning("[BandageUseAction] 인벤토리에서 아이템을 제거하지 못했습니다.");
            return ActionStatus.Failure;
        }
    }
}


using DevionGames.InventorySystem;
using StoreGame.Data;
using UnityEngine;

namespace StoreGame.Integration
{
    /// <summary>
    /// Devion Games Inventory System과 기존 파밍 로직 사이를 이어주는 헬퍼.
    /// </summary>
    public static class DevionInventoryBridge
    {
        private const string DefaultWindowName = "Inventory";

        public static bool TryAddItem(ItemData itemData, int amount, string windowName = DefaultWindowName)
        {
            if (itemData == null || amount <= 0)
            {
                return false;
            }

            var template = itemData.DevionItemTemplate;
            if (template == null)
            {
                UnityEngine.Debug.LogWarning($"Devion Item 템플릿이 비어 있어 지급할 수 없습니다: {itemData.DisplayName}");
                return false;
            }

            if (!EnsureInventoryManagerReady())
            {
                return false;
            }

            var remaining = amount;
            while (remaining > 0)
            {
                var clone = Object.Instantiate(template);
                clone.Stack = Mathf.Min(clone.MaxStack, remaining);
                OverrideVisualFromItemData(clone, itemData);
                UnityEngine.Debug.Log($"[DevionInventoryBridge] {itemData.DisplayName} icon={(clone.Icon != null ? clone.Icon.name : "null")}");

                if (!ItemContainer.AddItem(windowName, clone))
                {
                    UnityEngine.Debug.LogWarning($"Devion Inventory Window \"{windowName}\"에 {clone.DisplayName} 추가 실패");
                    return false;
                }

                remaining -= clone.Stack;
            }

            return true;
        }

        private static void OverrideVisualFromItemData(Item templateClone, ItemData source)
        {
            if (source == null || templateClone == null)
            {
                return;
            }

            if (source.Icon != null)
            {
                templateClone.Icon = source.Icon;
            }

            if (!string.IsNullOrEmpty(source.DisplayName))
            {
                templateClone.DisplayName = source.DisplayName;
            }
        }

        private static bool EnsureInventoryManagerReady()
        {
            if (InventoryManager.current != null)
            {
                return true;
            }

            UnityEngine.Debug.LogWarning("Devion InventoryManager 인스턴스를 찾지 못했습니다. 씬에 Inventory Manager를 추가해 주세요.");
            return false;
        }
    }
}



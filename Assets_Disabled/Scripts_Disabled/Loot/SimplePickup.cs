using StoreGame.Data;
using StoreGame.Integration;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StoreGame.Loot
{
    /// <summary>
    /// 간단한 상호작용 픽업: 플레이어가 범위 안에서 F 키를 누르면 아이템을 Devion 인벤토리에 추가합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SimplePickup : MonoBehaviour
    {
        [Header("Pickup Data")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private string devionWindowName = "Inventory";

        [Header("Player Detection")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private CanvasGroup promptCanvas;

        private bool _playerInRange;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            UnityEngine.Debug.Log($"[SimplePickup] OnTriggerEnter: {other.name}, Tag: {other.tag}, Expected: {playerTag}");
            if (other.CompareTag(playerTag))
            {
                _playerInRange = true;
                SetPromptVisible(true);
                UnityEngine.Debug.Log($"[SimplePickup] Player entered range!");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = false;
                SetPromptVisible(false);
            }
        }

        private void Update()
        {
            if (!_playerInRange)
            {
                return;
            }

            if (itemData == null)
            {
                UnityEngine.Debug.LogWarning($"[SimplePickup] ItemData is not assigned on {gameObject.name}!");
                return;
            }

            if (amount <= 0)
            {
                UnityEngine.Debug.LogWarning($"[SimplePickup] Amount is 0 or negative on {gameObject.name}!");
                return;
            }

            if (WasPickupPressed())
            {
                UnityEngine.Debug.Log($"[SimplePickup] F key pressed! Attempting to add {itemData.DisplayName} x{amount}");
                if (DevionInventoryBridge.TryAddItem(itemData, amount, devionWindowName))
                {
                    UnityEngine.Debug.Log($"[SimplePickup] Successfully added item to inventory!");
                    SetPromptVisible(false);
                    Destroy(gameObject);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[SimplePickup] Failed to add item to inventory!");
                }
            }
        }

        private static bool WasPickupPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                UnityEngine.Debug.LogWarning("[SimplePickup] Keyboard.current is null! Make sure Input System is properly initialized.");
                return false;
            }
            return Keyboard.current.fKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F);
#endif
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptCanvas == null)
            {
                return;
            }

            promptCanvas.alpha = visible ? 1f : 0f;
            promptCanvas.blocksRaycasts = visible;
            promptCanvas.interactable = visible;
        }
    }
}



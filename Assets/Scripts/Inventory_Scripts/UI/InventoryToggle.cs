using UnityEngine;
using System.Collections;
using DevionGames.InventorySystem;
using DevionGames.UIWidgets;

namespace StoreGame.UI
{
    /// <summary>
    /// 'i' 키를 눌러 인벤토리를 열고 닫는 스크립트
    /// </summary>
    public class InventoryToggle : MonoBehaviour
    {
        [Header("인벤토리 설정")]
        [SerializeField] private string inventoryWindowName = "Inventory";
        
        [Header("입력 설정")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        
        [Header("위치 설정")]
        [SerializeField] private bool centerOnScreen = true;
        [SerializeField] private float centerDelay = 0.1f; // Show() 애니메이션 후 위치 설정 지연 시간
        
        private ItemContainer inventoryContainer;
        private bool isInitialized = false;
        private Coroutine centerCoroutine;

        private void Start()
        {
            InitializeInventory();
        }

        private void InitializeInventory()
        {
            if (isInitialized) return;
            
            // 인벤토리 창 찾기
            inventoryContainer = WidgetUtility.Find<ItemContainer>(inventoryWindowName);
            
            if (inventoryContainer == null)
            {
                Debug.LogWarning($"[InventoryToggle] '{inventoryWindowName}' 창을 찾을 수 없습니다. 씬에 인벤토리 UI가 있는지 확인하세요.");
                return;
            }
            
            isInitialized = true;
            Debug.Log($"[InventoryToggle] 인벤토리 창 '{inventoryWindowName}' 초기화 완료. '{toggleKey}' 키를 눌러 열고 닫을 수 있습니다.");
        }

        private void Update()
        {
            // 'i' 키 입력 확인
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInventory();
            }
        }
        
        private void LateUpdate()
        {
            // 인벤토리가 열려있을 때 LateUpdate에서 중앙에 위치시키기 (모든 업데이트 후)
            if (centerOnScreen && inventoryContainer != null && inventoryContainer.IsVisible)
            {
                CenterInventoryWindow();
            }
        }

        /// <summary>
        /// 인벤토리를 열거나 닫습니다
        /// </summary>
        public void ToggleInventory()
        {
            if (!isInitialized)
            {
                InitializeInventory();
            }
            
            if (inventoryContainer == null)
            {
                Debug.LogWarning($"[InventoryToggle] 인벤토리 창을 찾을 수 없습니다.");
                return;
            }
            
            // 인벤토리가 열려있으면 닫고, 닫혀있으면 엽니다
            if (inventoryContainer.IsVisible)
            {
                inventoryContainer.Close();
                
                // 코루틴 중지
                if (centerCoroutine != null)
                {
                    StopCoroutine(centerCoroutine);
                    centerCoroutine = null;
                }
            }
            else
            {
                // Show() 전에 미리 위치 설정
                if (centerOnScreen)
                {
                    CenterInventoryWindow();
                }
                
                inventoryContainer.Show();
                
                // Show() 직후 즉시 위치 설정
                if (centerOnScreen)
                {
                    CenterInventoryWindow();
                    if (centerCoroutine != null)
                    {
                        StopCoroutine(centerCoroutine);
                    }
                    centerCoroutine = StartCoroutine(CenterInventoryWindowDelayed());
                }
            }
        }
        
        /// <summary>
        /// 인벤토리 창을 화면 중앙에 위치시킵니다 (지연 후)
        /// </summary>
        private IEnumerator CenterInventoryWindowDelayed()
        {
            // Show() 애니메이션이 완료될 때까지 대기
            yield return new WaitForSeconds(centerDelay);
            
            // 여러 프레임에 걸쳐 위치를 확실하게 설정
            for (int i = 0; i < 3; i++)
            {
                CenterInventoryWindow();
                yield return null; // 다음 프레임까지 대기
            }
        }
        
        /// <summary>
        /// 인벤토리 창을 화면 중앙에 위치시킵니다
        /// </summary>
        private void CenterInventoryWindow()
        {
            if (inventoryContainer == null) return;
            
            RectTransform rectTransform = inventoryContainer.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // 부모 Canvas 찾기
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null) return;
            
            // Canvas의 크기 가져오기
            Vector2 canvasSize = canvasRect.sizeDelta;
            if (canvasSize.x == 0 || canvasSize.y == 0)
            {
                // Canvas 크기가 0이면 Screen 크기 사용
                canvasSize = new Vector2(Screen.width, Screen.height);
            }
            
            // Pivot을 중앙으로 설정 (반드시 먼저 설정)
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Anchors를 중앙으로 설정
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            
            // 화면 중앙에 위치 (anchoredPosition을 0,0으로 설정)
            rectTransform.anchoredPosition = Vector2.zero;
            
            // localPosition도 0으로 설정
            rectTransform.localPosition = Vector3.zero;
            
            // worldPosition도 확인 (혹시 모를 경우 대비)
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Screen Space Overlay인 경우 화면 중앙 좌표로 설정
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                rectTransform.position = screenCenter;
            }
            
            // Force update
            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using DevionGames.UIWidgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DevionGames.InventorySystem
{
    public class Slot : CallbackHandler
    {
        /// <summary>
        /// The text to display item name.
        /// </summary>
        [SerializeField]
        protected Text m_ItemName;
        /// <summary>
        /// Should the name be colored?
        /// </summary>
        [SerializeField]
        protected bool m_UseRarityColor=false;

        /// <summary>
        /// The Image to display item icon.
        /// </summary>
        [SerializeField]
        protected Image m_Icon;
        /// <summary>
		/// The text to display item stack.
		/// </summary>
		[SerializeField]
        protected Text m_Stack;

        //Actions to run when the trigger is used.
        [HideInInspector]
        public List<Restriction> restrictions = new List<Restriction>();

        private Item m_Item;
        /// <summary>
        /// The item this slot is holding
        /// </summary>
        public Item ObservedItem
        {
            get{
                return this.m_Item;
            }
            set {
                this.m_Item = value;
                Repaint();
            }
        }

        /// <summary>
        /// Checks if the slot is empty ObservedItem == null
        /// </summary>
        public bool IsEmpty {
            get { return ObservedItem == null; }
        }

        private ItemContainer m_Container;
        /// <summary>
        /// The item container that holds this slot
        /// </summary>
        public ItemContainer Container {
            get {return this.m_Container;}
            set { this.m_Container = value; }
        }

        private int m_Index = -1;
        /// <summary>
        /// Index of item container
        /// </summary>
        public int Index {
            get { return this.m_Index; }
            set { this.m_Index = value; }
        }

        public override string[] Callbacks {
            get
            {
                List<string> callbacks = new List<string>();
                callbacks.Add("OnAddItem");
                callbacks.Add("OnRemoveItem");
                callbacks.Add("OnUseItem");
                return callbacks.ToArray();
            }
        }

        protected virtual void Start() {
            if (Container == null)
            {
                Container = GetComponentInParent<ItemContainer>();
                if (Container == null)
                {
                    Debug.LogWarning("Slot requires an ItemContainer reference.", this);
                    return;
                }
            }

            Container.OnAddItem += (Item item, Slot slot) => {
                if (slot == this)
                {
                    ItemEventData eventData = new ItemEventData(item);
                    eventData.slot = slot;
                    Execute("OnAddItem", eventData);
                }

            };
            Container.OnRemoveItem += (Item item, int amount, Slot slot) => {
                if (slot == this)
                {
                    ItemEventData eventData = new ItemEventData(item);
                    eventData.slot = slot;
                    Execute("OnRemoveItem", eventData);
                }

            };
            Container.OnUseItem += (Item item, Slot slot) => {
                if (slot == this)
                {
                    ItemEventData eventData = new ItemEventData(item);
                    eventData.slot = slot;
                    Execute("OnUseItem", eventData);
                }
            };

            if (this.m_Stack != null)
                this.m_Stack.raycastTarget = false;
            
            // 초기화 시 아이콘 상태 확인 및 수정
            if (this.m_Icon != null && !IsEmpty && ObservedItem != null && ObservedItem.Icon != null)
            {
                Repaint();
            }
        }

        /// <summary>
        /// Repaint slot visuals with item information
        /// </summary>
        public virtual void Repaint()
        {
            if (this.m_ItemName != null){
                //Updates the text with item name and rarity color. If this slot is empty, sets the text to empty.
                this.m_ItemName.text = (!IsEmpty ? (this.m_UseRarityColor?UnityTools.ColorString(ObservedItem.DisplayName, ObservedItem.Rarity.Color):ObservedItem.DisplayName) : string.Empty);
            }

            if (this.m_Icon != null){
                var hasIcon = !IsEmpty && ObservedItem != null && ObservedItem.Icon != null;
                
                if (hasIcon){
                    // 스프라이트가 실제로 존재하는지 확인
                    if (ObservedItem.Icon == null)
                    {
                        Debug.LogError($"[Slot Repaint] Slot {Index}: ObservedItem.Icon is NULL!", this);
                        return;
                    }
                    
                    // GameObject를 먼저 활성화 (스프라이트 할당 전에)
                    if (!this.m_Icon.gameObject.activeSelf)
                    {
                        this.m_Icon.gameObject.SetActive(true);
                    }
                    
                    // Image 컴포넌트를 먼저 활성화
                    if (!this.m_Icon.enabled)
                    {
                        this.m_Icon.enabled = true;
                    }
                    
                    // overrideSprite를 먼저 null로 설정
                    this.m_Icon.overrideSprite = null;
                    
                    // sprite에 직접 할당
                    this.m_Icon.sprite = ObservedItem.Icon;
                    
                    // 할당 실패 시 재시도 및 에러 로그
                    if (this.m_Icon.sprite == null && ObservedItem.Icon != null)
                    {
                        Debug.LogError($"[Slot Repaint] Slot {Index}: FAILED to assign sprite! Item={ObservedItem?.DisplayName}, Icon={ObservedItem.Icon?.name}", this);
                        
                        // 다시 시도
                        this.m_Icon.sprite = ObservedItem.Icon;
                        this.m_Icon.overrideSprite = ObservedItem.Icon;
                        
                        // 재확인
                        if (this.m_Icon.sprite == null && this.m_Icon.overrideSprite == null)
                        {
                            Debug.LogError($"[Slot Repaint] Slot {Index}: CRITICAL - Sprite assignment completely failed!", this);
                        }
                    }
                    
                    // Color를 흰색으로 설정 (검은색이면 안 보임)
                    var color = Color.white;
                    color.a = 1f;
                    this.m_Icon.color = color;
                    
                    // 다른 Image 컴포넌트가 위에 있는지 확인하고 순서 조정
                    var parent = this.m_Icon.transform.parent;
                    if (parent != null)
                    {
                        var siblings = parent.GetComponentsInChildren<Image>();
                        int iconSiblingIndex = this.m_Icon.transform.GetSiblingIndex();
                        bool needsReorder = false;
                        
                        foreach (var sibling in siblings)
                        {
                            if (sibling != this.m_Icon && sibling.gameObject.activeSelf && sibling.enabled)
                            {
                                var siblingRect = sibling.rectTransform;
                                var iconRect = this.m_Icon.rectTransform;
                                if (siblingRect != null && iconRect != null)
                                {
                                    // 같은 위치에 있고 더 위에 있는지 확인
                                    if (Vector2.Distance(siblingRect.anchoredPosition, iconRect.anchoredPosition) < 1f)
                                    {
                                        int siblingIndex = sibling.transform.GetSiblingIndex();
                                        if (siblingIndex > iconSiblingIndex)
                                        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                                            Debug.LogWarning($"[Slot Repaint] Slot {Index}: Another Image '{sibling.name}' is above Icon at same position! Moving Icon to top.", this);
#endif
                                            needsReorder = true;
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Icon을 가장 위로 이동 (나중에 렌더링되도록)
                        if (needsReorder)
                        {
                            this.m_Icon.transform.SetAsLastSibling();
                        }
                    }
                    
                    // UI 강제 업데이트
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.m_Icon.rectTransform);
                    this.m_Icon.SetAllDirty();
                    
                    // Canvas 강제 업데이트
                    var canvas = this.m_Icon.canvas;
                    if (canvas != null)
                    {
                        Canvas.ForceUpdateCanvases();
                    }
                    
                    
                    // 검증 실패 시 에러 로그
                    if (!this.m_Icon.gameObject.activeSelf)
                    {
                        Debug.LogError($"[Slot Repaint] Slot {Index}: CRITICAL - GameObject activation failed! Parent active: {this.m_Icon.transform.parent?.gameObject.activeSelf}", this);
                    }
                    
                    if (!this.m_Icon.enabled)
                    {
                        Debug.LogError($"[Slot Repaint] Slot {Index}: CRITICAL - Image component enabling failed!", this);
                    }
                    
                    if (this.m_Icon.sprite == null)
                    {
                        Debug.LogError($"[Slot Repaint] Slot {Index}: CRITICAL - Sprite assignment failed! Item.Icon={ObservedItem.Icon?.name}", this);
                    }
                    
                    // UI 계층 문제 확인 - CanvasGroup alpha 체크
                    var canvasGroup = this.m_Icon.GetComponent<CanvasGroup>();
                    if (canvasGroup != null && canvasGroup.alpha < 0.01f)
                    {
                        Debug.LogWarning($"[Slot Repaint] Slot {Index}: WARNING - CanvasGroup alpha is {canvasGroup.alpha}, setting to 1.0", this);
                        canvasGroup.alpha = 1f;
                    }
                    
                }else {
                    //If there is no item in this slot, disable icon
                    this.m_Icon.sprite = null;
                    this.m_Icon.overrideSprite = null;
                    this.m_Icon.enabled = false;
                    this.m_Icon.gameObject.SetActive(false);
                }
            }
            else
            {
                // m_Icon이 null인 경우 경고
                if (!IsEmpty && ObservedItem != null && ObservedItem.Icon != null)
                {
                    Debug.LogWarning($"[Slot] m_Icon is null for slot {Index} but item {ObservedItem.DisplayName} has icon!", this);
                }
            }

            if (this.m_Stack != null) {
                if (!IsEmpty && ObservedItem.MaxStack > 1 ){
                    //Updates the stack and enables it.
                    this.m_Stack.text = ObservedItem.Stack.ToString();
                    this.m_Stack.enabled = true;
                }else{
                    //If there is no item in this slot, disable stack field
                    this.m_Stack.enabled = false;
                }
            }
        }

        //Use the item
        public virtual void Use() {
            Container.NotifyTryUseItem(ObservedItem, this);
            //Check if the item can be used.
            if (CanUse())
            {
                //Check if there is an override item behavior on trigger.
                if ((Trigger.currentUsedTrigger as Trigger) != null && (Trigger.currentUsedTrigger as Trigger).OverrideUse(this, ObservedItem))
                {
                    return;
                }
                if (Container.UseReferences)
                {
                    ObservedItem.Slot.Use();
                    return;
                }
                //Try to move item
                if (!MoveItem())
                {
                    Debug.Log("use");
                    ObservedItem.Use();
                    Container.NotifyUseItem(ObservedItem, this);
                }
            }
        }

        //Checks if we can use the item in this slot
        public virtual bool CanUse() {
            return true;
        }

        /// <summary>
        /// Try to move item by move conditions set in inspector
        /// </summary>
        /// <returns>True if item was moved.</returns>
        public virtual bool MoveItem() {

            if (Container.MoveUsedItem)
            {
                for (int i = 0; i < Container.moveItemConditions.Count; i++)
                {
                    ItemContainer.MoveItemCondition condition = Container.moveItemConditions[i];
                    ItemContainer moveToContainer = WidgetUtility.Find<ItemContainer>(condition.window);
                    if (moveToContainer == null || (condition.requiresVisibility && !moveToContainer.IsVisible))
                    {
                        continue;
                    }
                    if (moveToContainer.IsLocked) {
                        InventoryManager.Notifications.inUse.Show();
                        continue;
                    }

                    if (moveToContainer.CanAddItem(ObservedItem) && moveToContainer.StackOrAdd(ObservedItem))
                    {
                        if (!moveToContainer.UseReferences || !Container.CanReferenceItems){
                           // Debug.Log("Move Item from "+Container.Name+" to "+moveToContainer.Name);

                            if (!moveToContainer.CanReferenceItems)
                            {
                                ItemContainer.RemoveItemReferences(ObservedItem);
                            }
                            Container.RemoveItem(Index);
                        }


                        return true;
                    }
                    for (int j = 0; j < moveToContainer.Slots.Count; j++)
                    {
                        if (moveToContainer.CanSwapItems(moveToContainer.Slots[j],this) && moveToContainer.SwapItems(moveToContainer.Slots[j], this))
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// Can the item be added to this slot. This does not check if the slot is empty.
        /// </summary>
        /// <param name="item">The item to test adding.</param>
        /// <returns>Returns true if the item can be added.</returns>
        public virtual bool CanAddItem(Item item)
        {
            if (item == null) { return true; }
            for (int i = 0; i < restrictions.Count; i++)
            {
                if (!restrictions[i].CanAddItem(item))
                {
                    return false;
                }
            }
            return true;
        }



    }
}
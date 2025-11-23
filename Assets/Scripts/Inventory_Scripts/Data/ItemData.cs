using DevionGames.InventorySystem;
using UnityEngine;

namespace StoreGame.Data
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "StoreGame/Item Data", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId = System.Guid.NewGuid().ToString();
        [SerializeField] private string displayName = "New Item";

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [TextArea]
        [SerializeField] private string description;

        [Header("Stacking")]
        [Min(1)]
        [SerializeField] private int maxStackSize = 1;

        [Header("Devion Inventory Integration")]
        [Tooltip("Devion Games Inventory System용 Item 템플릿. 에셋 인벤토리로 지급하려면 반드시 지정하세요.")]
        [SerializeField] private Item devionItemTemplate;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public ItemRarity Rarity => rarity;
        public string Description => description;
        public int MaxStackSize => maxStackSize;
        public Item DevionItemTemplate => devionItemTemplate;
    }
}



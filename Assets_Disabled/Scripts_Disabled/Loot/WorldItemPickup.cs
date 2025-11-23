using StoreGame.Data;
using StoreGame.Integration;
using UnityEngine;

namespace StoreGame.Loot
{
    [RequireComponent(typeof(Collider))]
    public class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private string devionWindowName = "Inventory";

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        public void Configure(ItemData item, int count)
        {
            itemData = item;
            amount = count;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (itemData == null || amount <= 0)
            {
                return;
            }

            if (DevionInventoryBridge.TryAddItem(itemData, amount, devionWindowName))
            {
                Destroy(gameObject);
            }
        }
    }
}



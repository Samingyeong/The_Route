using System.Collections.Generic;
using StoreGame.Data;
using StoreGame.Integration;
using UnityEngine;

namespace StoreGame.Loot
{
    public class LootSpawner : MonoBehaviour
    {
        [SerializeField] private LootTable lootTable;
        [SerializeField] private int currentWave;
        [SerializeField] private WorldItemPickup worldItemPickupPrefab;
        [SerializeField] private float scatterRadius = 2f;
        [SerializeField] private string devionWindowName = "Inventory";

        public void SpawnLoot()
        {
            if (lootTable == null)
            {
                UnityEngine.Debug.LogWarning($"{nameof(LootSpawner)} missing loot table", this);
                return;
            }

            var results = lootTable.Roll(currentWave);
            if (results.Count == 0)
            {
                return;
            }

            foreach (var result in results)
            {
                DropItem(result);
            }
        }

        private void DropItem(LootResult result)
        {
            if (worldItemPickupPrefab == null)
            {
                TryGiveDirectly(result);
                return;
            }

            var spawnPosition = transform.position + Random.insideUnitSphere * scatterRadius;
            spawnPosition.y = transform.position.y;

            var pickup = Instantiate(worldItemPickupPrefab, spawnPosition, Quaternion.identity);
            pickup.Configure(result.Item, result.Amount);
        }

        private void TryGiveDirectly(LootResult result)
        {
            if (DevionInventoryBridge.TryAddItem(result.Item, result.Amount, devionWindowName))
            {
                return;
            }

            UnityEngine.Debug.LogWarning("Failed to add item to Devion inventory.", this);
        }
    }
}



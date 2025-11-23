using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoreGame.Data
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "StoreGame/Loot Table", order = 1)]
    public class LootTable : ScriptableObject
    {
        [SerializeField] private List<LootEntry> entries = new();

        public IReadOnlyList<LootEntry> Entries => entries;

        public List<LootResult> Roll(int waveIndex)
        {
            var results = new List<LootResult>();

            foreach (var entry in entries)
            {
                if (entry.Item == null)
                {
                    continue;
                }

                var adjustedChance = entry.DropChance;
                if (entry.ScaleWithWave && entry.WaveChanceMultiplier != null)
                {
                    var evaluated = entry.WaveChanceMultiplier.Evaluate(waveIndex);
                    adjustedChance *= Mathf.Max(0f, evaluated);
                }

                if (UnityEngine.Random.value > adjustedChance)
                {
                    continue;
                }

                var amount = UnityEngine.Random.Range(entry.AmountRange.x, entry.AmountRange.y + 1);
                amount = Mathf.Max(1, amount);

                if (entry.ScaleWithWave && entry.WaveAmountMultiplier != null)
                {
                    var waveMultiplier = entry.WaveAmountMultiplier.Evaluate(waveIndex);
                    amount = Mathf.RoundToInt(amount * Mathf.Max(0f, waveMultiplier));
                }

                if (amount <= 0)
                {
                    continue;
                }

                results.Add(new LootResult(entry.Item, amount));
            }

            return results;
        }

        [Serializable]
        public class LootEntry
        {
            [SerializeField] private ItemData item;
            [Min(0f)]
            [SerializeField] private float dropChance = 1f;
            [SerializeField] private Vector2Int amountRange = new(1, 1);
            [SerializeField] private bool scaleWithWave;
            [SerializeField] private AnimationCurve waveChanceMultiplier = AnimationCurve.Linear(0, 1, 10, 1);
            [SerializeField] private AnimationCurve waveAmountMultiplier = AnimationCurve.Linear(0, 1, 10, 1);

            public ItemData Item => item;
            public float DropChance => dropChance;
            public Vector2Int AmountRange => amountRange;
            public bool ScaleWithWave => scaleWithWave;
            public AnimationCurve WaveChanceMultiplier => waveChanceMultiplier;
            public AnimationCurve WaveAmountMultiplier => waveAmountMultiplier;
        }
    }

    [Serializable]
    public struct LootResult
    {
        public ItemData Item { get; }
        public int Amount { get; }

        public LootResult(ItemData item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}



// The running meal: the list of added items and their totals. On AppRoot, and
// deliberately unaware of Vuforia so losing a target cannot erase it.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenMacros
{
    public class MealEntry
    {
        public readonly string DisplayName;
        public readonly float Grams;
        public readonly MacroValues Macros;

        public MealEntry(ProductData product, float grams)
        {
            DisplayName = product.displayName;
            Grams = grams;
            Macros = product.MacrosFor(grams);
        }
    }

    [DefaultExecutionOrder(-150)]
    public class MealSession : MonoBehaviour
    {
        public static MealSession Instance { get; private set; }

        readonly List<MealEntry> _entries = new List<MealEntry>();

        /// <summary>Raised after any change. The HUD listens to this.</summary>
        public event Action OnMealChanged;

        public int Count => _entries.Count;
        public MacroValues Totals { get; private set; } = MacroValues.Zero;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Add(ProductData product, float grams)
        {
            if (product == null || grams <= 0f) return;

            _entries.Add(new MealEntry(product, grams));
            Recalculate();
        }

        public void RemoveLast()
        {
            if (_entries.Count == 0) return;

            _entries.RemoveAt(_entries.Count - 1);
            Recalculate();
        }

        public void Clear()
        {
            if (_entries.Count == 0) return;

            _entries.Clear();
            Recalculate();
        }

        void Recalculate()
        {
            var sum = MacroValues.Zero;
            foreach (var entry in _entries) sum += entry.Macros;

            Totals = sum;
            OnMealChanged?.Invoke();
        }
    }
}

// Loads products.json and finds a product by Vuforia target name.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenMacros
{
    [Serializable]
    public struct MacroValues
    {
        public float Kcal;
        public float Protein;
        public float Carbs;
        public float Fat;

        public static readonly MacroValues Zero = new MacroValues();

        public MacroValues(float kcal, float protein, float carbs, float fat)
        {
            Kcal = kcal;
            Protein = protein;
            Carbs = carbs;
            Fat = fat;
        }

        public static MacroValues operator +(MacroValues a, MacroValues b) =>
            new MacroValues(a.Kcal + b.Kcal, a.Protein + b.Protein, a.Carbs + b.Carbs, a.Fat + b.Fat);
    }

    [Serializable]
    public class ProductData
    {
        public string targetName;      // must match the Vuforia target name
        public string displayName;

        public float kcalPer100g;
        public float proteinPer100g;
        public float carbsPer100g;
        public float fatPer100g;

        public float defaultPortionGrams = 100f;
        public float minPortionGrams = 10f;
        public float maxPortionGrams = 300f;

        public MacroValues MacrosFor(float grams)
        {
            var k = grams / 100f;
            return new MacroValues(kcalPer100g * k, proteinPer100g * k, carbsPer100g * k, fatPer100g * k);
        }

    }

    [Serializable]
    public class ProductCatalogJson
    {
        public ProductData[] products;
    }

    [DefaultExecutionOrder(-200)]
    public class ProductCatalog : MonoBehaviour
    {

        const string ResourceName = "products";

        public static ProductCatalog Instance { get; private set; }

        readonly Dictionary<string, ProductData> _byTargetName =
            new Dictionary<string, ProductData>(StringComparer.OrdinalIgnoreCase);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Load();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool TryGet(string targetName, out ProductData product)
        {
            product = null;
            return !string.IsNullOrEmpty(targetName) && _byTargetName.TryGetValue(targetName, out product);
        }

        void Load()
        {

            var asset = Resources.Load<TextAsset>(ResourceName);

            if (asset == null)
            {
                Debug.LogError($"[Catalog] Assets/Resources/{ResourceName}.json not found.");
                return;
            }

            Parse(asset.text);
            Debug.Log($"[Catalog] Loaded {_byTargetName.Count} products.");
        }

        void Parse(string json)
        {
            ProductCatalogJson parsed;

            try
            {
                parsed = JsonUtility.FromJson<ProductCatalogJson>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Catalog] products.json is not valid JSON: {e.Message}");
                return;
            }

            if (parsed?.products == null || parsed.products.Length == 0)
            {
                Debug.LogError("[Catalog] products.json has no \"products\" array.");
                return;
            }

            foreach (var product in parsed.products)
            {
                if (product == null || string.IsNullOrWhiteSpace(product.targetName)) continue;

                if (_byTargetName.ContainsKey(product.targetName))
                {
                    Debug.LogWarning($"[Catalog] Duplicate target '{product.targetName}' ignored.");
                    continue;
                }

                _byTargetName.Add(product.targetName, product);
            }
        }
    }
}

// Reads products.json from Resources at start-up and looks products up by Vuforia
// target name. On AppRoot.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenMacros
{

    [DefaultExecutionOrder(-200)]
    public class ProductCatalog : MonoBehaviour
    {

        const string ResourceName = "products";

        public static ProductCatalog Instance { get; private set; }

        public string LoadError { get; private set; }


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
                LoadError = $"Assets/Resources/{ResourceName}.json not found.";
                Debug.LogError($"[Catalog] {LoadError}");
                return;
            }

            Parse(asset.text);

            if (!string.IsNullOrEmpty(LoadError)) Debug.LogError($"[Catalog] {LoadError}");
            else Debug.Log($"[Catalog] Loaded {_byTargetName.Count} products.");
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
                LoadError = $"products.json is not valid JSON: {e.Message}";
                return;
            }

            if (parsed?.products == null || parsed.products.Length == 0)
            {
                LoadError = "products.json has no \"products\" array.";
                return;
            }

            foreach (var product in parsed.products)
            {
                if (product == null || string.IsNullOrWhiteSpace(product.targetName)) continue;

                product.Normalise();

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

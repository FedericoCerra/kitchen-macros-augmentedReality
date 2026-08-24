// Fills in the text of the world-space panel. On the panel prefab. It has no Unity
// callbacks at all: TrackedProduct calls Bind() once when it creates the panel.

using TMPro;
using UnityEngine;

namespace KitchenMacros
{

    public class ProductInfoPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text kcalText;
        [SerializeField] TMP_Text proteinText;
        [SerializeField] TMP_Text carbsText;
        [SerializeField] TMP_Text fatText;

        public void Bind(ProductData product)
        {
            if (product == null) return;

            if (nameText != null) nameText.text = product.displayName;
            if (kcalText != null) kcalText.text = $"{product.kcalPer100g:0} kcal";
            if (proteinText != null) proteinText.text = $"P {product.proteinPer100g:0.#} g";
            if (carbsText != null) carbsText.text = $"C {product.carbsPer100g:0.#} g";
            if (fatText != null) fatText.text = $"F {product.fatPer100g:0.#} g";

        }
    }
}

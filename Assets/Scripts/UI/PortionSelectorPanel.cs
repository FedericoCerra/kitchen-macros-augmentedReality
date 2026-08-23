// The portion card: slider, live macro preview and Add to meal. On the canvas rather
// than on the card it shows and hides, so it cannot disable itself.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitchenMacros
{

    public class PortionSelectorPanel : MonoBehaviour
    {
        [SerializeField] GameObject cardRoot;

        [Header("Header")]
        [SerializeField] TMP_Text productNameText;

        [Header("Portion")]
        [SerializeField] Slider portionSlider;
        [SerializeField] TMP_Text portionText;
        [SerializeField] float portionStepGrams = 5f;

        [Header("Preview for the chosen portion")]
        [SerializeField] TMP_Text kcalText;
        [SerializeField] TMP_Text proteinText;
        [SerializeField] TMP_Text carbsText;
        [SerializeField] TMP_Text fatText;

        [Header("Actions")]
        [SerializeField] Button addButton;
        [SerializeField] Button closeButton;

        ProductData _product;
        float _grams;


        bool _suppressSliderCallback;

        void Awake()
        {
            if (portionSlider != null) portionSlider.onValueChanged.AddListener(HandleSliderChanged);
            if (addButton != null) addButton.onClick.AddListener(AddToMeal);
            if (closeButton != null) closeButton.onClick.AddListener(CloseRequested);

            if (cardRoot != null) cardRoot.SetActive(false);
        }

        void Start()
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;
        }

        void OnDestroy()
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.OnSelectionChanged -= HandleSelectionChanged;
        }

        void HandleSelectionChanged(TrackedProduct trackedProduct)
        {
            if (trackedProduct == null || trackedProduct.Product == null) Close();
            else Show(trackedProduct.Product);
        }

        void Show(ProductData product)
        {
            _product = product;
            if (_product == null) return;

            if (cardRoot != null) cardRoot.SetActive(true);
            if (productNameText != null) productNameText.text = _product.displayName;


            if (portionSlider != null)
            {
                _suppressSliderCallback = true;
                portionSlider.minValue = _product.minPortionGrams;
                portionSlider.maxValue = _product.maxPortionGrams;
                _suppressSliderCallback = false;
            }

            SetGrams(_product.defaultPortionGrams);
        }

        void Close()
        {
            _product = null;
            if (cardRoot != null) cardRoot.SetActive(false);
        }


        void CloseRequested()
        {
            if (SelectionManager.Instance != null) SelectionManager.Instance.ClearSelection();
            else Close();
        }


        void HandleSliderChanged(float value)
        {
            if (!_suppressSliderCallback) SetGrams(value);
        }

        void SetGrams(float grams)
        {
            if (_product == null) return;

            if (portionStepGrams > 0f)
                grams = Mathf.Round(grams / portionStepGrams) * portionStepGrams;

            _grams = Mathf.Clamp(grams, _product.minPortionGrams, _product.maxPortionGrams);

            if (portionSlider != null && !Mathf.Approximately(portionSlider.value, _grams))
            {
                _suppressSliderCallback = true;
                portionSlider.value = _grams;
                _suppressSliderCallback = false;
            }

            RefreshPreview();
        }

        void RefreshPreview()
        {
            // Values only; the units and macro names are static captions in the layout.
            if (portionText != null) portionText.text = $"{_grams:0}";

            var macros = _product.MacrosFor(_grams);

            if (kcalText != null) kcalText.text = $"{macros.Kcal:0}";
            if (proteinText != null) proteinText.text = $"{macros.Protein:0.#} g";
            if (carbsText != null) carbsText.text = $"{macros.Carbs:0.#} g";
            if (fatText != null) fatText.text = $"{macros.Fat:0.#} g";

        }

        void AddToMeal()
        {
            if (_product == null || MealSession.Instance == null) return;

            MealSession.Instance.Add(_product, _grams);

            // Clearing the selection closes this card through OnSelectionChanged.
            if (SelectionManager.Instance != null) SelectionManager.Instance.ClearSelection();
            else Close();
        }
    }
}

// Writes the meal totals into the bottom bar. On the canvas. Runs only when
// MealSession raises OnMealChanged, never every frame.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitchenMacros
{

    public class MealHudView : MonoBehaviour
    {
        [Header("Totals")]
        [SerializeField] TMP_Text kcalText;
        [SerializeField] TMP_Text proteinText;
        [SerializeField] TMP_Text carbsText;
        [SerializeField] TMP_Text fatText;
        [SerializeField] TMP_Text itemCountText;

        [Header("States")]
        [SerializeField] GameObject emptyStateRoot;
        [SerializeField] GameObject totalsRoot;

        [Header("Actions")]
        [SerializeField] Button undoButton;
        [SerializeField] Button clearButton;


        void Awake()
        {
            if (undoButton != null) undoButton.onClick.AddListener(() => MealSession.Instance?.RemoveLast());
            if (clearButton != null) clearButton.onClick.AddListener(() => MealSession.Instance?.Clear());
        }

        void Start()
        {
            if (MealSession.Instance != null) MealSession.Instance.OnMealChanged += Refresh;

            Refresh();
        }

        void OnDestroy()
        {
            if (MealSession.Instance != null) MealSession.Instance.OnMealChanged -= Refresh;
        }


        void Refresh()
        {
            var session = MealSession.Instance;
            var count = session != null ? session.Count : 0;
            var totals = session != null ? session.Totals : MacroValues.Zero;

            if (emptyStateRoot != null) emptyStateRoot.SetActive(count == 0);
            if (totalsRoot != null) totalsRoot.SetActive(count > 0);

            // Values only; the units and macro names are static captions in the layout.
            if (kcalText != null) kcalText.text = $"{totals.Kcal:0}";
            if (proteinText != null) proteinText.text = $"{totals.Protein:0.#} g";
            if (carbsText != null) carbsText.text = $"{totals.Carbs:0.#} g";
            if (fatText != null) fatText.text = $"{totals.Fat:0.#} g";

            if (itemCountText != null)
                itemCountText.text = count == 1 ? "1 item" : $"{count} items";
        }
    }
}

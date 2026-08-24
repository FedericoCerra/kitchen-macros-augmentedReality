// Data classes for one product and its macros. Not a MonoBehaviour: instances are
// created by JsonUtility when products.json is read.

using System;

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
}

namespace Nutrify.Contracts.FoodItems;

public enum BarcodeLookupSource
{
    /// <summary>The barcode matched a food item the user already has.</summary>
    Internal = 0,

    /// <summary>The product was found in an external database (Open Food Facts).</summary>
    External = 1
}

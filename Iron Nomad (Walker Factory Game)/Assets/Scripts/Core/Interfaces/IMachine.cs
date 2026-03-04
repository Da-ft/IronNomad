using UnityEngine;

public interface IMachine
{
    CraftingRecipe[] GetRecipes();
    CraftingRecipe GetCurrentRecipe();
    void SetRecipe(CraftingRecipe recipe);
}
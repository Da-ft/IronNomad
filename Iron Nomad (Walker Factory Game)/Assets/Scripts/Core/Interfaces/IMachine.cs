using UnityEngine;

public interface IMachine
{
    CraftingRecipe[] GetRecipes();
    CraftingRecipe GetCurrentRecipe();
    void SetRecipe(CraftingRecipe recipe);
    float GetProgress();        // 0-1 für Progressbar
    float GetInputPerMinute();
    float GetOutputPerMinute();
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "IronNomad/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Input")]
    public ItemDefinition[] Inputs;
    public int[] InputAmounts;

    [Header("Output")]
    public ItemDefinition Output;
    public int OutputAmount = 1;

    [Header("Timing")]
    public float Duration = 5f;

    // Hilfsmethode: Hat diese Maschine genug Input?
    public bool CanCraft(Dictionary<ItemDefinition, int> available)
    {
        for (int i = 0; i < Inputs.Length; i++)
        {
            int needed = i < InputAmounts.Length ? InputAmounts[i] : 1;
            if (!available.TryGetValue(Inputs[i], out int count) || count < needed)
                return false;
        }
        return true;
    }
}
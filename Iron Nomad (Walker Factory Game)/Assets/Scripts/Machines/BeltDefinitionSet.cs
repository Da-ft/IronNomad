using UnityEngine;

/// <summary>
/// Erbt von BuildingDefinition → taucht automatisch im Baumenü auf.
/// BuildingPrefab und Ports = die Gerade.
/// CurveDefinition = die Kurve (eigenes Prefab + eigene Ports).
/// </summary>
[CreateAssetMenu(menuName = "IronNomad/Belt Definition Set")]
public class BeltDefinitionSet : BuildingDefinition
{
    [Header("Kurven-Prefab")]
    public BuildingDefinition CurveDefinition;
}
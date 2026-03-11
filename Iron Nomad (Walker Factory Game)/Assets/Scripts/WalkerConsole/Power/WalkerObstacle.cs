using UnityEngine;

[System.Serializable]
public class WalkerObstacle
{
    public string DisplayName;
    public Sprite Icon;
    public Vector2Int GridPosition;
    public Vector2Int GridSize = Vector2Int.one;
    public BuildingCost[] RepairCosts;
}
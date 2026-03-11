using UnityEngine;

public enum SurfaceBonusType { None, Solar, Wind }

[CreateAssetMenu(menuName = "IronNomad/Walker Surface")]
public class WalkerSurface : ScriptableObject
{
    [Header("Info")]
    public string SurfaceName;
    public Vector2Int GridSize = new Vector2Int(4, 4);

    [Header("Bonus")]
    public SurfaceBonusType BonusType = SurfaceBonusType.None;
    [Range(0f, 3f)]
    public float BonusMultiplier = 1f;

    [Header("Obstacles")]
    public WalkerObstacle[] Obstacles;
}
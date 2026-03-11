///<summary>
/// Wird gefeuert wenn eine Zelle in WalkerSurfaceGridUI angeklickt wird.
/// WalkerPowerMenuUI wertet das aus und aktualisiert das SelectedPanel.
/// </summary>

public class CellSelectionInfo
{
    public enum CellState { Free, Occupied, Obstacle }

    public CellState State;
    public int X;
    public int Y;
    public BuildingDefinition Building;
    public WalkerObstacle Obstacle;
    public WalkerSurfaceGridUI Surface;
}
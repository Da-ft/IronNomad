/// <summary>
/// Alle Maschinen die Strom brauchen implementieren dieses Interface.
/// </summary>
public interface IPowerConsumer
{
    float PowerDemand { get; }  // in MW
    bool IsPowered { get; set; }
}
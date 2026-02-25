public interface IMineable
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    void Mine(float damage);
}
public interface IMenu
{
    bool IsOpen { get; }
    void Open();
    void Close();
}
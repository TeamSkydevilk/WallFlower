/// <summary>
/// Observe Pattern
/// Subject
/// </summary>

public interface Subject
{
    void RegisterObserver(Observer o);
    void RemoveObserver(Observer o);
    void NotifyObservers(bool isCheck);
}

namespace TheAdventure.Models;

public class GameObject
{
    public int Id { get; private set; }

    private static int _nextId = 0;

    public GameObject()
    {
        // Thread-safe auto ID assignment
        Id = Interlocked.Increment(ref _nextId);
    }
}

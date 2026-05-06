namespace TheAdventure.Core;

/// <summary>
/// Tracks the player and offsets all rendering so the player stays centered.
/// Clamps to map edges so black never shows outside the map.
/// </summary>
public class Camera
{
    private int _x;
    private int _y;

    private readonly int _windowWidth;
    private readonly int _windowHeight;

    private int _mapPixelWidth;
    private int _mapPixelHeight;

    public int X => _x;
    public int Y => _y;

    public Camera(int windowWidth, int windowHeight)
    {
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
    }

    public void SetMapSize(int mapPixelWidth, int mapPixelHeight)
    {
        _mapPixelWidth = mapPixelWidth;
        _mapPixelHeight = mapPixelHeight;
    }

    public void Follow(int targetX, int targetY)
    {
        // Center the camera on the target
        _x = targetX - _windowWidth / 2;
        _y = targetY - _windowHeight / 2;

        // Clamp so we never show black outside the map
        _x = Math.Max(0, Math.Min(_x, _mapPixelWidth - _windowWidth));
        _y = Math.Max(0, Math.Min(_y, _mapPixelHeight - _windowHeight));
    }

    /// <summary>
    /// Converts a world position to a screen position
    /// </summary>
    public (int screenX, int screenY) WorldToScreen(int worldX, int worldY)
    {
        return (worldX - _x, worldY - _y);
    }
}

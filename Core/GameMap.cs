using System.Text.Json;
using Silk.NET.Maths;
using TheAdventure.Models.Data;

namespace TheAdventure.Core;

public class GameMap
{
    private readonly int _tileSize = 16;
    private int _mapWidth;
    private int _mapHeight;
    private IntPtr _tilesetTexture;
    private int _tilesPerRow = 8;

    private int[,] _collisionMap = new int[0, 0];

    private readonly HashSet<int> _walkableTiles = new()
    {
        39, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62,
        73, 87, 88, 89, 90, 91, 92, 101, 102, 109, 71,
        146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156
    };

    private readonly List<(int tileId, int x, int y)> _allTiles = new();

    public int[,] CollisionMap => _collisionMap;
    public int TileSize => _tileSize;
    public int MapWidth => _mapWidth;
    public int MapHeight => _mapHeight;
    public int MapPixelWidth => _mapWidth * _tileSize;
    public int MapPixelHeight => _mapHeight * _tileSize;

    public void LoadMap(string fileName, GameRenderer renderer)
    {
        var json = File.ReadAllText(fileName);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var mapFile = JsonSerializer.Deserialize<MapFile>(json, options)!;

        _mapWidth = mapFile.MapWidth;
        _mapHeight = mapFile.MapHeight;
        _collisionMap = new int[_mapHeight, _mapWidth];

        var tilesetId = renderer.LoadTexture(
            Path.Combine("Assets", "spritesheet.png"), out _
        );
        _tilesetTexture = renderer.GetTexturePointer(tilesetId);


        foreach (var layer in mapFile.Layers.AsEnumerable().Reverse())
        {
            foreach (var tile in layer.Tiles)
            {
                int tileId = int.Parse(tile.Id);
                _allTiles.Add((tileId, tile.X, tile.Y));
            }
        }

        // Everything starts blocked
        for (int y = 0; y < _mapHeight; y++)
            for (int x = 0; x < _mapWidth; x++)
                _collisionMap[y, x] = 0;

        // Layer_0 determines walkability
        var groundLayer = mapFile.Layers.FirstOrDefault(l => l.Name == "Layer_0");
        if (groundLayer != null)
        {
            foreach (var tile in groundLayer.Tiles)
            {
                int tileId = int.Parse(tile.Id);
                if (tile.Y < _mapHeight && tile.X < _mapWidth && _walkableTiles.Contains(tileId))
                    _collisionMap[tile.Y, tile.X] = 1;
            }
        }

        // Upper layers block everything they're placed on
        foreach (var layer in mapFile.Layers.Where(l => l.Name != "Layer_0"))
        {
            foreach (var tile in layer.Tiles)
            {
                if (tile.Y < _mapHeight && tile.X < _mapWidth)
                    _collisionMap[tile.Y, tile.X] = 0;
            }
        }
    }

    public void Render(GameRenderer renderer, Camera camera)
    {
        foreach (var (tileId, tileX, tileY) in _allTiles)
        {
            int srcCol = tileId % _tilesPerRow;
            int srcRow = tileId / _tilesPerRow;

            var src = new Rectangle<int>(
                srcCol * _tileSize, srcRow * _tileSize, _tileSize, _tileSize
            );

            // Apply camera offset
            var (screenX, screenY) = camera.WorldToScreen(tileX * _tileSize, tileY * _tileSize);
            var dst = new Rectangle<int>(screenX, screenY, _tileSize, _tileSize);

            renderer.RenderTile(_tilesetTexture, src, dst);
        }
    }
}

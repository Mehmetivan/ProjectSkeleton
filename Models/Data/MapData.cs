using System.Text.Json.Serialization;

namespace TheAdventure.Models.Data;

/// <summary>
/// Top-level map file from SpriteFusion export
/// </summary>
public class MapFile
{
    [JsonPropertyName("mapWidth")]
    public int MapWidth { get; set; }

    [JsonPropertyName("mapHeight")]
    public int MapHeight { get; set; }

    [JsonPropertyName("tileSize")]
    public int TileSize { get; set; }

    [JsonPropertyName("layers")]
    public List<MapLayer> Layers { get; set; } = new();
}

/// <summary>
/// A single layer in the map
/// </summary>
public class MapLayer
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("tiles")]
    public List<MapTile> Tiles { get; set; } = new();
}

/// <summary>
/// A single tile entry — its ID and grid position
/// </summary>
public class MapTile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

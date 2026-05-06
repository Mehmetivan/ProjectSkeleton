using Silk.NET.Maths;
using TheAdventure.Core;

namespace TheAdventure.Models;

public enum NpcType { Farmer, Miner, Turtle }

public class Npc : GameObject
{
    private readonly int _x;
    private readonly int _y;
    private readonly NpcType _type;
    private readonly int _proximityRange = 40; // pixels

    public NpcType Type => _type;
    public int X => _x;
    public int Y => _y;
    public bool QuestComplete { get; set; } = false;

    public string Name => _type switch
    {
        NpcType.Farmer => "Farmer Ali",
        NpcType.Miner => "Bob the Miner",
        NpcType.Turtle => "Oogway",
        _ => "NPC"
    };

    public Npc(int x, int y, NpcType type) : base()
    {
        _x = x;
        _y = y;
        _type = type;
    }

    public bool IsPlayerNearby(int playerX, int playerY)
    {
        var dx = playerX - _x;
        var dy = playerY - _y;
        return Math.Sqrt(dx * dx + dy * dy) <= _proximityRange;
    }

public void Render(GameRenderer renderer, Camera camera)
{
    // NPC sprites will go here later if I`m not lazy.
}
}

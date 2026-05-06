namespace TheAdventure.Models;

public enum CombatPhase
{
    PlayerTurn1,
    PlayerAttacking1,
    AldricTaunt1,
    PlayerTurn2,
    PlayerAttacking2,
    AldricTaunt2,
    AldricAttacking,
    PlayerDying,
    FadeOut,
    Done
}

public class CombatState
{
    public CombatPhase Phase { get; set; } = CombatPhase.PlayerTurn1;
    public int PlayerHealth { get; set; } = 100;
    public int AldricHealth { get; set; } = 100;
    public double AnimationTimer { get; set; } = 0;
    public int AnimationFrame { get; set; } = 0;
    public double FadeAlpha { get; set; } = 0;
    public string TauntText { get; set; } = "SHOW ME WHAT YOU GOT.";
}
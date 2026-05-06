namespace TheAdventure.Models;

public class GameState
{
    public int Honour { get; private set; } = -2;
    public int Coins { get; private set; } = 100;

    // Quest completion tracking
    public bool FarmerQuestDone { get; private set; } = false;
    public bool MinerQuestDone { get; private set; } = false;
    public bool TurtleQuestDone { get; private set; } = false;

    public bool AllQuestsDone => FarmerQuestDone && MinerQuestDone && TurtleQuestDone;

    public void AddHonour(int amount) => Honour += amount;
    public void SpendCoins(int amount) => Coins = Math.Max(0, Coins - amount);

    public void CompleteFarmerQuest() => FarmerQuestDone = true;
    public void CompleteMinerQuest() => MinerQuestDone = true;
    public void CompleteTurtleQuest() => TurtleQuestDone = true;

    public bool IsGoodEnding => Honour >= 1;
}

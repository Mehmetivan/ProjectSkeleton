namespace TheAdventure.Models;

/// Defines all quest dialogues in the game, due to an issue of not being able render ` i ` ( i have no idea why), all ` i `s in dialogue are replaced with ` ! `. Hm Maybe I can actually make that a gameplay future, we`ll see.

public static class Quests
{
    public static List<DialoguePage> FarmerQuest(GameState state) => new()
    {
        new DialoguePage("Farmer Al",
            "Good kn!ght,please hear me out.The harvest th!s year has gone bad." +
            "My young ones have not eaten well for days.We have noth!ng left.")
        ,
        new DialoguePage("Farmer Al",
            $"You look l!ke a person of wealth. You carry {state.Coins} co!ns. " +
            "Could you spare any at all?Even a small part would help us through the cold season.")
            .WithChoice(
                "Here, take all my co!ns.Bless!ngs to your fam!ly.",
                gs => { gs.SpendCoins(gs.Coins); gs.AddHonour(1); gs.CompleteFarmerQuest(); },
                "Bless you,noble kn!ght!You show true honour!My young ones shall eat ton!ght!"
            )
            .WithChoice(
                "No.These go to the tavern.Good luck.",
                gs => { gs.CompleteFarmerQuest(); },
                "! see.May God show mercy on your soul, kn!ght."
            )
    };

    public static List<DialoguePage> MinerQuest() => new()
    {
        new DialoguePage("Bob the Miner",
            "Kn!ght!Thank the heavens you're here.A terr!ble event struck the m!ne. " +
            "The dev!l came and brought the whole tunnel down!")
        ,
        new DialoguePage("Bob the Miner",
            "Three of my crew are st!ll trapped below.We hear them knock but we're " +
            "not strong enough to clear the rubble.Please, can you help us?")
            .WithChoice(
                "Of course.Lead the way,we get them out.",
                gs => { gs.AddHonour(1); gs.CompleteMinerQuest(); },
                "You are a true kn!ght!Come,follow me!There !s no t!me to waste!"
            )
            .WithChoice(
                "No. That work belongs to m!ners, not me.",
                gs => { gs.CompleteMinerQuest(); },
                "You call yourself a kn!ght?Those men have fam!l!es.May you bear that."
            )
    };

    public static List<DialoguePage> TurtleQuest() => new()
    {
        new DialoguePage("Oogway",
            "Ah..a traveler.How fortunate.Age has slowed me,and each search for food grows longer " +
            "each season.But my eggs..they cannot be left alone.")
        ,
        new DialoguePage("Oogway",
            "Wolves roam these parts.And worse.Would you stay near my eggs wh!le ! " +
            "fetch food? ! shall not be long.Perhaps an hour, perhaps two.")
            .WithChoice(
                "Of course,Master Oogway.Go, they stay safe under my watch.",
                gs => { gs.AddHonour(1); gs.CompleteTurtleQuest(); },
                "You have a good heart,young one. ! shall return sw!ftly."
            )
            .WithChoice(
                "Yes,of course..[Leaves and cooks the eggs]",
                gs => { gs.AddHonour(-1); gs.CompleteTurtleQuest(); },
                "..."
            )
    };
}
namespace TheAdventure.Models;


public class DialogueChoice
{
    public string Text { get; }
    public Action<GameState> Effect { get; }   // what happens when chosen
    public string? NextText { get; }           // optional NPC response after choice

    public DialogueChoice(string text, Action<GameState> effect, string? nextText = null)
    {
        Text = text;
        Effect = effect;
        NextText = nextText;
    }
}


// A single page of dialogue with optional choices

public class DialoguePage
{
    public string SpeakerName { get; }
    public string Text { get; }
    public List<DialogueChoice> Choices { get; } = new();

    public bool HasChoices => Choices.Count > 0;

    public DialoguePage(string speakerName, string text)
    {
        SpeakerName = speakerName;
        Text = text;
    }

    public DialoguePage WithChoice(string text, Action<GameState> effect, string? nextText = null)
    {
        Choices.Add(new DialogueChoice(text, effect, nextText));
        return this;
    }
}

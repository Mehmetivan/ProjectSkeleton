namespace TheAdventure.Models;

public enum DialogueState
{
    Inactive,
    ShowingText,
    ShowingChoices,
    ShowingResponse,
    Finished
}

public class DialogueSystem
{
    private List<DialoguePage> _pages = new();
    private int _currentPageIndex = 0;
    private DialogueState _state = DialogueState.Inactive;
    private string _responseText = "";
    private string _responseSpeaker = "";
    private Action? _onComplete;

    public bool IsActive => _state != DialogueState.Inactive && _state != DialogueState.Finished;
    public DialogueState State => _state;

    public DialoguePage? CurrentPage =>
        _currentPageIndex < _pages.Count ? _pages[_currentPageIndex] : null;

    public string ResponseText => _responseText;
    public string ResponseSpeaker => _responseSpeaker;

    public void StartDialogue(List<DialoguePage> pages, Action? onComplete = null)
    {
        _pages = pages;
        _currentPageIndex = 0;
        _onComplete = onComplete;
        _state = DialogueState.ShowingText;
    }


    // Called when player presses E/Space to advance dialogue
    public void Advance(GameState gameState)
    {
        if (_state == DialogueState.ShowingText)
        {
            var page = CurrentPage;
            if (page == null) { Finish(); return; }

            if (page.HasChoices)
                _state = DialogueState.ShowingChoices;
            else
            {
                _currentPageIndex++;
                if (_currentPageIndex >= _pages.Count)
                    Finish();
            }
        }
        else if (_state == DialogueState.ShowingResponse)
        {
            _currentPageIndex++;
            if (_currentPageIndex >= _pages.Count)
                Finish();
            else
                _state = DialogueState.ShowingText;
        }
    }


    // Called when player presses 1 or 2 to make a choice

    public void MakeChoice(int choiceIndex, GameState gameState)
    {
        if (_state != DialogueState.ShowingChoices) return;

        var page = CurrentPage;
        if (page == null || choiceIndex >= page.Choices.Count) return;

        var choice = page.Choices[choiceIndex];
        choice.Effect(gameState);

        if (choice.NextText != null)
        {
            _responseText = choice.NextText;
            _responseSpeaker = page.SpeakerName;
            _state = DialogueState.ShowingResponse;
        }
        else
        {
            _currentPageIndex++;
            if (_currentPageIndex >= _pages.Count)
                Finish();
            else
                _state = DialogueState.ShowingText;
        }
    }

    private void Finish()
    {
        _state = DialogueState.Finished;
        _onComplete?.Invoke();
    }

    public void Reset()
    {
        _state = DialogueState.Inactive;
        _pages.Clear();
        _currentPageIndex = 0;
    }
}

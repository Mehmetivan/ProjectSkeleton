using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;

namespace TheAdventure.Core;

public class DialogueRenderer
{
    private readonly GameRenderer _renderer;
    private readonly int _windowWidth = 1024;
    private readonly int _windowHeight = 800;

    // Fixed layout constants
    private const int BoxX = 20;
    private const int BoxY = 560;
    private const int BoxW = 984;
    private const int BoxH = 220;

    private const int PortraitW = 160;
    private const int PortraitH = 200;
    private const int PortraitY = 340;
    private const int LeftPortraitX = 20;
    private const int RightPortraitX = 1024 - 80 - PortraitW;

    private const int TextX = BoxX + 16;
    private const int SpeakerY = BoxY + 10;
    private const int Line1Y = BoxY + 36;
    private const int Line2Y = BoxY + 56;
    private const int Line3Y = BoxY + 76;
    private const int Choice1Y = BoxY + 110;
    private const int Choice2Y = BoxY + 140;
    private const int NextY = BoxY + 185;
    private const int CharsPerLine = 55;

    private int _knightPortraitId = -1;
    private int _farmerPortraitId = -1;
    private int _minerPortraitId = -1;
    private int _turtlePortraitId = -1;
    private int _aldricPortraitId = -1;

    public DialogueRenderer(GameRenderer renderer)
    {
        _renderer = renderer;
        LoadPortraits();
    }

    private void LoadPortraits()
    {
        _knightPortraitId = _renderer.LoadTexture("Assets/_Idle.png", out _);
        _farmerPortraitId = _renderer.LoadTexture("Assets/farmer.png", out _);
        _minerPortraitId = _renderer.LoadTexture("Assets/miner.png", out _);
        _turtlePortraitId = _renderer.LoadTexture("Assets/turtle.png", out _);
        _aldricPortraitId = _renderer.LoadTexture("Assets/_Idle2.png", out _);
    }

    private int GetNpcPortraitId(string npcName) => npcName switch
    {
        "Farmer Ali" => _farmerPortraitId,
        "Bob the Miner" => _minerPortraitId,
        "Oogway" => _turtlePortraitId,
        "Sr Aldr!c" => _aldricPortraitId,
        _ => -1
    };

    private Rectangle<int> GetNpcSrcRect(string npcName) => npcName switch
    {
        "Oogway" => new Rectangle<int>(120, 0, 250, 130),
        "Sr Aldr!c" => new Rectangle<int>(0, 0, 120, 80),
        _ => new Rectangle<int>(0, 0, 48, 48)
    };

    public void Render(DialogueSystem dialogue, string npcName)
    {
        if (!dialogue.IsActive) return;

        // Dark overlay
        _renderer.DrawFilledRect(0, 0, _windowWidth, _windowHeight, 0, 0, 0, 160);

        // Left portrait
        var knightSrc = new Rectangle<int>(0, 0, 120, 80);
        var knightDst = new Rectangle<int>(LeftPortraitX, PortraitY, PortraitW, PortraitH);
        if (_knightPortraitId >= 0)
            _renderer.RenderTexture(_knightPortraitId, knightSrc, knightDst);
        else
            _renderer.DrawFilledRect(LeftPortraitX, PortraitY, PortraitW, PortraitH, 30, 60, 120);
        _renderer.DrawText("KNIGHT", LeftPortraitX, PortraitY + PortraitH + 4);

        // Right portrait
        int npcPortraitId = GetNpcPortraitId(npcName);
        var npcSrc = GetNpcSrcRect(npcName);
        var npcDst = new Rectangle<int>(RightPortraitX, PortraitY, PortraitW, PortraitH);
        if (npcPortraitId >= 0)
            _renderer.RenderTexture(npcPortraitId, npcSrc, npcDst, RendererFlip.Horizontal);
        else
            _renderer.DrawFilledRect(RightPortraitX, PortraitY, PortraitW, PortraitH, 80, 40, 20);

        // NPC name
        string displayName = npcName.Length > 12 ? npcName[..12] : npcName;
        _renderer.DrawText(displayName.ToUpper(), RightPortraitX, PortraitY + PortraitH + 4);

        // Text box
        _renderer.DrawFilledRect(BoxX, BoxY, BoxW, BoxH, 20, 20, 40, 230);
        _renderer.DrawRect(BoxX, BoxY, BoxW, BoxH, 180, 160, 80);

        if (dialogue.State == DialogueState.ShowingText && dialogue.CurrentPage != null)
        {
            var page = dialogue.CurrentPage;
            _renderer.DrawText($"[ {page.SpeakerName.ToUpper()} ]", TextX, SpeakerY);
            RenderWrappedAt3Lines(page.Text, TextX, Line1Y);
            _renderer.DrawText("[ E ] NEXT", BoxX + BoxW - 150, NextY);
        }
        else if (dialogue.State == DialogueState.ShowingChoices && dialogue.CurrentPage != null)
        {
            var page = dialogue.CurrentPage;
            _renderer.DrawText($"[ {page.SpeakerName.ToUpper()} ]", TextX, SpeakerY);
            RenderWrappedAt3Lines(page.Text, TextX, Line1Y);

            if (page.Choices.Count > 0)
                RenderSingleLine($"[1] {page.Choices[0].Text}", TextX, Choice1Y);
            if (page.Choices.Count > 1)
                RenderSingleLine($"[2] {page.Choices[1].Text}", TextX, Choice2Y);
        }
        else if (dialogue.State == DialogueState.ShowingResponse)
        {
            _renderer.DrawText($"[ {dialogue.ResponseSpeaker.ToUpper()} ]", TextX, SpeakerY);
            RenderWrappedAt3Lines(dialogue.ResponseText, TextX, Line1Y);
            _renderer.DrawText("[ E ] NEXT", BoxX + BoxW - 150, NextY);
        }
    }

    private void RenderWrappedAt3Lines(string text, int x, int startY)
    {
        var lines = WrapText(text, CharsPerLine);
        int[] lineYs = { startY, startY + 20, startY + 40 };

        for (int i = 0; i < Math.Min(lines.Count, 3); i++)
        {
            string line = lines[i];
            if (i == 2 && lines.Count > 3)
                line = line.Length > CharsPerLine - 3
                    ? line[..(CharsPerLine - 3)] + "..."
                    : line + "...";
            _renderer.DrawText(line.ToUpper(), x, lineYs[i]);
        }
    }

    private void RenderSingleLine(string text, int x, int y)
    {
        string line = text.Length > CharsPerLine
            ? text[..(CharsPerLine - 3)] + "..."
            : text;
        _renderer.DrawText(line.ToUpper(), x, y);
    }

    private List<string> WrapText(string text, int charsPerLine)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var current = "";

        foreach (var word in words)
        {
            if ((current + word).Length > charsPerLine)
            {
                if (current.Length > 0) lines.Add(current.TrimEnd());
                current = word + " ";
            }
            else
            {
                current += word + " ";
            }
        }
        if (current.Length > 0) lines.Add(current.TrimEnd());
        return lines;
    }
}
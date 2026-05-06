using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
namespace TheAdventure.Core;

public class CombatRenderer
{
    private readonly GameRenderer _renderer;
    private readonly int _windowWidth = 1024;
    private readonly int _windowHeight = 800;

    private readonly int _playerAttackId;
    private readonly int _aldricAttackId;
    private readonly int _deathId;
    private readonly int _aldricIdleId;
    private readonly int _playerIdleId;

    private const int PlayerX = 150;
    private const int PlayerY = 350;
    private const int AldricX = 750;
    private const int AldricY = 350;
    private const int SpriteW = 240;
    private const int SpriteH = 160;

    public CombatRenderer(GameRenderer renderer)
    {
        _renderer = renderer;
        _playerAttackId = renderer.LoadTexture("Assets/_Attack.png", out _);
        _aldricAttackId = renderer.LoadTexture("Assets/_Attack2.png", out _);
        _deathId = renderer.LoadTexture("Assets/_Death.png", out _);
        _aldricIdleId = renderer.LoadTexture("Assets/_Idle2.png", out _);
        _playerIdleId = renderer.LoadTexture("Assets/_Idle.png", out _);
    }

    public void Render(CombatState state)
    {
        // Dark background
        _renderer.DrawFilledRect(0, 0, _windowWidth, _windowHeight, 10, 5, 20);

        // Health bars
        DrawHealthBar("HANDYMAN", state.PlayerHealth, 50, 50, false);
        DrawHealthBar("SIR ALDRIC", state.AldricHealth, _windowWidth - 350, 50, true);

        // Player sprite
        RenderPlayerSprite(state);

        // Aldric sprite
        RenderAldricSprite(state);

        // Taunt text box
        if (state.Phase == CombatPhase.PlayerTurn1 ||
            state.Phase == CombatPhase.PlayerTurn2 ||
            state.Phase == CombatPhase.AldricTaunt1 ||
            state.Phase == CombatPhase.AldricTaunt2)
        {
            _renderer.DrawFilledRect(20, 680, 984, 100, 20, 20, 40, 220);
            _renderer.DrawRect(20, 680, 984, 100, 180, 160, 80);
            _renderer.DrawText("[ SIR ALDRIC ]", 36, 692);
            _renderer.DrawText(state.TauntText, 36, 720);

            if (state.Phase == CombatPhase.PlayerTurn1 ||
                state.Phase == CombatPhase.PlayerTurn2)
                _renderer.DrawText("[ CLICK ] TO ATTACK", 36, 748);
        }

        // Fade out overlay
        if (state.Phase == CombatPhase.FadeOut || state.Phase == CombatPhase.Done)
        {
            byte alpha = (byte)Math.Min(255, (int)state.FadeAlpha);
            _renderer.DrawFilledRect(0, 0, _windowWidth, _windowHeight, 0, 0, 0, alpha);
        }
    }

    private void RenderPlayerSprite(CombatState state)
    {
        Rectangle<int> src;
        int textureId;
        int offsetX = 0;

        switch (state.Phase)
        {
            case CombatPhase.PlayerAttacking1:
            case CombatPhase.PlayerAttacking2:
                // Slide toward Aldric based on frame
                offsetX = state.AnimationFrame * 180;
                int attackFrame = Math.Min(state.AnimationFrame, 3);
                src = new Rectangle<int>(attackFrame * 120, 0, 120, 80);
                textureId = _playerAttackId;
                break;

            case CombatPhase.PlayerDying:
            case CombatPhase.FadeOut:
            case CombatPhase.Done:
                int deathFrame = Math.Min(state.AnimationFrame, 9);
                src = new Rectangle<int>(deathFrame * 120, 0, 120, 80);
                textureId = _deathId;
                break;

            default:
                src = new Rectangle<int>(0, 0, 120, 80);
                textureId = _playerIdleId;
                break;
        }

        var dst = new Rectangle<int>(PlayerX + offsetX, PlayerY, SpriteW, SpriteH);
        _renderer.RenderTexture(textureId, src, dst);
    }

    private void RenderAldricSprite(CombatState state)
    {
        Rectangle<int> src;
        int textureId;
        int offsetX = 0;

        if (state.Phase == CombatPhase.AldricAttacking)
        {
            // Slide toward player based on frame
            offsetX = -(state.AnimationFrame * 180);
            int frame = Math.Min(state.AnimationFrame, 5);
            src = new Rectangle<int>(frame * 120, 0, 120, 80);
            textureId = _aldricAttackId;
            var dst2 = new Rectangle<int>(AldricX + offsetX, AldricY, SpriteW, SpriteH);
            _renderer.RenderTexture(textureId, src, dst2, RendererFlip.Horizontal);
            return;
        }

        src = new Rectangle<int>(0, 0, 120, 80);
        textureId = _aldricIdleId;
        var dst = new Rectangle<int>(AldricX + offsetX, AldricY, SpriteW, SpriteH);
        _renderer.RenderTexture(textureId, src, dst, RendererFlip.Horizontal);
    }
    private void DrawHealthBar(string name, int health, int x, int y, bool rightAlign)
    {
        int barW = 300;
        int barH = 20;

        _renderer.DrawText(name, x, y);
        _renderer.DrawFilledRect(x, y + 20, barW, barH, 60, 0, 0);
        int filled = (int)(barW * (health / 100.0));
        _renderer.DrawFilledRect(x, y + 20, filled, barH, 200, 30, 30);
        _renderer.DrawRect(x, y + 20, barW, barH, 180, 160, 80);
    }
}
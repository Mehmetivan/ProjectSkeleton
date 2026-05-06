namespace TheAdventure.Core;
using TheAdventure.Models;
public enum GamePhase
{
    Intro,
    Exploring,
    Ending,
    Combat,
    Done
}

public class GameLogic : IDisposable
{
    private readonly GameRenderer _renderer;
    private readonly Dictionary<int, GameObject> _gameObjects = new();

    private readonly AudioManager _audio = new();
    private HandymanKnight? _player;
    private GameMap? _map;
    private Camera? _camera;

    private VideoPlayer? _videoPlayer;

    private bool _moveUp, _moveDown, _moveLeft, _moveRight;
    private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

    public GameState GameState { get; } = new();
    private readonly DialogueSystem _dialogue = new();
    private DialogueRenderer? _dialogueRenderer;
    private Npc? _activeNpc;
    private GamePhase _phase = GamePhase.Intro;

    private readonly List<Npc> _npcs = new();

    private readonly (int X, int Y) _castleEndPos = (837, 670);
    private readonly int _endingRange = 80;

    private CombatState? _combat;
    private CombatRenderer? _combatRenderer;

    public GameLogic(GameRenderer renderer)
    {
        _renderer = renderer;
    }

    public void InitializeGame()
    {

        _camera = new Camera(1024, 800);
        _map = new GameMap();
        _map.LoadMap(Path.Combine("Assets", "map.json"), _renderer);
        _camera.SetMapSize(_map.MapPixelWidth, _map.MapPixelHeight);

        int startX = 652;
        int startY = 417;

        _player = new HandymanKnight(startX, startY, _renderer);
        _gameObjects.Add(_player.Id, _player);
        _camera.Follow(startX, startY);

        _dialogueRenderer = new DialogueRenderer(_renderer);

        _npcs.Add(new Npc(256, 548, NpcType.Farmer));
        _npcs.Add(new Npc(625, 37, NpcType.Miner));
        _npcs.Add(new Npc(197, 782, NpcType.Turtle));

        _audio.PlayMusic(Path.Combine("Assets", "Secunda.mp3"));
        
        // TEMP: skip to video
        //_phase = GamePhase.Done;

        StartIntroDialogue();

        //testing last quest directly:
        // TEMP: complete all quests for testing
        //GameState.CompleteFarmerQuest();
        //GameState.CompleteMinerQuest();
        //GameState.CompleteTurtleQuest();
    }

    private void StartIntroDialogue()
    {
        var pages = new List<DialoguePage>
        {
            new DialoguePage("Sir Aldr!c",
                "Handyman! You dare call yourself a kn!ght of th!s castle? " +
                "You are a shame to the order."),
            new DialoguePage("Sir Aldr!c",
                "The counc!l has ruled. You are now stripped of your role " +
                "and cast out of these walls."),
            new DialoguePage("Sir Aldr!c",
                "However... the k!ng shows mercy. Prove your worth. " +
                "Go out !nto the realm and perform three honourable deeds."),
            new DialoguePage("Sir Aldr!c",
                "Return when done. Only then may you reclaim your honour " +
                "and your place among us. Now go!"),
            new DialoguePage("Handyman the Knight",
                "...! w!ll prove my worth. One way or another.")
        };

        _dialogue.StartDialogue(pages, () =>
        {
            _phase = GamePhase.Exploring;
            _dialogue.Reset();
        });
    }

    private void StartEndingDialogue()
    {
        _phase = GamePhase.Ending;

        List<DialoguePage> pages;

        if (GameState.IsGoodEnding)
        {
            pages = new List<DialoguePage>
            {
                new DialoguePage("Sir Aldr!c",
                    "You return...and word of your deeds has reached us before you. " +
                    "The farmer's young, the trapped m!ners, the turtle's eggs."),
                new DialoguePage("Sir Aldr!c",
                    "! never thought you had such worth, Handyman. Perhaps ! was wrong about you."),
                new DialoguePage("Sir Aldr!c",
                    "Nevertheless, custom must stand. To reclaim your honour, " +
                    "you must face me !n combat. Draw your sword."),
                new DialoguePage("Handyman the Knight",
                    "! am ready.")
            };
        }
        else
        {
            pages = new List<DialoguePage>
            {
                new DialoguePage("Sir Aldr!c",
                    "You dare return? ! have heard tales from the realm. " +
                    "Coward!ce. Greed. You dece!ved a turtle, Handyman. A TURTLE."),
                new DialoguePage("Sir Aldr!c",
                    "You are beyond hope. Truly beyond hope. " +
                    "Yet here you stand. F!ne. Custom !s custom."),
                new DialoguePage("Sir Aldr!c",
                    "Draw your sword, you wretched excuse for a kn!ght. " +
                    "Let us end th!s.")
            };
        }

        _dialogue.StartDialogue(pages, () =>
        {
            StartCombat();
            _dialogue.Reset();
        });
    }

    private void StartCombat()
    {
        _audio.PlayMusic(Path.Combine("Assets", "BossBattle.mp3"));
        _phase = GamePhase.Combat;
        _combat = new CombatState();
        _combatRenderer = new CombatRenderer(_renderer);
    }

    public void HandleCombatClick()
    {
        if (_combat == null || _phase != GamePhase.Combat) return;

        if (_combat.Phase == CombatPhase.PlayerTurn1)
        {
            _combat.Phase = CombatPhase.PlayerAttacking1;
            _combat.AnimationFrame = 0;
            _combat.AnimationTimer = 0;
        }
        else if (_combat.Phase == CombatPhase.PlayerTurn2)
        {
            _combat.Phase = CombatPhase.PlayerAttacking2;
            _combat.AnimationFrame = 0;
            _combat.AnimationTimer = 0;
        }
    }

    private void UpdateCombat(double timeSinceLastFrame)
    {
        if (_combat == null) return;

        _combat.AnimationTimer += timeSinceLastFrame;

        switch (_combat.Phase)
        {
            case CombatPhase.PlayerAttacking1:
                if (_combat.AnimationTimer > 150)
                {
                    _combat.AnimationTimer = 0;
                    _combat.AnimationFrame++;
                    if (_combat.AnimationFrame >= 4)
                    {
                        _combat.AldricHealth -= 15;
                        _combat.Phase = CombatPhase.AldricTaunt1;
                        _combat.TauntText = "NOT !MPRESSED. AGA!N!";
                        _combat.AnimationFrame = 0;
                        _combat.AnimationTimer = 0;
                    }
                }
                break;

            case CombatPhase.AldricTaunt1:
                if (_combat.AnimationTimer > 2500)
                {
                    _combat.Phase = CombatPhase.PlayerTurn2;
                    _combat.TauntText = "SHOW ME WHAT YOU GOT.";
                    _combat.AnimationTimer = 0;
                }
                break;

            case CombatPhase.PlayerAttacking2:
                if (_combat.AnimationTimer > 150)
                {
                    _combat.AnimationTimer = 0;
                    _combat.AnimationFrame++;
                    if (_combat.AnimationFrame >= 4)
                    {
                        _combat.AldricHealth -= 15;
                        _combat.Phase = CombatPhase.AldricTaunt2;
                        _combat.TauntText = "MY TURN.";
                        _combat.AnimationFrame = 0;
                        _combat.AnimationTimer = 0;
                    }
                }
                break;

            case CombatPhase.AldricTaunt2:
                if (_combat.AnimationTimer > 2500)
                {
                    _combat.Phase = CombatPhase.AldricAttacking;
                    _combat.AnimationFrame = 0;
                    _combat.AnimationTimer = 0;
                }
                break;

            case CombatPhase.AldricAttacking:
                if (_combat.AnimationTimer > 150)
                {
                    _combat.AnimationTimer = 0;
                    _combat.AnimationFrame++;
                    if (_combat.AnimationFrame >= 6)
                    {
                        _combat.PlayerHealth = 0;
                        _combat.Phase = CombatPhase.PlayerDying;
                        _combat.AnimationFrame = 0;
                        _combat.AnimationTimer = 0;
                    }
                }
                break;

            case CombatPhase.PlayerDying:
                if (_combat.AnimationTimer > 120)
                {
                    _combat.AnimationTimer = 0;
                    _combat.AnimationFrame++;
                    if (_combat.AnimationFrame >= 10)
                    {
                        _combat.Phase = CombatPhase.FadeOut;
                        _combat.AnimationFrame = 9;
                    }
                }
                break;

            case CombatPhase.FadeOut:
                _combat.FadeAlpha += timeSinceLastFrame * 0.3;
                if (_combat.FadeAlpha >= 255)
                {
                    _combat.FadeAlpha = 255;
                    _combat.Phase = CombatPhase.Done;
                    _phase = GamePhase.Done;
                }
                break;
        }
    }

    public void SetPlayerMovement(bool up, bool down, bool left, bool right)
    {
        if (_dialogue.IsActive) return;
        if (_phase == GamePhase.Combat) return;
        _moveUp = up;
        _moveDown = down;
        _moveLeft = left;
        _moveRight = right;
    }

    public void TryInteract()
    {
        if (_dialogue.IsActive)
        {
            _dialogue.Advance(GameState);
            if (!_dialogue.IsActive)
            {
                _activeNpc = null;
                _dialogue.Reset();
            }
            return;
        }

        if (_player == null) return;

        if (_phase == GamePhase.Exploring && GameState.AllQuestsDone)
        {
            var dx = _player.X - _castleEndPos.X;
            var dy = _player.Y - _castleEndPos.Y;
            if (Math.Sqrt(dx * dx + dy * dy) <= _endingRange)
            {
                StartEndingDialogue();
                return;
            }
        }

        if (_phase == GamePhase.Exploring)
        {
            foreach (var npc in _npcs)
            {
                if (!npc.QuestComplete && npc.IsPlayerNearby(_player.X, _player.Y))
                {
                    _activeNpc = npc;
                    StartNpcDialogue(npc);
                    return;
                }
            }
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (!_dialogue.IsActive) return;
        _dialogue.MakeChoice(choiceIndex, GameState);
        if (!_dialogue.IsActive)
        {
            if (_activeNpc != null)
                _activeNpc.QuestComplete = true;
            _activeNpc = null;
            _dialogue.Reset();
        }
    }

    private void StartNpcDialogue(Npc npc)
    {
        var pages = npc.Type switch
        {
            NpcType.Farmer => Quests.FarmerQuest(GameState),
            NpcType.Miner => Quests.MinerQuest(),
            NpcType.Turtle => Quests.TurtleQuest(),
            _ => new List<DialoguePage>()
        };

        _dialogue.StartDialogue(pages, () =>
        {
            npc.QuestComplete = true;
            _dialogue.Reset();
        });
    }

    public void RenderFrame()
    {
        var now = DateTimeOffset.Now;
        var timeSinceLastFrame = (now - _lastUpdate).TotalMilliseconds;
        _lastUpdate = now;

        _renderer.Clear();

        // Combat phase takes over entirely
        if (_phase == GamePhase.Combat && _combat != null)
        {
            UpdateCombat(timeSinceLastFrame);
            _combatRenderer?.Render(_combat);
            _renderer.Present();
            return;
        }

        // Game over screen
        if (_phase == GamePhase.Done)
        {
            if (_videoPlayer == null)
            {
                _audio.StopMusic();
                try
                {
                    _videoPlayer = new VideoPlayer(_renderer);
                    bool loaded = _videoPlayer.Load(Path.Combine("Assets", "skyrim_opening.mp4"));
                    if (loaded)
                    {
                        _audio.PlayMusic(Path.Combine("Assets", "skyrim_opening_audio.mp3"), false);
                        Console.WriteLine("Video loaded successfully");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Video failed to load");
                        _videoPlayer = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"VIDEO ERROR: {ex}");
                    _videoPlayer = null;
                }
            }

            if (_videoPlayer == null)
            {
                _renderer.DrawFilledRect(0, 0, 1024, 800, 0, 0, 0, 255);
                _renderer.DrawText("PRESS ESC TO EXIT", 370, 400);
                _renderer.Present();
                return;
            }

            _videoPlayer.Update(timeSinceLastFrame);
            _videoPlayer.Render(1024, 800);

            if (_videoPlayer.IsFinished)
            {
                _renderer.DrawFilledRect(0, 0, 1024, 800, 0, 0, 0, 255);
                _renderer.DrawText("PRESS ESC TO EXIT", 370, 400);
            }

            _renderer.Present();
            return;
        }
        if (_map != null && _camera != null && _player != null)
        {
            if (!_dialogue.IsActive && _phase == GamePhase.Exploring)
            {
                _player.UpdatePosition(
                    _moveUp, _moveDown, _moveLeft, _moveRight,
                    timeSinceLastFrame,
                    _map.CollisionMap,
                    _map.TileSize
                );
                _camera.Follow(_player.X, _player.Y);
            }

            _map.Render(_renderer, _camera);

            foreach (var npc in _npcs)
                npc.Render(_renderer, _camera);

            var toRemove = new List<int>();
            foreach (var obj in _gameObjects.Values)
            {
                if (obj is HandymanKnight knight)
                    knight.RenderWithCamera(_renderer, _camera);
                else if (obj is RenderableGameObject renderable)
                {
                    if (renderable.Update(timeSinceLastFrame))
                        renderable.Render(_renderer);
                    else
                        toRemove.Add(obj.Id);
                }
            }
            foreach (var id in toRemove)
                _gameObjects.Remove(id);

            if (!_dialogue.IsActive && _phase == GamePhase.Exploring)
            {
                foreach (var npc in _npcs)
                {
                    if (!npc.QuestComplete)
                    {
                        var (sx, sy) = _camera.WorldToScreen(npc.X, npc.Y - 5);
                        // Draw exclamation marker
                        _renderer.DrawFilledRect(sx - 4, sy - 20, 8, 16, 255, 200, 0);
                        _renderer.DrawFilledRect(sx - 4, sy - 2, 8, 8, 255, 200, 0);

                        if (npc.IsPlayerNearby(_player.X, _player.Y))
                            _renderer.DrawText("[E] TALK", sx - 30, sy - 22);
                    }
                }

                if (GameState.AllQuestsDone)
                {
                    var (sx, sy) = _camera.WorldToScreen(_castleEndPos.X, _castleEndPos.Y - 30);
                    // Draw marker for sir aldrich
                    _renderer.DrawFilledRect(sx - 15, sy - 20, 8, 16, 255, 200, 0);
                    _renderer.DrawFilledRect(sx - 15, sy - 2, 8, 8, 255, 200, 0);

                    var dx = _player.X - _castleEndPos.X;
                    var dy = _player.Y - _castleEndPos.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) <= _endingRange)
                        _renderer.DrawText("[E] PROVE YOURSELF", sx - 140, sy - 60);
                }
            }

            _renderer.DrawText($"HONOUR:{GameState.Honour}  COINS:{GameState.Coins}", 10, 20);

            if (_phase == GamePhase.Exploring)
            {
                int done = (GameState.FarmerQuestDone ? 1 : 0) +
                           (GameState.MinerQuestDone ? 1 : 0) +
                           (GameState.TurtleQuestDone ? 1 : 0);
                _renderer.DrawText($"DEEDS:{done}/3", 10, 40);

                if (GameState.AllQuestsDone)
                    _renderer.DrawText("ALL DEEDS DONE - GO TO SIR ALRIC", 150, 860);
            }

            if (_dialogue.IsActive)
            {
                string npcName = _activeNpc?.Name ?? "Sr Aldr!c";
                _dialogueRenderer?.Render(_dialogue, npcName);
            }
        }

        _renderer.Present();
    }
    public void Dispose()
    {
        _audio.Dispose();
        _videoPlayer?.Dispose();
    }    
}
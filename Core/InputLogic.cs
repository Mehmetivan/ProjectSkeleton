using TheAdventure.Input;

namespace TheAdventure.Core;

public class InputLogic
{
    private readonly Silk.NET.SDL.Sdl _sdl;
    private readonly GameLogic _gameLogic;

    private bool _eWasPressed = false;
    private bool _oneWasPressed = false;
    private bool _twoWasPressed = false;


    public InputLogic(Silk.NET.SDL.Sdl sdl, GameLogic gameLogic)
    {
        _sdl = sdl;
        _gameLogic = gameLogic;
    }

    public bool ProcessInput()
    {
        ReadOnlySpan<byte> keyboardState;
        unsafe
        {
            keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
        }

        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];
        var ev = new Silk.NET.SDL.Event();

        while (_sdl.PollEvent(ref ev) != 0)
        {
            if (ev.Type == (uint)Silk.NET.SDL.EventType.Quit) return true;

            switch (ev.Type)
            {
                case (uint)Silk.NET.SDL.EventType.Windowevent:
                    if (ev.Window.Event == (byte)Silk.NET.SDL.WindowEventID.TakeFocus)
                    {
                        unsafe
                        {
                            _sdl.SetWindowInputFocus(
                                _sdl.GetWindowFromID(ev.Window.WindowID)
                            );
                        }
                    }
                    break;

                case (uint)Silk.NET.SDL.EventType.Mousebuttondown:
                    mouseButtonStates[ev.Button.Button] = 1;
                    if (ev.Button.Button == (byte)MouseButton.Primary)
                         _gameLogic.HandleCombatClick();
                    break;

                case (uint)Silk.NET.SDL.EventType.Mousebuttonup:
                    mouseButtonStates[ev.Button.Button] = 0;
                    break;
            }
        }

        _gameLogic.SetPlayerMovement(
            keyboardState[(byte)KeyCode.W] > 0 || keyboardState[(byte)KeyCode.Up] > 0,
            keyboardState[(byte)KeyCode.S] > 0 || keyboardState[(byte)KeyCode.Down] > 0,
            keyboardState[(byte)KeyCode.A] > 0 || keyboardState[(byte)KeyCode.Left] > 0,
            keyboardState[(byte)KeyCode.D] > 0 || keyboardState[(byte)KeyCode.Right] > 0
        );

        bool ePressed = keyboardState[(byte)KeyCode.E] > 0;
        if (ePressed && !_eWasPressed)
            _gameLogic.TryInteract();
        _eWasPressed = ePressed;

        bool onePressed = keyboardState[(byte)KeyCode.One] > 0;
        if (onePressed && !_oneWasPressed)
            _gameLogic.MakeChoice(0);
        _oneWasPressed = onePressed;

        bool twoPressed = keyboardState[(byte)KeyCode.Two] > 0;
        if (twoPressed && !_twoWasPressed)
            _gameLogic.MakeChoice(1);
        _twoWasPressed = twoPressed;

        return false;
    }
}
using Silk.NET.SDL;
using TheAdventure.Core;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(
            Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents |
            Sdl.InitTimer | Sdl.InitGamecontroller | Sdl.InitJoystick
        );

        if (sdlInitResult < 0)
            throw new InvalidOperationException("Failed to initialize SDL.");

        // Renderer first, then logic that holds the renderer
        var gameWindow = new GameWindow(sdl);
        var gameRenderer = new GameRenderer(sdl, gameWindow);
        using var gameLogic = new GameLogic(gameRenderer);
        var inputLogic = new InputLogic(sdl, gameLogic);

        try
        {
            gameLogic.InitializeGame();

            bool running = true;
            while (running)
            {
                running = !inputLogic.ProcessInput();
                if (!running) break;
                gameLogic.RenderFrame();
                System.Threading.Thread.Sleep(13);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            Console.ReadLine();
        }

        gameWindow.Destroy();
        sdl.Quit();
    }
}

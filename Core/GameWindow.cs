using Silk.NET.SDL;

namespace TheAdventure.Core;

public class GameWindow
{
    private readonly Sdl _sdl;
    private readonly IntPtr _window;

    public GameWindow(Sdl sdl)
    {
        _sdl = sdl;
        unsafe
        {
            _window = (IntPtr)sdl.CreateWindow(
                "In Your Service",
                Sdl.WindowposUndefined, Sdl.WindowposUndefined,
                1024, 800,
                (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
            );
        }

        if (_window == IntPtr.Zero)
        {
            var ex = sdl.GetErrorAsException();
            if (ex != null) throw ex;
            throw new Exception("Failed to create window.");
        }
    }

    public IntPtr CreateRenderer()
    {
        IntPtr renderer;
        unsafe
        {
            renderer = (IntPtr)_sdl.CreateRenderer(
                (Window*)_window, -1, (uint)RendererFlags.Accelerated
            );
            _sdl.RenderSetVSync((Renderer*)renderer, 1);
        }

        if (renderer == IntPtr.Zero)
        {
            var ex = _sdl.GetErrorAsException();
            if (ex != null) throw ex;
            throw new Exception("Failed to create renderer.");
        }

        return renderer;
    }

    public void Destroy()
    {
        unsafe
        {
            _sdl.DestroyWindow((Window*)_window);
        }
    }
}

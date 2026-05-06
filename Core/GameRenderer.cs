using Silk.NET.SDL;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;

namespace TheAdventure.Core;

public class GameRenderer
{
    private readonly Sdl _sdl;
    private readonly IntPtr _renderer;

    private readonly Dictionary<int, IntPtr> _texturePointers = new();
    private readonly Dictionary<int, TextureData> _textureInformation = new();
    private int _textureIndex = 0;

    public GameRenderer(Sdl sdl, GameWindow gameWindow)
    {
        _sdl = sdl;
        _renderer = gameWindow.CreateRenderer();
    }


    public int LoadTexture(string fileName, out TextureData textureData)
    {
        using var fStream = new FileStream(
            fileName, FileMode.Open, FileAccess.Read, FileShare.Read
        );
        var image = Image.Load<Rgba32>(fStream);

        textureData = new TextureData()
        {
            Width = image.Width,
            Height = image.Height
        };

        var imageRawData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(imageRawData.AsSpan());

        unsafe
        {
            fixed (byte* data = imageRawData)
            {
                var imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(
                    data,
                    image.Width,
                    image.Height,
                    8,
                    image.Width * 4,
                    (uint)PixelFormatEnum.Rgba32
                );

                var imageTexture = _sdl.CreateTextureFromSurface(
                    (Renderer*)_renderer,
                    imageSurface
                );

                _sdl.FreeSurface(imageSurface);
                _texturePointers[_textureIndex] = (IntPtr)imageTexture;
            }
        }

        _textureInformation[_textureIndex] = textureData;
        return _textureIndex++;
    }

    public IntPtr GetTexturePointer(int id)
    {
        _texturePointers.TryGetValue(id, out var ptr);
        return ptr;
    }


    public void Clear()
    {
        unsafe
        {
            _sdl.SetRenderDrawColor((Renderer*)_renderer, 0, 0, 0, 255);
            _sdl.RenderClear((Renderer*)_renderer);
        }
    }

    public void Present()
    {
        unsafe
        {
            _sdl.RenderPresent((Renderer*)_renderer);
        }
    }


    public void RenderTexture(
        int textureId,
        Rectangle<int> src,
        Rectangle<int> dst,
        RendererFlip flip = RendererFlip.None)
    {
        unsafe
        {
            if (_texturePointers.TryGetValue(textureId, out var texturePointer))
            {
                _sdl.RenderCopyEx(
                    (Renderer*)_renderer,
                    (Texture*)texturePointer,
                    src,
                    dst,
                    0,
                    new Silk.NET.SDL.Point(0, 0),
                    flip
                );
            }
        }
    }


    public void RenderTile(IntPtr texturePointer, Rectangle<int> src, Rectangle<int> dst)
    {
        unsafe
        {
            _sdl.RenderCopyEx(
                (Renderer*)_renderer,
                (Texture*)texturePointer,
                src,
                dst,
                0,
                new Silk.NET.SDL.Point(0, 0),
                RendererFlip.None
            );
        }
    }
    public void DrawFilledRect(int x, int y, int width, int height,
        byte r, byte g, byte b, byte a = 255)
    {
        unsafe
        {
            var renderer = (Renderer*)_renderer;
            _sdl.SetRenderDrawBlendMode(renderer, BlendMode.Blend);
            _sdl.SetRenderDrawColor(renderer, r, g, b, a);
            var rect = new Silk.NET.Maths.Rectangle<int>(x, y, width, height);
            _sdl.RenderFillRect(renderer, rect);
        }
    }

    public void DrawRect(int x, int y, int width, int height,
        byte r, byte g, byte b, byte a = 255)
    {
        unsafe
        {
            var renderer = (Renderer*)_renderer;
            _sdl.SetRenderDrawBlendMode(renderer, BlendMode.Blend);
            _sdl.SetRenderDrawColor(renderer, r, g, b, a);
            _sdl.RenderDrawLine(renderer, x, y, x + width, y);
            _sdl.RenderDrawLine(renderer, x, y + height, x + width, y + height);
            _sdl.RenderDrawLine(renderer, x, y, x, y + height);
            _sdl.RenderDrawLine(renderer, x + width, y, x + width, y + height);
        }
    }

    public void DrawText(string text, int x, int y, byte r = 255, byte g = 255, byte b = 255)
    {
        int scale = 2;
        int cursorX = x;

        foreach (char c in text)
        {
            if (_font.TryGetValue(char.ToUpper(c), out var rows))
            {
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        if ((rows[row] & (1 << (7 - col))) != 0)
                        {
                            DrawFilledRect(
                                cursorX + col * scale,
                                y + row * scale,
                                scale, scale,
                                r, g, b
                            );
                        }
                    }
                    
                }
            }
            cursorX += (8 + 1) * scale;
        }
    }
    
    public int LoadTextureFromRawData(byte[] data, int width, int height)
    {
        unsafe
        {
            fixed (byte* ptr = data)
            {
                var surface = _sdl.CreateRGBSurfaceWithFormatFrom(
                    ptr, width, height, 8, width * 4,
                    (uint)PixelFormatEnum.Rgba32
                );
                var texture = _sdl.CreateTextureFromSurface(
                    (Renderer*)_renderer, surface
                );
                _sdl.FreeSurface(surface);
                _texturePointers[_textureIndex] = (IntPtr)texture;
                _textureInformation[_textureIndex] = new TextureData { Width = width, Height = height };
                return _textureIndex++;
            }
        }
    }
    private static readonly Dictionary<char, int[]> _font = new()
    {
        ['A'] = new[] { 0x18, 0x24, 0x42, 0x7E, 0x42, 0x42, 0x42, 0x00 },
        ['B'] = new[] { 0x7C, 0x42, 0x42, 0x7C, 0x42, 0x42, 0x7C, 0x00 },
        ['C'] = new[] { 0x3C, 0x42, 0x40, 0x40, 0x40, 0x42, 0x3C, 0x00 },
        ['D'] = new[] { 0x78, 0x44, 0x42, 0x42, 0x42, 0x44, 0x78, 0x00 },
        ['E'] = new[] { 0x7E, 0x40, 0x40, 0x7C, 0x40, 0x40, 0x7E, 0x00 },
        ['F'] = new[] { 0x7E, 0x40, 0x40, 0x7C, 0x40, 0x40, 0x40, 0x00 },
        ['G'] = new[] { 0x3C, 0x42, 0x40, 0x4E, 0x42, 0x42, 0x3C, 0x00 },
        ['H'] = new[] { 0x42, 0x42, 0x42, 0x7E, 0x42, 0x42, 0x42, 0x00 },
        ['J'] = new[] { 0x0E, 0x04, 0x04, 0x04, 0x44, 0x44, 0x38, 0x00 },
        ['K'] = new[] { 0x42, 0x44, 0x48, 0x70, 0x48, 0x44, 0x42, 0x00 },
        ['L'] = new[] { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x7E, 0x00 },
        ['M'] = new[] { 0x42, 0x66, 0x5A, 0x42, 0x42, 0x42, 0x42, 0x00 },
        ['N'] = new[] { 0x42, 0x62, 0x52, 0x4A, 0x46, 0x42, 0x42, 0x00 },
        ['O'] = new[] { 0x3C, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x00 },
        ['P'] = new[] { 0x7C, 0x42, 0x42, 0x7C, 0x40, 0x40, 0x40, 0x00 },
        ['Q'] = new[] { 0x3C, 0x42, 0x42, 0x42, 0x4A, 0x44, 0x3A, 0x00 },
        ['R'] = new[] { 0x7C, 0x42, 0x42, 0x7C, 0x48, 0x44, 0x42, 0x00 },
        ['S'] = new[] { 0x3C, 0x42, 0x40, 0x3C, 0x02, 0x42, 0x3C, 0x00 },
        ['T'] = new[] { 0x7E, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x00 },
        ['U'] = new[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x00 },
        ['V'] = new[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x24, 0x18, 0x00 },
        ['W'] = new[] { 0x42, 0x42, 0x42, 0x42, 0x5A, 0x66, 0x42, 0x00 },
        ['X'] = new[] { 0x42, 0x24, 0x18, 0x18, 0x18, 0x24, 0x42, 0x00 },
        ['Y'] = new[] { 0x42, 0x42, 0x24, 0x18, 0x18, 0x18, 0x18, 0x00 },
        ['I'] = new[] { 0xFF, 0xFF, 0x18, 0x18, 0x18, 0xFF, 0xFF, 0x00 },
        ['Z'] = new[] { 0x7E, 0x04, 0x08, 0x18, 0x20, 0x40, 0x7E, 0x00 },
        ['0'] = new[] { 0x3C, 0x42, 0x46, 0x4A, 0x52, 0x62, 0x3C, 0x00 },
        ['1'] = new[] { 0x18, 0x28, 0x08, 0x08, 0x08, 0x08, 0x3E, 0x00 },
        ['2'] = new[] { 0x3C, 0x42, 0x02, 0x0C, 0x30, 0x40, 0x7E, 0x00 },
        ['3'] = new[] { 0x3C, 0x42, 0x02, 0x1C, 0x02, 0x42, 0x3C, 0x00 },
        ['4'] = new[] { 0x08, 0x18, 0x28, 0x48, 0x7E, 0x08, 0x08, 0x00 },
        ['5'] = new[] { 0x7E, 0x40, 0x7C, 0x02, 0x02, 0x42, 0x3C, 0x00 },
        ['6'] = new[] { 0x1C, 0x20, 0x40, 0x7C, 0x42, 0x42, 0x3C, 0x00 },
        ['7'] = new[] { 0x7E, 0x02, 0x04, 0x08, 0x10, 0x20, 0x20, 0x00 },
        ['8'] = new[] { 0x3C, 0x42, 0x42, 0x3C, 0x42, 0x42, 0x3C, 0x00 },
        ['9'] = new[] { 0x3C, 0x42, 0x42, 0x3E, 0x02, 0x04, 0x38, 0x00 },
        [' '] = new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
        ['.'] = new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x18, 0x00 },
        [','] = new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x08, 0x10 },
        ['!'] = new[] { 0x18, 0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x00 },
        ['?'] = new[] { 0x3C, 0x42, 0x04, 0x08, 0x08, 0x00, 0x08, 0x00 },
        ['\''] = new[] { 0x18, 0x18, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00 },
        ['-'] = new[] { 0x00, 0x00, 0x00, 0x7E, 0x00, 0x00, 0x00, 0x00 },
        [':'] = new[] { 0x00, 0x18, 0x18, 0x00, 0x18, 0x18, 0x00, 0x00 },
        ['['] = new[] { 0x3C, 0x20, 0x20, 0x20, 0x20, 0x20, 0x3C, 0x00 },
        [']'] = new[] { 0x3C, 0x04, 0x04, 0x04, 0x04, 0x04, 0x3C, 0x00 },
        ['/'] = new[] { 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x00 },
    };
    public void UpdateTexture(int textureId, byte[] data, int width, int height)
    {
        if (!_texturePointers.TryGetValue(textureId, out var texturePtr)) return;
        unsafe
        {
            fixed (byte* ptr = data)
            {
                _sdl.UpdateTexture(
                    (Texture*)texturePtr,
                    (Silk.NET.Maths.Rectangle<int>*)null,
                    ptr,
                    width * 4
                );
            }
        }
    }
    
}

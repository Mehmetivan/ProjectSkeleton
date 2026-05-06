using Silk.NET.Maths;
using TheAdventure.Core;

namespace TheAdventure.Models;

public class RenderableGameObject : GameObject
{
    private int _textureId;
    public int TextureId => _textureId;
    public Rectangle<int> TextureSource { get; set; }
    public Rectangle<int> TextureDestination { get; set; }
    public TextureData TextureInformation { get; }

    public RenderableGameObject(string fileName, GameRenderer renderer) : base()
    {
        _textureId = renderer.LoadTexture(fileName, out var textureData);
        TextureInformation = textureData;
        TextureSource = new Rectangle<int>(0, 0, textureData.Width, textureData.Height);
        TextureDestination = new Rectangle<int>(0, 0, textureData.Width, textureData.Height);
    }

    protected void SetActiveTexture(int textureId)
    {
        _textureId = textureId;
    }

    public virtual void Render(GameRenderer renderer)
    {
        renderer.RenderTexture(TextureId, TextureSource, TextureDestination);
    }

    public virtual bool Update(double timeSinceLastFrame)
    {
        return true;
    }
}

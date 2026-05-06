using Silk.NET.Maths;
using TheAdventure.Core;

namespace TheAdventure.Models;

public class AnimatedGameObject : RenderableGameObject
{
    private readonly int _numberOfFrames;
    private readonly int _numberOfColumns;
    private readonly int _numberOfRows;
    private readonly int _columnWidth;
    private readonly int _rowHeight;
    private readonly int _durationInSeconds;
    private readonly double _timePerFrame;

    private double _timeSinceAnimationStart = 0;
    private int _currentRow = 0;
    private int _currentColumn = 0;

    public AnimatedGameObject(
        string fileName,
        GameRenderer renderer,
        int durationInSeconds,
        int numberOfFrames,
        int numberOfColumns,
        int numberOfRows,
        int x, int y) : base(fileName, renderer)
    {
        _numberOfFrames = numberOfFrames;
        _numberOfColumns = numberOfColumns;
        _numberOfRows = numberOfRows;
        _durationInSeconds = durationInSeconds;
        _columnWidth = TextureInformation.Width / numberOfColumns;
        _rowHeight = TextureInformation.Height / numberOfRows;
        _timePerFrame = (durationInSeconds * 1000.0) / numberOfFrames;

        var halfColumn = _columnWidth / 2;
        var halfRow = _rowHeight / 2;

        TextureDestination = new Rectangle<int>(
            x - halfColumn, y - halfRow, _columnWidth, _rowHeight
        );
        TextureSource = new Rectangle<int>(0, 0, _columnWidth, _rowHeight);
    }

    public override bool Update(double timeSinceLastFrame)
    {
        _timeSinceAnimationStart += timeSinceLastFrame;

        if (_timeSinceAnimationStart > _durationInSeconds * 1000)
            return false;

        var currentFrame = _timeSinceAnimationStart / _timePerFrame;
        _currentRow = (int)(currentFrame / _numberOfColumns);
        _currentColumn = (int)(currentFrame % _numberOfColumns);

        TextureSource = new Rectangle<int>(
            _currentColumn * _columnWidth,
            _currentRow * _rowHeight,
            _columnWidth,
            _rowHeight
        );

        return true;
    }
}

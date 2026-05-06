using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Core;

namespace TheAdventure.Models;

public enum FacingDirection { Right, Left }

public class HandymanKnight : RenderableGameObject
{
    private readonly int _frameWidth;
    private readonly int _frameHeight;

    private double _timeSinceLastFrame = 0;
    private int _currentFrame = 0;

    private int _x;
    private int _y;

    private bool _isMoving = false;
    private bool _wasMoving = false;
    private FacingDirection _facing = FacingDirection.Right;
    private FacingDirection _previousFacing = FacingDirection.Right;
    private bool _isTurning = false;

    private readonly int _idleTextureId;
    private readonly int _runTextureId;
    private readonly int _turnTextureId;

    private readonly int _idleFrames = 10;
    private readonly int _runFrames = 10;
    private readonly int _turnFrames = 3;

    private readonly double _idleTimePerFrame = 100;
    private readonly double _runTimePerFrame = 80;
    private readonly double _turnTimePerFrame = 100;

    public int X => _x;
    public int Y => _y;

    public HandymanKnight(int x, int y, GameRenderer renderer)
        : base("Assets/_Idle.png", renderer)
    {
        _x = x;
        _y = y;

        _idleTextureId = TextureId;
        _runTextureId = renderer.LoadTexture("Assets/_Run.png", out _);
        _turnTextureId = renderer.LoadTexture("Assets/_TurnAround.png", out _);

        _frameWidth = TextureInformation.Width / _idleFrames;
        _frameHeight = TextureInformation.Height;

        TextureSource = new Rectangle<int>(0, 0, _frameWidth, _frameHeight);
        UpdateDestination(_x, _y);
    }

    public void UpdatePosition(
        bool moveUp, bool moveDown, bool moveLeft, bool moveRight,
        double timeSinceLastFrame,
        int[,] collisionMap,
        int tileSize)
    {
        double deltaSeconds = timeSinceLastFrame / 1000.0;
        int movement = (int)(100 * deltaSeconds);
        movement = Math.Max(1, movement);

        int newX = _x;
        int newY = _y;

        _isMoving = moveUp || moveDown || moveLeft || moveRight;

        if (moveLeft) _facing = FacingDirection.Left;
        else if (moveRight) _facing = FacingDirection.Right;

        if (_facing != _previousFacing && _isMoving && !_isTurning)
        {
            _isTurning = true;
            _currentFrame = 0;
            _timeSinceLastFrame = 0;
        }

        if (!_isTurning)
        {
            if (moveUp) newY -= movement;
            if (moveDown) newY += movement;
            if (moveLeft) newX -= movement;
            if (moveRight) newX += movement;

            if (CanMoveTo(newX, newY, collisionMap, tileSize))
            {
                _x = newX;
                _y = newY;
            }
        }

        if (_isMoving != _wasMoving)
        {
            _currentFrame = 0;
            _timeSinceLastFrame = 0;
            _wasMoving = _isMoving;
        }

        _previousFacing = _facing;
        UpdateAnimation(timeSinceLastFrame);
    }

    private bool CanMoveTo(int x, int y, int[,] collisionMap, int tileSize)
    {
        int feetX = x;
        int feetY = y + (_frameHeight / 2) - 4;
        int tileX = feetX / tileSize;
        int tileY = feetY / tileSize;

        if (tileX < 0 || tileY < 0 ||
            tileX >= collisionMap.GetLength(1) ||
            tileY >= collisionMap.GetLength(0))
            return false;

        return collisionMap[tileY, tileX] == 1;
    }

    private void UpdateAnimation(double timeSinceLastFrame)
    {
        _timeSinceLastFrame += timeSinceLastFrame;

        if (_isTurning)
        {
            if (_timeSinceLastFrame >= _turnTimePerFrame)
            {
                _timeSinceLastFrame = 0;
                _currentFrame++;
                if (_currentFrame >= _turnFrames)
                {
                    _isTurning = false;
                    _currentFrame = 0;
                }
            }
            SetActiveTexture(_turnTextureId);
        }
        else if (_isMoving)
        {
            if (_timeSinceLastFrame >= _runTimePerFrame)
            {
                _timeSinceLastFrame = 0;
                _currentFrame = (_currentFrame + 1) % _runFrames;
            }
            SetActiveTexture(_runTextureId);
        }
        else
        {
            if (_timeSinceLastFrame >= _idleTimePerFrame)
            {
                _timeSinceLastFrame = 0;
                _currentFrame = (_currentFrame + 1) % _idleFrames;
            }
            SetActiveTexture(_idleTextureId);
        }

        TextureSource = new Rectangle<int>(
            _currentFrame * _frameWidth, 0, _frameWidth, _frameHeight
        );
    }

    public void RenderWithCamera(GameRenderer renderer, Camera camera)
    {
        var (screenX, screenY) = camera.WorldToScreen(_x, _y);
        UpdateDestination(screenX, screenY);

        if (_facing == FacingDirection.Left)
            renderer.RenderTexture(TextureId, TextureSource, TextureDestination, RendererFlip.Horizontal);
        else
            renderer.RenderTexture(TextureId, TextureSource, TextureDestination);
    }

    private void UpdateDestination(int x, int y)
    {
        TextureDestination = new Rectangle<int>(
            x - _frameWidth / 2,
            y - _frameHeight / 2,
            _frameWidth,
            _frameHeight
        );
    }

    public override bool Update(double timeSinceLastFrame)
    {
        return true;
    }
}
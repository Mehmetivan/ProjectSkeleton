using NAudio.Wave;

namespace TheAdventure.Core;

public class AudioManager : IDisposable
{
    private AudioFileReader? _currentReader;
    private WaveOutEvent? _currentOutput;
    private string _currentTrack = "";

    public void PlayMusic(string filePath, bool loop = true)
    {
        if (_currentTrack == filePath) return;

        StopMusic();

        _currentTrack = filePath;
        _currentReader = new AudioFileReader(filePath);
        _currentOutput = new WaveOutEvent();

        if (loop)
        {
            var loopStream = new LoopStream(_currentReader);
            _currentOutput.Init(loopStream);
        }
        else
        {
            _currentOutput.Init(_currentReader);
        }

        _currentOutput.Play();
    }

    public void StopMusic()
    {
        _currentOutput?.Stop();
        _currentOutput?.Dispose();
        _currentReader?.Dispose();
        _currentOutput = null;
        _currentReader = null;
        _currentTrack = "";
    }

    public void SetVolume(float volume)
    {
        if (_currentOutput != null)
            _currentOutput.Volume = Math.Clamp(volume, 0f, 1f);
    }

    public void Dispose()
    {
        StopMusic();
    }
}

public class LoopStream : WaveStream
{
    private readonly WaveStream _source;

    public LoopStream(WaveStream source)
    {
        _source = source;
    }

    public override WaveFormat WaveFormat => _source.WaveFormat;
    public override long Length => long.MaxValue;
    public override long Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = _source.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0)
                _source.Position = 0; // loop back
            totalRead += read;
        }
        return totalRead;
    }
}
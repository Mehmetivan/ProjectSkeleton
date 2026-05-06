using FFmpeg.AutoGen;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Core;

public unsafe class VideoPlayer : IDisposable
{
    private readonly GameRenderer _renderer;
    private AVFormatContext* _formatCtx = null;
    private AVCodecContext* _codecCtx = null;
    private AVFrame* _frame = null;
    private AVFrame* _frameRgb = null;
    private SwsContext* _swsCtx = null;
    private int _videoStream = -1;

    private int _textureId = -1;
    private int _width;
    private int _height;

    private bool _isPlaying = false;
    private bool _isFinished = false;
    private double _frameTimer = 0;
    private double _frameDelay = 0;

    private bool _textureCreated = false;

    public bool IsFinished => _isFinished;

public VideoPlayer(GameRenderer renderer)
{
    _renderer = renderer;
    
    ffmpeg.RootPath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"
);
    FFmpeg.AutoGen.DynamicallyLoadedBindings.Initialize();
    
    ffmpeg.avformat_network_init();
}
    public bool Load(string filePath)
    {
        fixed (AVFormatContext** fmt = &_formatCtx)
        {
            if (ffmpeg.avformat_open_input(fmt, filePath, null, null) < 0)
                return false;
        }

        if (ffmpeg.avformat_find_stream_info(_formatCtx, null) < 0)
            return false;

        for (int i = 0; i < (int)_formatCtx->nb_streams; i++)
        {
            if (_formatCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                _videoStream = i;
                break;
            }
        }

        if (_videoStream == -1) return false;

        var codecPar = _formatCtx->streams[_videoStream]->codecpar;
        var codec = ffmpeg.avcodec_find_decoder(codecPar->codec_id);
        if (codec == null) return false;

        _codecCtx = ffmpeg.avcodec_alloc_context3(codec);
        ffmpeg.avcodec_parameters_to_context(_codecCtx, codecPar);
        ffmpeg.avcodec_open2(_codecCtx, codec, null);

        _width = _codecCtx->width;
        _height = _codecCtx->height;

        _frame = ffmpeg.av_frame_alloc();
        _frameRgb = ffmpeg.av_frame_alloc();

        var bufferSize = ffmpeg.av_image_get_buffer_size(
            AVPixelFormat.AV_PIX_FMT_RGBA, _width, _height, 1
        );
        var buffer = (byte*)ffmpeg.av_malloc((ulong)bufferSize);

        var dstData = new byte_ptrArray4();
        var dstLinesize = new int_array4();
        ffmpeg.av_image_fill_arrays(
            ref dstData, ref dstLinesize,
            buffer, AVPixelFormat.AV_PIX_FMT_RGBA,
            _width, _height, 1
        );
        _frameRgb->data[0] = dstData[0];
        _frameRgb->data[1] = dstData[1];
        _frameRgb->data[2] = dstData[2];
        _frameRgb->data[3] = dstData[3];
        _frameRgb->linesize[0] = dstLinesize[0];

        _swsCtx = ffmpeg.sws_getContext(
            _width, _height, _codecCtx->pix_fmt,
            _width, _height, AVPixelFormat.AV_PIX_FMT_RGBA,
            4, null, null, null
        );

        var timeBase = _formatCtx->streams[_videoStream]->avg_frame_rate;
        _frameDelay = timeBase.den > 0 ? 1000.0 / (timeBase.num / (double)timeBase.den) : 33.0;

        _isPlaying = true;
        return true;
    }

    public void Update(double timeSinceLastFrame)
    {
        if (!_isPlaying || _isFinished) return;

        _frameTimer += timeSinceLastFrame;
        if (_frameTimer < _frameDelay) return;
        _frameTimer = 0;

        var packet = ffmpeg.av_packet_alloc();
        try
        {
            bool gotFrame = false;
            while (!gotFrame)
            {
                int ret = ffmpeg.av_read_frame(_formatCtx, packet);
                if (ret < 0)
                {
                    _isFinished = true;
                    return;
                }

                if (packet->stream_index == _videoStream)
                {
                    ffmpeg.avcodec_send_packet(_codecCtx, packet);
                    if (ffmpeg.avcodec_receive_frame(_codecCtx, _frame) == 0)
                    {
                        ffmpeg.sws_scale(
                            _swsCtx,
                            _frame->data, _frame->linesize, 0, _height,
                            _frameRgb->data, _frameRgb->linesize
                        );
                        gotFrame = true;
                        UploadFrame();
                    }
                }
                ffmpeg.av_packet_unref(packet);
            }
        }
        finally
        {
            ffmpeg.av_packet_free(&packet);
        }
    }

    private void UploadFrame()
    {
        var data = new byte[_width * _height * 4];
        System.Runtime.InteropServices.Marshal.Copy(
            (IntPtr)_frameRgb->data[0], data, 0, data.Length
        );

        if (!_textureCreated)
        {
            _textureId = _renderer.LoadTextureFromRawData(data, _width, _height);
            _textureCreated = true;
        }
        else
        {
            _renderer.UpdateTexture(_textureId, data, _width, _height);
        }
    }

    public void Render(int windowWidth, int windowHeight)
    {
        if (_textureId < 0) return;

        var src = new Rectangle<int>(0, 0, _width, _height);
        var dst = new Rectangle<int>(0, 0, windowWidth, windowHeight);
        _renderer.RenderTexture(_textureId, src, dst);
    }

    public void Dispose()
    {
        if (_frame != null) { fixed (AVFrame** f = &_frame) ffmpeg.av_frame_free(f); }
        if (_frameRgb != null) { fixed (AVFrame** f = &_frameRgb) ffmpeg.av_frame_free(f); }
        if (_codecCtx != null) { fixed (AVCodecContext** c = &_codecCtx) ffmpeg.avcodec_free_context(c); }
        if (_formatCtx != null) { fixed (AVFormatContext** f = &_formatCtx) ffmpeg.avformat_close_input(f); }
        if (_swsCtx != null) ffmpeg.sws_freeContext(_swsCtx);
    }
}
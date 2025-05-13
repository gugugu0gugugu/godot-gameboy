using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Godot;

public partial class App : Control
{
    public readonly string TopMenuBarRootName = "VBoxContainer/TopMenuBar";
    private TopMenuBar _topMenuBarRoot;
    public readonly string FileDialogName = "FileDialog";
    private FileDialog _fileDialog;
    private ulong _last_frame_ticks;
    private Emulator _emulator;
    private DebugWindow _debugWindow;
    private bool _stop_on_entry;
    private string _rom_path;
    protected TextureRect _screen;
    protected Image _screen_image;
    protected ImageTexture _screen_image_tex;
    protected Thread _audio_thread;
    protected EmulatorInput _input;
    protected AudioStreamGeneratorPlayback _playback;
    protected AudioStreamGenerator _audio_stream;
    protected AudioStreamPlayer _player;
    protected float _sample_rate = 44100.0f;
    protected bool _exit;


    public override void _Ready()
    {
        base._Ready();
        _topMenuBarRoot = GetNode<TopMenuBar>(TopMenuBarRootName);
        _fileDialog = _topMenuBarRoot.GetNode<FileDialog>("MenuBar/FileDialog");
        _debugWindow = GetNode<DebugWindow>("DebugWindow");
        _screen = GetNode<TextureRect>("VBoxContainer/Screen");
        _input = GetNode<EmulatorInput>("Input");
        _player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _audio_stream = (AudioStreamGenerator)_player.Stream;
        _player.MixTarget = AudioStreamPlayer.MixTargetEnum.Surround;
        _player.Play();

        _playback = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();
        _audio_thread = new Thread( () => 
            {
                while (true && !_exit);
                    //AudioPlayback();
            }
        );
    }
    public override void _Notification(int what)
    {
        base._Notification(what);
        _exit = true;
        if (what == NotificationWMCloseRequest && _emulator != null)
        {
            _emulator.Pause();
            _emulator.Close();
        }
    }
    public void AudioPlayback()
    {
        lock(_emulator.Apu._audio_buffer_lock)
        {
            int frames = _playback.GetFramesAvailable();
            int get_frames = 0;
            for (; get_frames < frames; get_frames++)
            {
                double timestamp = get_frames / (double)_audio_stream.MixRate;
                double sample_index = timestamp * 1048576.0;

                uint sample1_idx = (uint)Math.Floor(sample_index);
                uint sample2_idx = (uint)Math.Ceiling(sample_index);
                if (sample1_idx >= _emulator.Apu._audioBufferL.Size) break;
                if (sample2_idx >= _emulator.Apu._audioBufferR.Size) break;
                float sample1_l = _emulator.Apu._audioBufferL[(int)sample1_idx];
                float sample2_l = _emulator.Apu._audioBufferL[(int)sample2_idx];
                float sample1_r = _emulator.Apu._audioBufferR[(int)sample1_idx];
                float sample2_r = _emulator.Apu._audioBufferR[(int)sample2_idx];
                _playback.PushFrame(new Vector2(
                    Utils.Math.Lerp(sample1_l, sample2_l, (float)sample_index -  sample1_idx),
                    Utils.Math.Lerp(sample1_r, sample2_r, (float)sample_index -  sample1_idx)));
            }
            if (get_frames > 0)
            {
                double delta = get_frames / _audio_stream.MixRate;
                uint num_samples = (uint)(delta * 1048576.0);
                num_samples = (uint)Math.Min(num_samples, _emulator.Apu._audioBufferL.Size);
                _emulator.Apu._audioBufferL.RemoveFromStart(num_samples);
                _emulator.Apu._audioBufferR.RemoveFromStart(num_samples);

            }
        }
    }


    public App() 
    {
        Utils.CycleQueue<int> queue = new Utils.CycleQueue<int>(10);
        for (int i = 0; i < queue.Capacity; i++)
        {
            queue.Enqueue(i);
        }
        queue.RemoveFromStart(5);
        for (int i = 0; !queue.Empty; i++)
        {
            Console.Write(queue.Dequeue()  + " ");
        }
        _screen_image = new Image();
        _screen_image_tex = new ImageTexture();
        _exit = false;
    }

    public void NewEmulator(string path) {
        _rom_path = path;
        FileStream fs = System.IO.File.Open(path, FileMode.Open, System.IO.FileAccess.Read);
        fs.Seek(0x100 + 71, SeekOrigin.Begin);
        CartridgeType type = (CartridgeType)fs.ReadByte();
        fs.Close();
        Cartridge cartridge;
        switch (type)
        {
            case CartridgeType.ROM_ONLY:
                cartridge = new(path);
                break;
            case CartridgeType.MBC1:
            case CartridgeType.MBC1_RAM:
            case CartridgeType.MBC1_RAM_BATTERY:
                cartridge = new CartridgeMBC1(path);
                break;
            case CartridgeType.MBC2:
            case CartridgeType.MBC2_BATTERY:
                cartridge = new CartridgeMBC2(path);
                break;
            case CartridgeType.MBC3:
            case CartridgeType.MBC3_RAM:
            case CartridgeType.MBC3_RAM_BATTERY:
            case CartridgeType.MBC3_TIMER_BATTERY:
            case CartridgeType.MBC3_TIMER_RAM_BATTERY:
                cartridge = new CartridgeMBC3(path);
                break;
            default:
                throw new Exception("Unsupported cartridge type!");
        }
        if (cartridge.VerifyCartridge()) {
            cartridge.Header.Info();
        } else return;
        _emulator = new Emulator(cartridge);
        if (_stop_on_entry) _emulator.Pause();
        //_audio_thread.Start();
    }
    public void Reset() 
    {
        NewEmulator(_rom_path);
    }

    public void SetStopOnEntry(bool b)
    {
        _stop_on_entry = b;
    }
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        _input._UnhandledKeyInput(@event);
    }

    public void UpdateEmulatorInput(Emulator e)
    {
        e.JoyPad.Left = _input.LeftPressed;
        e.JoyPad.Right = _input.RightPressed;
        e.JoyPad.Up = _input.UpPressed;
        e.JoyPad.Down = _input.DownPressed;
        e.JoyPad.A = _input.APressed;
        e.JoyPad.B = _input.BPressed;
        e.JoyPad.Select = _input.SelectPressed;
        e.JoyPad.Start= _input.StartPressed;
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_emulator != null)
            UpdateEmulatorInput(_emulator);
        _emulator?.Update(delta);
        if (_emulator != null && !_emulator.Suspend) {
            AudioPlayback();
            DrawScreen();
            _debugWindow.CpuLog(ref _emulator);
            _debugWindow.SerialLog(ref _emulator);
            _debugWindow.TilesLog(ref _emulator);
        }
    }



    public void PopupSelectCartridgeFile(bool playing) {
        _stop_on_entry = !playing;
        _fileDialog.Popup();
    }
    public void PopupDebugWindow()
    {
        _debugWindow.Show();
    }
    public void DrawScreen()
    {
        if (_emulator != null)
        {
            if (((_emulator.Ppu.CurBackBuffer + 1) & 0x1) == 0)
            {
                _screen_image.SetData((int)Ppu.XRES, (int)Ppu.YRES, false, Image.Format.Rgba8, _emulator.Ppu.Buffer1);
            }
            else 
            {
                _screen_image.SetData((int)Ppu.XRES, (int)Ppu.YRES, false, Image.Format.Rgba8, _emulator.Ppu.Buffer2);
            }
            _screen_image_tex.SetImage(_screen_image);
            _screen.Texture = _screen_image_tex;
        }
    }

    public void CpuStepCallback()
    {
        int steps = 1600;
        while (steps-- > 0)
        {
            _emulator.Cpu.Step(_emulator);
        }
        _debugWindow.Update(ref _emulator);
        _debugWindow.SerialLog(ref _emulator);
        _debugWindow.CpuLog(ref _emulator);
        _debugWindow.TilesLog(ref _emulator);
        DrawScreen();
    }
}

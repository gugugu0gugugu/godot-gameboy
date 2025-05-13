using System;
using System.Collections;
using System.Configuration.Assemblies;
using System.Linq;
using System.Runtime.Loader;
using Godot;

public partial class Emulator {
    protected string _alias;
    protected Cartridge _cartridge;
    public bool Suspend;
    protected UInt64 _clock_cycles;
    public UInt64 ClockCycles
    {
        get { return _clock_cycles; }
    }
    protected float _clock_speed_scale;
    public Cpu Cpu;
    public Timer Timer;
    public Serial Serial;
    public Ppu Ppu;
    public JoyPad JoyPad;
    public Apu.Apu Apu;
    /// <summary>
    /// 8 kb
    /// </summary>
    protected byte[] _vram = new byte[8 * (1 << 10)];
    public ref byte[] VRam { get { return ref _vram; }}
    /// <summary>
    /// 8 kb
    /// </summary>
    protected byte[] _wram = new byte[8 * (1 << 10)];
    public byte[] WRam { get { return _wram; }}
    protected byte[] _hram = new byte[128];
    public byte[] HRam { get { return _hram; }}
    protected byte[] _oam = new byte[160];
    public byte[] Oam{ get { return _oam; }}
    public byte IntFlags;
    public byte IntEnableFlags;
    public const byte INT_VBLANK = 1;
    public const byte INT_LCD_STAT= 2;
    public const byte INT_TIMER= 4;
    public const byte INT_SERIAL = 8;
    public const byte INT_JOYPAD = 16;


    public Emulator(Cartridge cartridge) {
        _cartridge = cartridge;
        _clock_cycles = 0;
        Suspend = false;
        _clock_speed_scale = (float)1.0;
        Cpu = new Cpu();
        Cpu.Init();
        IntEnableFlags = 0;
        IntFlags = 0;

        Timer = new Timer();
        Timer.Init();

        Serial = new Serial();
        Serial.Init();

        Ppu = new Ppu();
        Ppu.Init();

        JoyPad = new JoyPad();
        JoyPad.Init();

        Apu = new Apu.Apu();
        Apu.Init();

        if (!Cartridge.IsMBC2Cart((byte)cartridge.Type)) 
        {
            switch (cartridge.Header.ram_size)
            {
                case 2: cartridge.CramSize = 8096;break;
                case 3: cartridge.CramSize = 32 << 10;break;
                case 4: cartridge.CramSize = 128 <<10;break;
                case 5: cartridge.CramSize = 64 << 10;break;
                default: break;
            }
        }
        if (cartridge.CramSize > 0)
        {
            cartridge.Cram = new byte[cartridge.CramSize];
            if (CartridgeHeader.IsBatteryCart(cartridge.Header.cartridge_type))
            {
                cartridge.LoadCartridgeRamData();
            }
        }
    
    }
    ~Emulator()
    {
    }

    public void Update(double delta) {
        JoyPad.Update(this);
        if (Cartridge.IsSupportCartTimer((byte)_cartridge.Type))
        {
            ((CartridgeMBC3)_cartridge).RTC.Update(delta);
        }
        UInt64 frame_cycles = Convert.ToUInt64(delta * Clock.ClockRate * _clock_speed_scale);
        UInt64 end_cycles = _clock_cycles + frame_cycles;

        while (_clock_cycles < end_cycles) {
            if (Suspend) break;
            Cpu.Step(this);
        }

    }

    public void Tick(UInt32 mcycles) {
        UInt32 tick_cycles = mcycles << 2;
        for (UInt32 i = 0; i < tick_cycles; i++) 
        {
            ++_clock_cycles;
            Timer.Tick(this);
            if ((_clock_cycles & 511) == 0) 
            {
                // Serial is sticked at 8192 HZ
                Serial.Tick(this);
            }
            Ppu.Tick(this);
            Apu.Tick(this);
        }
    }

    public byte BusRead(UInt16 addr) {
        if (addr <= 0x7fff) return _cartridge.ReadByte(addr);
        if (addr <= 0x9fff) return _vram[addr - 0x8000];
        if (addr <= 0xbfff) return _cartridge.ReadByte(addr);
        if (addr <= 0xdfff) return _wram[addr - 0xc000];
        if (addr >= 0xfe00 && addr <= 0xfe9f) return _oam[addr - 0xfe00];
        if (addr == 0xff00) return JoyPad.BusRead();
        if (addr >= 0xff01 && addr <= 0xff02)
        {
            return Serial.BusRead(addr);
        }
        if (addr >= 0xff04 && addr <= 0xff07)
        {
            return Timer.BusRead(addr);
        }
        if (addr == 0xFF0F) return (byte)(IntFlags | 0xE0);
        if (addr >= 0xFF10 && addr <= 0xFF3F) return Apu.BusRead(addr);
        if (addr >= 0xff40 && addr <= 0xff4b) return Ppu.BusRead(addr);
        if (addr >= 0xff80 && addr <= 0xfffe) return _hram[addr - 0xff80];
        if (addr == 0xffff) return (byte)(IntEnableFlags | 0xE0);
        Console.WriteLine("\x1b[31mError: Unsupported busread address {0:d}\x1b[0m", addr);
        return 0xff;
    }

    public void BusWrite(UInt16 addr, byte value) {
        if (addr <= 0x7fff) {
            _cartridge.WriteByte(addr, value); 
            return;
        }

        if (addr <= 0x9fff) {
            _vram[addr - 0x8000] = value;
            return;
        }

        if (addr <= 0xbfff) {
            _cartridge.WriteByte(addr, value);
            return ;
        }

        if (addr <= 0xdfff) {
            _wram[addr - 0xc000] = value;
            return;
        }
        if (addr >= 0xfe00 && addr <= 0xfe9f)
        {
            _oam[addr - 0xfe00] = value;
            return;
        }
        if (addr == 0xff00)
        {
            JoyPad.BusWrite(value);
            return;
        }
        if (addr >= 0xff01 && addr <= 0xff02) {
            Serial.BusWrite(addr, value);
            return;
        }
        if (addr >= 0xff04 && addr <= 0xff07) {
            Timer.BusWrite(addr, value);
            return;
        }

        if (addr == 0xff0f)
        {
            IntFlags = (byte)(value & 0x1F);
            return;
        }
        if (addr >= 0xFF10 && addr <= 0xFF3F)
        {
            Apu.BusWrite(addr, value);
            return;
        }
        if (addr >= 0xff40 && addr <= 0xff4b)
        {
            Ppu.BusWrite(addr, value);
            return;
        }


        if (addr >= 0xff80 && addr <= 0xfffe) {
            _hram[addr - 0xff80] = value;
            return;
        }

        if (addr == 0xffff) {
            IntEnableFlags = (byte)(value & 0x1f);
            return;
        }
        Console.WriteLine("Error: Unsupported BusWrite address {0:d}", addr);
        return;
    }


    public void Pause() {
        Suspend = true;
    }

    

    public void Close() {
        if (_cartridge.Cram != null)
        {
            if (CartridgeHeader.IsBatteryCart(_cartridge.Header.cartridge_type))
            {
                _cartridge.SaveCartridgeRamData();
            }
            _cartridge.Cram = null;
            _cartridge.CramSize = 0;
        }
        if (_cartridge.RomData != null)
        {
            _cartridge.RomData = null;
        }
    }
}

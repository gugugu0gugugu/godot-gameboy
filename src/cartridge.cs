using System;
using System.Data.SqlTypes;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using csharp_branch.src.ui;
using Godot;

interface ICard {
    public void LoadCartridge();
    public void UnloadCartridge();
}


public enum CartridgeType {
    ROM_ONLY = 0,
    MBC1 = 1,
    MBC1_RAM = 2,
    MBC1_RAM_BATTERY = 3,
    MBC2 = 5,
    MBC2_BATTERY = 6,
    ROM_RAM = 8,
    ROM_RAM_BATTERY = 9,
    MBC3_TIMER_BATTERY = 0x0F,
    MBC3_TIMER_RAM_BATTERY = 0x10,
    MBC3 = 0x11,
    MBC3_RAM = 0x12,
    MBC3_RAM_BATTERY = 0x13,
    MAX = CartridgeType.MBC3_RAM_BATTERY + 1,
}



public partial class Cartridge {
    public static readonly string[] ROM_TYPES_INFO = {
        "ROM_ONLY",
        "MBC1",
        "MBC1+RAM",
        "MBC1+RAM+BATTERY",
        "0x04 ???",
        "MBC2",
        "MBC2+BATTERY",
        "0x07 ???",
        "ROM+RAM 1",
        "ROM+RAM+BATTERY 1",
        "0x0A ???",
        "MMM01",
        "MMM01+RAM",
        "MMM01+RAM+BATTERY",
        "0x0E ???",
        "MBC3+TIMER+BATTERY",
        "MBC3+TIMER+RAM+BATTERY 2",
        "MBC3",
        "MBC3+RAM 2",
        "MBC3+RAM+BATTERY 2",
        "0x14 ???",
        "0x15 ???",
        "0x16 ???",
        "0x17 ???",
        "0x18 ???",
        "MBC5",
        "MBC5+RAM",
        "MBC5+RAM+BATTERY",
        "MBC5+RUMBLE",
        "MBC5+RUMBLE+RAM",
        "MBC5+RUMBLE+RAM+BATTERY",
        "0x1F ???",
        "MBC6",
        "0x21 ???",
        "MBC7+SENSOR+RUMBLE+RAM+BATTERY",
    };

    public static readonly string[] RAM_SIZE_INFO = {
        "0",
        "-",
        "8 KB (1 bank)",
        "32 KB (4 banks of 8KB each)",
        "128 KB (16 banks of 8KB each)",
        "64 KB (8 banks of 8KB each)"

    };

    public struct LicCodeInfo {
        public byte lic_coed;
        public string name;
        public LicCodeInfo(byte lic_coed, string name) {
            this.lic_coed = lic_coed ;
            this.name = name;
        }
    }

    protected byte[] _rom_data;
    protected CartridgeHeader _header;
    protected string _path;
    protected CartridgeType _cartridgeType;

    public CartridgeHeader Header {get => _header;}
    public CartridgeType Type {get => _cartridgeType;}
    protected byte _romBanksNum;
    protected byte[] _cram;
    protected uint _cram_size;
    public byte[] RomData 
    {
        get => _rom_data;
        set => _rom_data = value;
    }
    public uint RomSize
    {
        get 
        {
            if (_rom_data != null)
                return (uint)_rom_data.Length;
            return 0;
        }
    }

    public string Path
    {
        get => _path;
    }
    public byte[] Cram 
    {
        get => _cram;
        set => _cram = value;
    }
    public uint CramSize
    {
        get => _cram_size;
        set => _cram_size = value;
    }

    public Cartridge() 
    {
    }
    public Cartridge(string path) {
        _path = path;
        try {
            _rom_data =  new byte[new FileInfo(path).Length];
            FileStream fs = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read);
            fs.Read(_rom_data, 0, _rom_data.Length);
            _header = CartridgeHeader.FromRomData(ref _rom_data);
            _cartridgeType = (CartridgeType)_header.cartridge_type;
            _romBanksNum = (byte)((32 << _header.rom_size) >> 4);
        } catch (Exception e) {
            string[] btns = {"Ok"};
            OneMessageBox.Show("Error", e.Message, btns, null);
        }
    }

    public void OpenCartridge(string path) {
        _path = path;
    }

    public bool VerifyCartridge() {
        if (_rom_data == null || _header == null) {
            return false;
        }
        byte checksum = 0;
        for (ushort address = 0x0134; address <= 0x014C; ++address) {
            checksum = (byte)(checksum - _rom_data[address] - 1);
        }
        if (checksum != _header.checksum) {
            throw new Exception("Bad checksum!");
        }
        return true;
    }
    public virtual byte ReadByte(UInt16 offset) {
        if (offset <= 0x7fff) 
            return _rom_data[offset];
        if (offset >= 0xA000 && offset >= 0xBFFF && _cram != null)
            return _cram[offset - 0xA000];

        Console.WriteLine("\x1b[31m[Error]: Cartridge Read do not support address 0x{0:X4}\x1b[0m!", offset);
        return 0xff;
       
    }
    public virtual void WriteByte(UInt16 offset, byte value) {
        if (offset >= 0xA000 && offset <= 0xBFFF && _cram != null)
        {
            _cram[offset - 0xA000] = value;
            return;
        }
        Console.WriteLine("\x1b[31m[Error]: Cartridge Write do not support address 0x{0:X4}!\x1b[0m", offset);
        //throw new NotImplementedException("Cartridge Write not implement");
        //_rom_data[offset] = value;
    }
    public void LoadCartridgeRamData()
    {
        string savePath = _path.GetFile() + ".cram";
        FileStream fs = null;
        try 
        {
            fs = System.IO.File.Open(savePath, FileMode.Open, System.IO.FileAccess.Read);
            fs.Position = 0;
            fs.Read(_cram);
            if (IsSupportCartTimer((byte)Type))
            {
                int size = Marshal.SizeOf<RTC>();
                byte[] buffer = new byte[size];
                fs.Read(buffer);
                ((CartridgeMBC3)this).RTC = Utils.Converter.BytesToStruct<RTC>(buffer);

                buffer = new byte[sizeof(Int64)];
                fs.Read(buffer);
                Int64 savedTimeStamp = BitConverter.ToInt64(buffer, 0);

                if (!((CartridgeMBC3)this).RTC.Halted)
                {
                    Int64 delta = Utils.Time.GetCurrentUTCTimeStamp() - savedTimeStamp;
                    if (delta < 0) delta = 0;
                    ((CartridgeMBC3)this).RTC.Time += delta;
                    ((CartridgeMBC3)this).RTC.UpdateTimeRegisters();
                }
            }

            Console.WriteLine("[Info]: cartridge Ram data loaded from {0:G}", savePath);
        } catch (Exception err)
        {
            Console.WriteLine("\x1b[34m[Warnning]: can't load Ram data from {0:G}!\nReason: {1:G}\x1b[0m",
                savePath, err.Message
            );
        } finally 
        {
            fs?.Close();
        }
    }
    public void SaveCartridgeRamData()
    {
        if (_cram != null)
        {
            string savePath = _path.GetFile() + ".cram";
            FileStream fs = null;
            try 
            {
                fs = System.IO.File.Open(savePath, FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                fs.Position = 0;
                fs.Write(_cram);
                
                if (IsSupportCartTimer((byte)Type))
                {
                    fs.Write(Utils.Converter.StructToBytes(((CartridgeMBC3)this).RTC));
                    long seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
                    fs.Write(BitConverter.GetBytes(seconds));
                }
                Console.WriteLine("[Info]: Cartridge Ram data save to {0:G}", savePath);
                
            } 
            catch (Exception err)
            {
                Console.WriteLine("\x1b[34m[Warnning]: can't save Ram data to {0:G}!\nReason: {1:G}\x1b[0m",
                    savePath, err.Message
                );
            }
            finally {fs?.Close();}
            return;
        }
        Console.WriteLine("\x1b[34m[Warnning]: can't save Ram data to {0:G}!\nReason: {1:G}\x1b[0m",
            _path, "The path is not valid or cartridge ram do not init!"
        );

    }
    public static bool IsMBC1Cart(byte cartridgeType)
    {
        return cartridgeType >= 1 && cartridgeType <= 3;
    }
    public static bool IsMBC2Cart(byte cartridgeType)
    {
        return cartridgeType >= 5 && cartridgeType <= 6;
    }
    public static bool IsMBC3Cart(byte cartridgeType)
    {
        return cartridgeType >= 15 && cartridgeType <= 19;
    }
    public static bool IsSupportCartTimer(byte cartridgeType)
    {
        return cartridgeType == 15 || cartridgeType == 16;
    }

    public static string GetCartridgeTypeInfo(byte type_idx) {
        if (type_idx < ROM_TYPES_INFO.Length) {
            return ROM_TYPES_INFO[type_idx];
        }
        return "UNKNOWN";
    }
    public static string GetCartridgeRamSizeInfo(byte ram_idx) {
        if (ram_idx < RAM_SIZE_INFO.Length) {
            return RAM_SIZE_INFO[ram_idx];
        }
        return "UNKNOWN";
    }

    public static string GetCartridgeLicCodeInfo(byte lic_code_idx) {
        switch(lic_code_idx)
        {
            case 0x00 : return "None";
            case 0x01 : return "Nintendo R&D1";
            case 0x08 : return "Capcom";
            case 0x13 : return "Electronic Arts";
            case 0x18 : return "Hudson Soft";
            case 0x19 : return "b-ai";
            case 0x20 : return "kss";
            case 0x22 : return "pow";
            case 0x24 : return "PCM Complete";
            case 0x25 : return "san-x";
            case 0x28 : return "Kemco Japan";
            case 0x29 : return "seta";
            case 0x30 : return "Viacom";
            case 0x31 : return "Nintendo";
            case 0x32 : return "Bandai";
            case 0x33 : return "Ocean/Acclaim";
            case 0x34 : return "Konami";
            case 0x35 : return "Hector";
            case 0x37 : return "Taito";
            case 0x38 : return "Hudson";
            case 0x39 : return "Banpresto";
            case 0x41 : return "Ubi Soft";
            case 0x42 : return "Atlus";
            case 0x44 : return "Malibu";
            case 0x46 : return "angel";
            case 0x47 : return "Bullet-Proof";
            case 0x49 : return "irem";
            case 0x50 : return "Absolute";
            case 0x51 : return "Acclaim";
            case 0x52 : return "Activision";
            case 0x53 : return "American sammy";
            case 0x54 : return "Konami";
            case 0x55 : return "Hi tech entertainment";
            case 0x56 : return "LJN";
            case 0x57 : return "Matchbox";
            case 0x58 : return "Mattel";
            case 0x59 : return "Milton Bradley";
            case 0x60 : return "Titus";
            case 0x61 : return "Virgin";
            case 0x64 : return "LucasArts";
            case 0x67 : return "Ocean";
            case 0x69 : return "Electronic Arts";
            case 0x70 : return "Infogrames";
            case 0x71 : return "Interplay";
            case 0x72 : return "Broderbund";
            case 0x73 : return "sculptured";
            case 0x75 : return "sci";
            case 0x78 : return "THQ";
            case 0x79 : return "Accolade";
            case 0x80 : return "misawa";
            case 0x83 : return "lozc";
            case 0x86 : return "Tokuma Shoten Intermedia";
            case 0x87 : return "Tsukuda Original";
            case 0x91 : return "Chunsoft";
            case 0x92 : return "Video system";
            case 0x93 : return "Ocean/Acclaim";
            case 0x95 : return "Varie";
            case 0x96 : return "Yonezawa/s’pal";
            case 0x97 : return "Kaneko";
            case 0x99 : return "Pack in soft";
            case 0xA4 : return "Konami (Yu-Gi-Oh!)";
            default: break;
        }

        return "UNKNOWN";
    }
}

public partial class CartridgeMBC1 : Cartridge
{
    public const uint ROM0_SIZE = 16 * 1024;
    public const uint ROM1_SIZE = 16 * 1024;
    public const uint RAM_SIZE = 8 * 1024;
    /// <summary>
    /// Cartridge Ram is enabled for reading/writing
    /// </summary>
    protected bool _cram_enable;
    protected byte _bankingMode;
    protected byte _romBankId;
    protected byte _ramBankId;
    public CartridgeMBC1()
    {
        _cram_enable = false;
        _cram_size = 0;
        _romBankId = 1;
        _ramBankId = 0;
        _bankingMode = 0;
    }
    public CartridgeMBC1(string path): base(path)
    {
        _cram_enable = false;
        _cram_size = 0;
        _romBankId = 1;
        _ramBankId = 0;
        _bankingMode = 0;
    }
    public override byte ReadByte(ushort addr)
    {
        // rom0 area
        if (addr <= 0x3fff)
        {
            // 2MB cartridge and advanced mode
            if (_bankingMode > 0 && _romBanksNum > 32)
            {
                uint bankIdx = _ramBankId;
                uint bankOffset = (bankIdx << 5) * ROM0_SIZE;
                return _rom_data[bankOffset + addr];
            }
            // Not 2MB cartidge or advanced mode disabled
            else return _rom_data[addr];
        }
        // rom1 area
        if (addr >= 0x4000 && addr <= 0x7fff)
        {
            // 2MB cartridge and advanced mode
            if (_bankingMode > 0 && _romBanksNum > 32)
            {
                uint bankIdx =  (uint)(_romBankId + (_ramBankId << 5));
                uint bankOffset = bankIdx * ROM1_SIZE;
                return _rom_data[bankOffset + (addr - 0x4000)];
            }
            // Not 2MB cartidge or advanced mode disabled
            else
            {
                uint bankIdx = (uint)_romBankId;
                uint bankOffset = bankIdx * ROM1_SIZE;
                return _rom_data[bankOffset + addr - 0x4000];
            }
        }
        // Ram Area
        if (addr >= 0xA000 && addr <= 0xBFFF)
        {
            if (_cram != null) 
            {
                if (!_cram_enable) return 0xFF;
                // 2MB cartridge
                if (_romBanksNum <= 32)
                {
                    // advanced mode
                    if (_bankingMode > 0)
                    {
                        uint bankOffset = _ramBankId * RAM_SIZE;
                        return _cram[bankOffset + addr - 0xA000];
                    }
                    // default mode
                    else
                    {
                        return _cram[addr - 0xA000];
                    }
                }
                // Common cartridge 
                else
                {
                    return _cram[addr - 0xA000];
                }
            }
        }
        Console.WriteLine("\x1b[31m[Error]: Unsupported MBC1 cartridge read address 0x{0:X4}", addr);
        return 0xFF;
    }
    public override void WriteByte(UInt16 addr, byte value)
    {
        if (addr <= 0x1fff)
        {
            if (_cram != null)
            {
                if ((value & 0x0F) == 0x0A)
                {
                    _cram_enable = true;
                }
                else _cram_enable = false;
                return;
            }
        }
        if (addr >= 0x2000 && addr <= 0x3FFF)
        {
            _romBankId = (byte)(value & 0x1F);
            // Do not allow to remap rom0 area
            if (_romBankId == 0)
                _romBankId = 1;
            if (_romBanksNum <= 2)
            {
                _romBankId = (byte)(_romBankId & 0x01);
            }
            else if (_romBanksNum <= 4)
            {
                _romBankId = (byte)(_romBankId & 0x03);
            }
            else if (_romBanksNum <= 8)
            {
                _romBankId = (byte)(_romBankId & 0x07);
            }
            else if (_romBanksNum <= 16)
            {
                _romBankId = (byte)(_romBankId & 0x0f);
            }
            return;
        }
        // Set Ram bank id
        if (addr >= 0x4000 && addr <= 0x5fff)
        {
            // Default set Ram num to 0~3;
            _ramBankId = (byte)(value & 0x03);
            // 2MB cartridge
            if (_romBanksNum > 32)
            {
                if (_romBanksNum <= 64)
                {
                    _ramBankId &= 0x01;
                }
            }
            // Common cartridge
            else
            {
                // Ram bank id limited by Cartridge Ram
                if (_cram_size <= 8 * 1024)
                {
                    _ramBankId = 0;
                }
                else if (_cram_size <= 16 * 1024)
                {
                    _ramBankId &= 0x1;
                }
            }
            return;
        }
        if (addr >= 0x6000 && addr <= 0x7FFF)
        {
            if (_romBanksNum > 32 || _cram_size > 8 * 1024)
            {
                _bankingMode = (byte)(value & 0x1);
            }
            return;
        }
        // Cartridge Ram Write
        if (addr >= 0xA000 && addr <= 0xBFFF)
        {
            if (_cram != null)
            {
                if (!_cram_enable) return;
                // Common cartridge
                if (_romBanksNum <= 32)
                {
                    // Advanced Mode
                    if (_bankingMode > 0)
                    {
                        uint bankOffset = _ramBankId * RAM_SIZE;
                        if ((bankOffset + addr - 0xA000) < _cram_size)
                        {
                            _cram[bankOffset + addr - 0xA000] = value;
                        }
                        else throw new InvalidDataException("Ram offset is out of range of Cram!");
                    }
                    // Default Mode
                    else
                    {
                        _cram[addr - 0xA000] = value;
                    }
                }
                else
                {
                    _cram[addr - 0xA000] = value;
                }
                return;
            }
        }
        Console.WriteLine("\x1b[31m[Error]: Unsupported MBC1 cartridge write address 0x{0:X4}", addr);
    }
    
}
public partial class CartridgeMBC2 : Cartridge
{
    public const uint ROM0_SIZE = 16 * 1024;
    public const uint ROM1_SIZE = 16 * 1024;
    protected bool _cram_enable;
    protected byte _romBankId;
    
    public CartridgeMBC2()
    {
        _cram_enable = false;
        _romBankId = 1;
        _cram_size = 512;
    }
    public CartridgeMBC2(string path): base(path)
    {
        _cram_enable = false;
        _romBankId = 1;
        _cram_size = 512;
    }
    public override byte ReadByte(ushort offset)
    {
        // Rom0 Area
        if (offset <= 0x3FFF)
        {
            return _rom_data[offset];
        }
        // Rom1 Area
        if (offset >= 0x4000 && offset <= 0x7FFF)
        {
            uint bankIdx = _romBankId;
            uint bankOffset = bankIdx * ROM1_SIZE;
            return _rom_data[bankOffset + offset - 0x4000];
        }
        // Ram Area, since MBC2 Cart only supports 512 X4 Ram, we can't read/write high 4 bits, then 'bit-or' with 0xF0;
        if (offset >= 0xA000 && offset <= 0xBFFF)
        {
            if (!_cram_enable) return 0xFF;
            ushort dataOffset = (ushort)(offset - 0xA000);
            dataOffset &= 511;
            return (byte)((_cram[dataOffset] & 0x0F) | 0xF0);
        }
        Console.WriteLine("\x1b[31m[Error]: Unsupported MBC2 cartridge read address 0x{0:X4}", offset);
        return 0xFF;
    }
    public override void WriteByte(ushort offset, byte value)
    {
        if (offset <= 0x3FFF)
        {
            //  Bit 8 is set
            if ((offset & 0x100) > 0)
            {
                // There are 1~16 roms
                _romBankId = (byte)(value & 0x0F);
                if (_romBankId == 0)
                {
                    _romBankId = 1;
                }
                if (_romBanksNum <= 2)
                {
                    _romBankId = (byte)(_romBankId & 0x01);
                }
                else if (_romBanksNum <= 4)
                {
                    _romBankId = (byte)(_romBankId & 0x03);
                }
                else if (_romBanksNum <= 8)
                {
                    _romBankId = (byte)(_romBankId & 0x07);
                }
                return;
            }
            else
            {
                if (_cram != null)
                {
                    if (value == 0x0A)
                    {
                        _cram_enable = true;
                    } else _cram_enable = false;
                    return;
                }
            }
        }
        else if (offset >= 0xA000 && offset <= 0xBFFF)
        {
            if (!_cram_enable) return;
            ushort dataOffset = (ushort)(offset - 0xA000);
            dataOffset &= 511;
            _cram[dataOffset] = (byte)(value & 0x0F);
            return;
        }
        Console.WriteLine("\x1b[31m[Error]: Unsupported MBC2 cartridge write address 0x{0:X4}", offset);
    }
}


public partial class CartridgeMBC3 : Cartridge
{
    protected RTC _rtc;
    protected bool _cram_enable;
    protected byte _romBankId;
    protected byte _ramBankId;

    public ref RTC RTC
    {
        get => ref _rtc;
    }
    public CartridgeMBC3()
    {
        _cram_enable = false;
        _ramBankId = 0;
        _romBankId = 1;
    }
    public CartridgeMBC3(string path): base(path)
    {
        _cram_enable = false;
        _ramBankId = 0;
        _romBankId = 1;
    }
    public override byte ReadByte(ushort offset)
    {
        if (offset <= 0x3FFF)
        {
            return _rom_data[offset];
        }
        if (offset >= 0x4000 && offset <= 0x7FFF)
        {
            uint bankIdx = _romBankId;
            uint bankOffset = bankIdx * CartridgeMBC1.ROM1_SIZE;
            return _rom_data[bankOffset + offset - 0x4000];
        }
        if (offset >= 0xA000 && offset <= 0xBFFF)
        {
            if (_ramBankId <= 0x03)
            {
                if (_cram != null)
                {
                    if (!_cram_enable) return 0xFF;
                    uint bankOffset = _ramBankId * CartridgeMBC1.RAM_SIZE;
                    return _cram[bankOffset + offset - 0xA000];
                }
            }
            if (IsSupportCartTimer((byte)Type) && _ramBankId >= 0x08 && _ramBankId <= 0x0C)
            {
                switch (_ramBankId)
                {
                    case 0x08: return RTC.S;
                    case 0x09: return RTC.M;
                    case 0x0A: return RTC.H;
                    case 0x0B: return RTC.Dl;
                    case 0x0C: return RTC.Dh;
                }
            }
        }
        Console.WriteLine("\x1b[31m[Error]: Unsupported MBC3 cartridge read address 0x{0:X4}", offset);
        return 0xFF;
    }
    public override void WriteByte(ushort offset, byte value)
    {
        if (offset <= 0x1FFF)
        {
            if (value == 0x0A)
            {
                _cram_enable = true;
            }
            else
            {
                _cram_enable = false;
            }
            return;
        }
        if (offset >= 0x2000 && offset <= 0x3FFF)
        {
            // maxinum is 128
            _romBankId = (byte)(value & 0x7F);
            if (_romBankId == 0)
            {
                // can't map rom1 area to rom0
                _romBankId = 1;
            }
            return;
        }
        if (offset >= 0x4000 && offset <= 0x5FFF)
        {
            _ramBankId = value;
            return;
        }
        if (offset >= 0x6000 && offset <= 0x7FFF)
        {
            if (IsSupportCartTimer((byte)Type))
            {
                if (value == 0x1 && RTC.TimeLaching)
                {
                    RTC.Latch();
                }
                if (value == 0x00)
                {
                    RTC.TimeLaching = true;
                }
                else
                {
                    RTC.TimeLaching = false;
                }
                return;
            }
        }
        if (offset >= 0xA000 && offset <= 0xBFFF)
        {
            if (_ramBankId <= 0x03)
            {
                if (_cram != null)
                {
                    if (!_cram_enable) return;
                    uint bankOffset = _ramBankId * CartridgeMBC1.RAM_SIZE;
                    _cram[bankOffset + offset - 0xA000] = value;
                    return;
                }
            }
            if (IsSupportCartTimer((byte)Type) && _ramBankId >= 0x08 && _ramBankId <= 0x0C)
            {
                switch (_ramBankId)
                {
                    case 0x08: RTC.S = value; break;
                    case 0x09: RTC.M = value; break;
                    case 0x0A: RTC.H = value; break;
                    case 0x0B: RTC.Dl = value; break;
                    case 0x0C: RTC.Dh = value; break;
                }
                RTC.UpdateTimeStamp();
                return;
            }
        }
        Console.WriteLine("\x1b[31m[Error]: Unsupported MBC3 cartridge write address 0x{0:X4}", offset);

    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public partial class CartridgeHeader {
    /// <summary>
    /// 4 bytes
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public UInt32 _entry;
    /// <summary>
    /// 48 bytes
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48, ArraySubType = UnmanagedType.U1)]
    readonly byte[] _logo = {
        0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D,
        0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99,
        0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E
    };
    /// <summary>
    /// 16 bytes
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U1)]
    public char[] title = {};
    /// <summary>
    /// 2 bytes
    /// </summary>
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 _new_lic_code;

    [MarshalAs(UnmanagedType.U1)]
    public byte sgb_flag;
    [MarshalAs(UnmanagedType.U1)]
    public byte cartridge_type;
    [MarshalAs(UnmanagedType.U1)]
    public byte rom_size;
    [MarshalAs(UnmanagedType.U1)]
    public byte ram_size;
    [MarshalAs(UnmanagedType.U1)]
    public byte dest_code;
    [MarshalAs(UnmanagedType.U1)]
    public byte lic_code;
    [MarshalAs(UnmanagedType.U1)]
    public byte version;
    [MarshalAs(UnmanagedType.U1)]
    public byte checksum;
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 global_checksum;

    public void Info() {
        Console.WriteLine("[Info]: CartridgeHeader detail infomations");
        Console.WriteLine("[Info]: Title        : {0:G}", new string(title).Split('\0')[0]);
        Console.WriteLine("[Info]: Type         : {0:G} ({1})", cartridge_type, Cartridge.GetCartridgeTypeInfo(cartridge_type));
        Console.WriteLine("[Info]: ROM Size     : {0:G} KB", 32 << rom_size);
        Console.WriteLine("[Info]: RAM Size     : 0x{0:X4} ({1})", ram_size, Cartridge.GetCartridgeRamSizeInfo(ram_size));
        Console.WriteLine("[Info]: Lic Code     : 0x{0:X4} ({1})", lic_code, Cartridge.GetCartridgeLicCodeInfo(lic_code));
        Console.WriteLine("[Info]: Rom Ver      : {0:G}", version);
    }

    public static CartridgeHeader FromRomData(ref byte[] romdata) {
        return Marshal.PtrToStructure<CartridgeHeader>(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(romdata, 0x0100));
    }

    public static void RomDataSeekToHeaderBegin(ref FileStream fs) {
        fs.Seek(0x0100, SeekOrigin.Begin);
    }
    public static bool IsBatteryCart(byte cartridgeType)
    {
        return cartridgeType == 3 // MBC1+RAM+BATTERY
            || cartridgeType == 6 // MBC2+BATTERY
            || cartridgeType == 9 // ROM+RAM+BATTERY 1
            || cartridgeType == 13 // MMM01+RAM+BATTERY
            || cartridgeType == 15 // MBC3+TIMER+BATTERY
            || cartridgeType == 16 // MBC3+TIMER+RAM+BATTERY 2
            || cartridgeType == 19 // MBC3+RAM+BATTERY 2
            || cartridgeType == 27 // MBC5+RAM+BATTERY
            || cartridgeType == 30 // MBC5+RUMBLE+RAM+BATTERY
            || cartridgeType == 34; // MBC7+SENSOR+RUMBLE+RAM+BATTERY
    }
}


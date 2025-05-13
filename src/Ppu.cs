using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;

public enum PPUMode
{
    HBLANK = 0,
    VBLANK = 1,
    OAM_SCAN = 2,
    DRAWING = 3,
}

public enum PPUFetchState
{
    TILE = 0,
    DATA0 = 1,
    DATA1 = 2,
    IDLE = 3,
    PUSH = 4
}
public struct BGWPixel
{
    public byte Color;
    public byte Palette;
}


public struct OAM
{
    public byte Y;
    public byte X;
    public byte TileIdx;
    public byte Flags;
    public readonly byte DmgPalette { get {return (byte)((Flags >> 4) & 0x1); } }
    public readonly bool FlipX {get {return ((Flags >> 5) & 0x1) == 1;}}

    public readonly bool FlipY {get {return ((Flags >> 6) & 0x1) == 1;}}
    /// <summary>
    /// Override background/window or be override
    /// </summary>
    public readonly bool Override {get {return ((Flags >> 7) & 0x1) == 1;}}
}

public struct ObjPixel
{
    public byte Color;
    public byte Palette;
    public bool Override;
}


public partial class Ppu 
{
    public const uint TOTAL_LINES = 154;
    public const uint CYCLES_PER_LINE = 456;
    public const uint YRES = 144;
    public const uint XRES = 160;
    [MarshalAs(UnmanagedType.U1)]
    private byte _lcd_c;


    /// <summary>
    /// LCD Status, address is 0xff41
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    private byte _lcd_s;

    /// <summary>
    /// 0xff42
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte ScrollY;
    /// <summary>
    /// 0xff43
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte ScrollX;

    /// <summary>
    /// LY register , address at 0xff44. Update by Ppu, value is the scan line, and the range is 0-153 while the range 144-153 is VBlank.
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    private byte _ly;
    public byte LY 
    {
        get { return _ly; }
    }

    /// <summary>
    /// LY Compare, address at 0xff45.
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte LYC;

    /// <summary>
    /// 0xff46
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Dma;

    /// <summary>
    /// 0xff47 BG palette data
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Bgp;

    /// <summary>
    /// 0xff48 OBJ0 palette data
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Obp0;

    /// <summary>
    /// 0xff49 OBJ1 palette data
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Obp1;

    /// <summary>
    /// 0xff4A  window y position plus
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte WY;

    /// <summary>
    /// 0xff4B  window x position plus
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte WX;

    public bool DmaActive;
    public byte DmaOffset;
    public byte DmaDelay;


    private uint _line_circles;
    public byte[] Buffer1;
    public byte[] Buffer2;
    public byte CurBackBuffer;

    /// <summary>
    /// Queue of background/window pixels
    /// </summary>
    public List<BGWPixel> BgwQueue;
    public List<ObjPixel> ObjQueue;

    /// <summary>
    /// True fetching window, otherwise background
    /// </summary>
    public bool FetchWindow;
    public byte WindowLine;
    public PPUFetchState State;
    /// <summary>
    /// X position of next pixel fetching in screen
    /// </summary>
    public byte FetchX;
    public ushort BgwDataAddrOffset;
    public short TileBeginX;
    public byte[] BgwFetchedData;
    public byte PushX;
    public byte DrawX;
    ///  ------------------------------------------------------------------------------------------
    /// Spirit
    ///  ------------------------------------------------------------------------------------------
    public List<OAM> Sprites;
    public OAM[] FetchedSprites = new OAM[3];
    public byte FetchedSpritesNum;
    public byte[] FetchedSpritesData = new byte[6];
    public bool Enabled
    {
        get { return ((_lcd_c >> 7) & 0x1) == 1; }
    }
    public PPUMode Mode
    {
        get { return (PPUMode)(_lcd_s & 0x3);}
        set 
        { 
            _lcd_s &= 0xFC; // clear previous mode;
            _lcd_s |= (byte)value;
        }
    }
    public bool LycFlag
    {
        get { return ((_lcd_s >> 2) & 0x1) == 1;}
        private set
        {
            if (value)
                _lcd_s |= 0x4;
            else _lcd_s &= 0xfb;
        }
    }
    public bool HBlankIntEnabled
    {
         get {return ((_lcd_s >> 3) & 0x1) == 1;}
    }

    public bool VBlankIntEnabled
    {
         get {return ((_lcd_s >> 4) & 0x1) == 1;}
    }

    public bool OamIntEnabled
    {
         get {return ((_lcd_s >> 5) & 0x1) == 1;}
    }

    public bool LycIntEnabled
    {
         get {return ((_lcd_s >> 6) & 0x1) == 1;}
    }

    public bool BGWindowEnabled { get {return (_lcd_c & 0x1) == 1;} }
    public bool WindowEnabled{ get {return ((_lcd_c >> 5) & 0x1) == 1;}}
    public ushort BgMapArea {get {return ((_lcd_c >> 3) & 0x1) == 1 ? (ushort)0x9c00 : (ushort)0x9800;}}
    public ushort WindowMapArea {get {return ((_lcd_c >> 6) & 0x1) == 1 ? (ushort)0x9C00 : (ushort)0x9800;}}
    public uint BgwDataArea {get {return (uint)(((_lcd_c >> 4) & 0x1) == 1 ? 0x8000 : 0x8800); }}
    public bool WindowVisible
    {
        get {return WindowEnabled && WX <= 166 && WY < YRES;}
    }
    public bool IsPixelWindow(byte screenX, byte screenY)
    {
        return WindowVisible && (screenX + 7 >= WX) && (screenY >= WY);
    }
    public bool ObjEnabled 
    {
        get {return ((_lcd_c >> 1) & 0x1) == 1;}
    }
    public byte ObjHeight 
    {
        get {return (byte)(((_lcd_c >> 2) & 0x1) == 1 ? 16 : 8);}
    }

    public void Init()
    {
        _lcd_c = 0x91;
        _lcd_s = 0;
        ScrollX = 0;
        ScrollY = 0;
        _ly = 0;
        WindowLine = 0;
        LYC = 0;
        Dma = 0;
        Bgp = 0xFC;
        Obp0 = 0xFF;
        Obp1 = 0xFF;
        WY = 0;
        WX = 0;
        Mode = PPUMode.OAM_SCAN;
        DmaActive = false;
        DmaDelay = 0;
        DmaOffset = 0;
        _line_circles = 0;
        BgwQueue = new List<BGWPixel>();
        ObjQueue = new List<ObjPixel>();
        Sprites = new List<OAM>();

        CurBackBuffer = 0;
        Buffer1 = new byte[YRES * XRES << 2];
        Buffer2 = new byte[YRES * XRES << 2];
        BgwFetchedData = new byte[2];
    }

    public void Tick(Emulator e)
    {
        if ((e.ClockCycles & 3) == 0)
        {
            TickDma(e);
        }
        if (!Enabled) return;
        ++_line_circles;
        switch (Mode)
        {
            case PPUMode.OAM_SCAN:
                TickOamScan(e);break;
            case PPUMode.DRAWING:
                TickDrawing(e);break;
            case PPUMode.HBLANK:
                TickHBlank(e);break;
            case PPUMode.VBLANK:
                TickVBlank(e);break;
            default:
                throw new Exception("Invalid PpuMode");
        }
    }

    public byte BusRead(ushort addr)
    {
        if (addr >= 0xff40 && addr <= 0xff4B)
        {
            if (addr == 0xff40) return _lcd_c;
            if (addr == 0xff41) return _lcd_s;
            if (addr == 0xff42) return ScrollY;
            if (addr == 0xff43) return ScrollX;
            if (addr == 0xff44) return _ly;
            if (addr == 0xff45) return LYC;
            if (addr == 0xff46) return Dma;
            if (addr == 0xff47) return Bgp;
            if (addr == 0xff48) return Obp0;
            if (addr == 0xff49) return Obp1;
            if (addr == 0xff4A) return WY;
            if (addr == 0xff4B) return WX;
        }

        throw new Exception("Unsuppored Ppu.BusRead address");
    }

    public void BusWrite(ushort addr, byte value)
    {
        if (addr >= 0xff40 && addr <= 0xff4B)
        {
            if (addr == 0xff40 && Enabled && (((value >> 7) & 0x1) == 0))
            {
                _lcd_s &= 0x7c;
                _ly = 0;
                WindowLine = 0;
                _line_circles = 0;
            }
            if (addr == 0xff41)
            {
                // lower 3 bits are readonly
                _lcd_s = (byte)((_lcd_s & 0x07) | (value & 0xf8));
                return;
            }
            if (addr == 0xff44) return;
            if (addr == 0xff46)
            {
                DmaActive = true;
                DmaOffset = 0;
                DmaDelay = 1;
            }

            switch (addr) 
            {
                case 0xff40:
                    _lcd_c = value; break;
                case 0xff42:
                    ScrollY = value; break;
                case 0xff43:
                    ScrollX = value; break;
                case 0xff45: 
                    LYC = value; break;
                case 0xff46: 
                    Dma = value; break;
                case 0xff47: 
                    Bgp = value; break;
                case 0xff48: 
                    Obp0 = value; break;
                case 0xff49: 
                    Obp1 = value; break;
                case 0xff4A: 
                    WY = value; break;
                case 0xff4B: 
                    WX = value; break;
            }

        }
    }
    public void TickOamScan(Emulator e)
    {
        if (_line_circles >= 80)
        {
            Mode = PPUMode.DRAWING;
            FetchWindow = false;
            State = PPUFetchState.TILE;
            FetchX = 0;
            PushX = 0;
            DrawX = 0;
            //BgwQueue.Clear();
        }
        if (_line_circles == 1)
        {
            Sprites.Clear();
            Sprites.Capacity = 10;
            byte spriteHeight = ObjHeight;
            for (byte i = 0; i < 40; i++)
            {
                if (Sprites.Count >= 10)
                {
                    break;
                }
                byte curOamStart = (byte)(i * Marshal.SizeOf<OAM>());
                OAM curOam = new()
                {
                    Y = e.Oam[curOamStart],
                    X = e.Oam[curOamStart + 1],
                    TileIdx = e.Oam[curOamStart + 2],
                    Flags = e.Oam[curOamStart + 3]
                };

                if (curOam.Y <= (_ly + 16) && (curOam.Y + spriteHeight) > (_ly + 16))
                {
                    byte insertTo = 0;
                    for (int j = 0; j < Sprites.Count; j++)
                    {
                        if (Sprites[j].X > curOam.X) break;
                        insertTo++;
                    }
                    Sprites.Insert(insertTo, curOam);
                }
            }
            
        }
    }

    public void TickDrawing(Emulator e)
    {
        if ((_line_circles & 0x1) == 0)
        {
            switch (State)
            {
                case PPUFetchState.TILE:
                    FetcherGetTile(e); break;
                case PPUFetchState.DATA0:
                    FetcherGetData(e, 0); break;
                case PPUFetchState.DATA1:
                    FetcherGetData(e, 1); break;
                case PPUFetchState.IDLE:
                    State = PPUFetchState.PUSH; break;
                case PPUFetchState.PUSH:
                    FetcherPushPixels(); break;
            }
            if (DrawX >= XRES)
            {
                if (_line_circles >= 252 && _line_circles <= 369)
                {
                    Mode = PPUMode.HBLANK;
                    if (HBlankIntEnabled)
                    {
                        e.IntFlags |= Emulator.INT_LCD_STAT;
                    }
                    BgwQueue.Clear();
                    ObjQueue.Clear();
                } 
                else throw new Exception("Invalid Ppu Circles");
            }
        }
        LcdDrawPixel();
    }

    public void TickHBlank(Emulator e)
    {
        if (_line_circles >= CYCLES_PER_LINE)
        {
            IncLy(e);
            if (_ly >= YRES)
            {
                Mode = PPUMode.VBLANK;
                e.IntFlags |= Emulator.INT_VBLANK;
                if (VBlankIntEnabled)
                {
                    e.IntFlags |= Emulator.INT_LCD_STAT;
                }
                CurBackBuffer++;
                CurBackBuffer &= 1;
            }
            else
            {
                Mode = PPUMode.OAM_SCAN;
                if (OamIntEnabled)
                {
                    e.IntFlags |= Emulator.INT_LCD_STAT;
                }
            }
            _line_circles = 0;
        }
    }
    public void TickVBlank(Emulator e)
    {
        if (_line_circles >= CYCLES_PER_LINE)
        {
            IncLy(e);
            if (_ly >= TOTAL_LINES)
            {
                // update to next frame
                Mode = PPUMode.OAM_SCAN;
                _ly = 0;
                WindowLine = 0;
                if (OamIntEnabled)
                {
                    e.IntFlags |= Emulator.INT_LCD_STAT;
                }
            }
            _line_circles = 0;
        }
    }
    public void TickDma(Emulator e)
    {
        if (!DmaActive) return;
        if (DmaDelay> 0)
        {
            --DmaDelay;
            return;
        }
        e.Oam[DmaOffset] = e.BusRead((ushort)((Dma * 0x100) + DmaOffset));
        ++DmaOffset;
        DmaActive = DmaOffset < 0xa0;
    }
    

    public void FetcherGetTile(Emulator e)
    {
        if (BGWindowEnabled)
        {
            if (FetchWindow)
            {
                FetcherGetWindowTile(e);
            }
            else
            {
                FetcherGetBackgroundTile(e);
            }
        }
        else
        {
            TileBeginX = FetchX;
        }
        if (ObjEnabled)
        {
            FetcherGetSpriteTile(e);
        }
        State = PPUFetchState.DATA0;
        FetchX += 8;
    }
    public void FetcherGetData(Emulator e, byte dataIdx)
    {
        if (BGWindowEnabled)
        {
            BgwFetchedData[dataIdx] = e.BusRead((ushort)(BgwDataArea + BgwDataAddrOffset + dataIdx));
        }
        if (ObjEnabled)
        {
            FetcherGetSpriteData(e, dataIdx);
        }
        if (dataIdx == 0) State = PPUFetchState.DATA1;
        else State = PPUFetchState.IDLE;
    }
    public void FetcherPushPixels()
    {
        bool pushed = false;
        if (BgwQueue.Count < 8)
        {
            byte pushBegin = PushX;
            FetcherPushGbwPixels();
            byte pushEnd = PushX;
            FetcherPushSpritePixels(pushBegin, pushEnd);
            pushed = true;
        }
        if (pushed)
        {
            State = PPUFetchState.TILE;
        }
    }
    public void LcdDrawPixel()
    {
        if (BgwQueue.Count >= 8)
        {
            if (DrawX >= XRES) return;
            BGWPixel bgwPixel = BgwQueue[0];
            BgwQueue.RemoveAt(0);
            ObjPixel objPixel = ObjQueue[0];
            ObjQueue.RemoveAt(0);

            byte bgColor = ApplyPalette(bgwPixel.Color, bgwPixel.Palette);
            bool drawObj = (objPixel.Color > 0) && (!objPixel.Override || bgColor == 0);
            byte objColor = ApplyPalette(objPixel.Color, (byte)(objPixel.Palette & 0xFc));
            byte color = drawObj ? objColor : bgColor;
            switch (color)
            {
                case 0: SetPixel(DrawX, _ly, 153, 161, 120, 255);break;
                case 1: SetPixel(DrawX, _ly, 87, 93, 67, 255);break;
                case 2: SetPixel(DrawX, _ly, 42, 46, 32, 255);break;
                case 3: SetPixel(DrawX, _ly, 10, 10, 2, 255);break;
            }
            ++DrawX;
        }
    }

    public void FetcherGetBackgroundTile(Emulator e)
    {
        // The y position of the next pixel to fetch relative to 256x256 tile map origin.
        byte mapY =(byte)(_ly + ScrollY);
        // The x position of the next pixel to fetch relative to 256x256 tile map origin.
        byte mapX =(byte)(FetchX + ScrollX);
        // The address to read map index.
        // ((map_y / 8) * 32) : 32 bytes per row in tile maps.
        ushort addr = (ushort)(BgMapArea + (mapX >> 3) + ((mapY >> 3) << 5));
        byte tileIdx = e.BusRead(addr);
        if (BgwDataArea == 0x8800)
        {
            // If LCDC.4=0, then range 0x9000~0x97FF is mapped to [0, 127], and range 0x8800~0x8FFF is mapped to [128, 255].
            // We can achieve this by simply add 128 to the fetched data, which will overflow and reset the value if greater 
            // than 127.
            tileIdx += 128;
        }
        BgwDataAddrOffset = (ushort)((((ushort)tileIdx) << 4) + ((mapY % 8) << 1));
        int tileX = FetchX + ScrollX;
        tileX = ((tileX >> 3) << 3) - ScrollX;
        TileBeginX = (short)tileX;
    }
    public void FetcherGetWindowTile(Emulator e)
    {
        byte windowX = (byte)(FetchX  + 7 - WX);
        byte windowY = WindowLine;
        ushort windowAddr = (ushort)(WindowMapArea + (windowX >> 3) + ((windowY >> 3) << 5));
        byte tileIdx = e.BusRead(windowAddr);
        if (BgwDataArea == 0x8800)
        {
            tileIdx += 128;
        }
        BgwDataAddrOffset = (ushort)((((ushort)(tileIdx)) << 4) + ((windowY % 8) << 1));
        int tileX = FetchX - WX + 7;
        tileX = ((tileX >> 3) << 3) + WX - 7;
        TileBeginX = (short)tileX;
    }
    
    
    public void FetcherGetSpriteTile(Emulator e)
    {
        FetchedSpritesNum =  0;
        for (byte i = 0; i < Sprites.Count; i++)
        {
            int spX = Sprites[i].X - 8;
            if (((spX >= TileBeginX) && (spX < (TileBeginX + 8)))
            || ((spX + 7 >= TileBeginX) && (spX + 7 < (TileBeginX + 8))))
            {
                FetchedSprites[FetchedSpritesNum] = Sprites[i];
                ++FetchedSpritesNum;
            }

            if (FetchedSpritesNum >= 3) break;
        }
    }
    public void FetcherGetSpriteData(Emulator e, byte dataIdx)
    {
        for (byte i = 0; i < FetchedSpritesNum; i++)
        {
            byte ty = (byte)(_ly + 16 - FetchedSprites[i].Y);
            if (FetchedSprites[i].FlipY)
            {
                ty = (byte)(ObjHeight - 1 - ty);
            }
            byte tileIdx = FetchedSprites[i].TileIdx;
            if (ObjHeight == 16)
            {
                tileIdx &=0xFE;
            }
            FetchedSpritesData[(i << 1) + dataIdx] = e.BusRead((ushort)(0x8000 + (tileIdx << 4) + (ty << 1) + dataIdx));

        }
    }
    public void FetcherPushGbwPixels()
    {
        byte b1 = BgwFetchedData[0];
        byte b2 = BgwFetchedData[1];
        for (uint i = 0; i < 8; i++)
        {
            if (TileBeginX + i < 0)
                continue;

            // If this is a window pixel, we reset fetcher to fetch window and discard remaining pixels.
            if (!FetchWindow && IsPixelWindow(PushX, _ly))
            {
                FetchWindow = true;
                FetchX = PushX;
                break;
            }

            BGWPixel pixel;
            if (BGWindowEnabled)
            {
                byte b = (byte)(7 - i);
                byte l = (byte)((b1 >> b) & 0x1);
                byte h = (byte)(((b2 >> b) & 0x1) << 1);
                pixel.Color = (byte)(h | l);
                pixel.Palette = Bgp;
            }
            else
            {
                pixel.Color = 0;
                pixel.Palette = 0;
            }
            BgwQueue.Add(pixel);
            ++PushX;
        }
    }
    public void FetcherPushSpritePixels(byte pushBegin, byte pushEnd)
    {
        for (uint i = pushBegin; i < pushEnd; i++)
        {
            ObjPixel pixel;
            pixel.Color = 0;
            pixel.Palette = 0 ;
            pixel.Override = true;
            if (ObjEnabled)
            {
                for (byte s = 0; s < FetchedSpritesNum; s++)
                {
                    int spX = FetchedSprites[s].X - 8;
                    int offset = (int)i - spX;
                    /// this sprite does not cover this pixel
                    if (offset < 0 || offset > 7)
                        continue;
                    byte b1 = FetchedSpritesData[s << 1];
                    byte b2 = FetchedSpritesData[(s << 1) + 1];
                    byte b = (byte)(7 - offset);

                    if (FetchedSprites[s].FlipX)
                    {
                        b = (byte)offset;
                    }

                    byte l = (byte)((b1 >> b) & 0x1);
                    byte h = (byte)(((b2 >> b) & 0x1) << 1);
                    byte color = (byte)(h | l);

                    if (color == 0)
                    {
                        continue;
                    }

                    pixel.Color = color;
                    pixel.Palette = FetchedSprites[s].DmgPalette > 0 ? Obp1 : Obp0;
                    pixel.Override = FetchedSprites[s].Override;
                    break;
                }
            }
            ObjQueue.Add(pixel);
        }
    }

    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
    {
        if (x < 0 || x >= Ppu.XRES)
            throw new Exception("Lcd X out of range");
        if (y < 0 || y >= Ppu.YRES)
            throw new Exception("Lcd Y out of range");
        
        uint offset = (uint)((y * XRES + x) << 2);
        if (CurBackBuffer == 0)
        {
            Buffer1[offset] = r;
            Buffer1[offset + 1] = g;
            Buffer1[offset + 2] = b;
            Buffer1[offset + 3] = a;
        }
        else
        {
            Buffer2[offset] = r;
            Buffer2[offset + 1] = g;
            Buffer2[offset + 2] = b;
            Buffer2[offset + 3] = a;
        }
    }
    public void IncLy(Emulator e)
    {
        ///
        /// Greater than Window Y And Less than Total Y
        if (WindowVisible && _ly >= WY && (_ly < (WY + YRES)))
        {
            ++WindowLine;
        }
        ++_ly;
        if (_ly == LYC)
        {
            LycFlag = true;
            if (LycIntEnabled)
            {
                e.IntFlags |= Emulator.INT_LCD_STAT;
            }
        }
        else
        {
            LycFlag = false;
        }
    }

    public static byte ApplyPalette(byte color, byte palette)
    {
        switch (color)
        {
            case 0: color = (byte)(palette & 0x03); break;
            case 1: color = (byte)((palette >> 2) & 0x03); break;
            case 2: color = (byte)((palette >> 4) & 0x03); break;
            case 3: color = (byte)((palette >> 6) & 0x03); break;
        }
        return color;
    }
    
}

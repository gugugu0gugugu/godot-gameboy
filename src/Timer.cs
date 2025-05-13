using System;
using System.IO;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public partial struct Timer 
{
    /// <summary>
    /// 0xFF04
    /// Only the high 8-bit is accessible via bus, thus behaves like incrementing at 16384Hz (once per 256 clock cycles).
    /// Writing any value to this resets the value to 0.
    /// </summary>
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 Div;
    /// <summary>
    /// 0xFF05 Timer counter
    /// Triggers a INT_TIMER when overflows (exceeds 0xFF).
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Tima;
    /// <summary>
    /// 0xFF06 Timer modulo
    /// The value to reset TIMA to if overflows.
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Tma;
    /// <summary>
    /// 0xFF07 Timer control
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public byte Tac;

    public readonly byte ReadDiv() {
        return (byte)(Div >> 8);
    }
    public readonly byte ClockType()
    {
        return (byte)(Tac & 0x03);
    }
    public readonly bool TimaEnabled()
    {
        return ((Tac >> 2) & 0x1) == 1;
    }
    public void Init()
    {
        Div = 0xAC00;
        Tima = 0;
        Tma = 0;
        Tac = 0xF8;
    }
    public void Tick(Emulator e)
    {
        UInt16 prev_div = Div;
        ++Div;
        if (TimaEnabled())
        {
            bool tima_update =false;
            
            switch (ClockType())
            {
                // 4096 HZ
                case 0:
                    tima_update = ((prev_div & (1 << 9)) > 0) && ((Div & (1 << 9)) == 0);
                    break;
                // 262144 HZ
                case 1:
                    tima_update = ((prev_div & (1 << 3)) > 0) && ((Div & (1 << 3)) == 0);
                    break;
                // 65536 HZ
                case 2:
                    tima_update = ((prev_div & (1 << 5)) > 0) && ((Div & (1 << 5)) == 0);
                    break;
                // 16384 HZ
                case 3:
                    tima_update = ((prev_div & (1 << 7)) > 0) && ((Div & (1 << 7)) == 0);
                    break;
            }
            if (tima_update)
            {
                if (Tima == 0xFF)
                {
                    e.IntFlags |= Emulator.INT_TIMER;
                    Tima = Tma;
                }
                else ++Tima;
            }
        }
    }
    public readonly byte BusRead(UInt16 addr)
    {
        if (addr >= 0xff04 && addr <= 0xff07)
        {
            if (addr == 0xff04) return ReadDiv();
            if (addr == 0xff05) return Tima;
            if (addr == 0xff06) return Tma;
            if (addr == 0xff07) return Tac;
        }
        throw new InvalidDataException("Timer.BusRead expect address from 0xff04 to 0xff07");
    }
    public void BusWrite(UInt16 addr, byte v)
    {
        if (addr >= 0xff04 && addr <= 0xff07)
        {
            if (addr == 0xff04)
            {
                Div = 0;
                return;
            }
            if (addr == 0xff05)
            {
                Tima = v;
                return;
            }
            if (addr == 0xff06)
            {
                Tma = v;
                return;
            }
            if (addr == 0xff07)
            {
                Tac = (byte)(0xF8 | (v & 0x07));
                return;
            }
            
        }
        throw new InvalidDataException("Timer.BusRead expect address from 0xff04 to 0xff07");

    }

}

using System;

public struct JoyPad
{
    // Functional key
    public bool A;
    public bool B;
    public bool Select;
    public bool Start;

    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;
    public byte P1;


    public void Init()
    {
        P1 = 0xff;
    }

    public readonly byte GetKeyState()
    {
        byte v = 0xFF;
        if (((P1 >> 4) & 0x1) == 0)
        {
            if (Right) v &= 0xFE;
            if (Left) v &= 0xFD;
            if (Up) v &= 0xFB;
            if (Down) v &= 0xF7;
            v &= 0xEF;
        }
        if (((P1 >> 5) & 0x1) == 0)
        {
            if (A) v &= 0xFE;
            if (B) v &= 0xFD;
            if (Select) v &= 0xFB;
            if (Start) v &= 0xF7;
            v &= 0xDF;
        }
        return v;
    }

    public void Update(Emulator e)
    {
        byte v = GetKeyState();
        if ((((P1 & 0x1) == 1) && ((v & 0x1) == 0))
        || ((((P1 >> 1) & 0x1) == 1) && (((v >> 1) & 0x1) == 0))
        || ((((P1 >> 2) & 0x1) == 1) && (((v >> 2) & 0x1) == 0))
        || ((((P1 >> 3) & 0x1) == 1) && (((v >> 3) & 0x1) == 0)))
        {
            e.IntFlags |= Emulator.INT_JOYPAD;
        }
        P1 = v;
    }

    public byte BusRead()
    {
        return P1;
    }
    public void BusWrite(byte v)
    {
        P1 = (byte)((v & 0x30) | (P1 & 0xCF));
        P1 = GetKeyState();
    }

}

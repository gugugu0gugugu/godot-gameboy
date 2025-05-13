using System.Data;

public struct RTC
{
    public byte S;
    public byte M;
    public byte H;
    public byte Dl; // lower 8 bits of Day record
    public byte Dh; // high 1 bits of Day record
    public double Time;
    public bool TimeLatched;
    public bool TimeLaching;

    public readonly bool Halted
    {
        get { return ((Dh >> 6) & 0x1) == 1;}
    }
    public readonly ushort Days
    {
        get {return (ushort)(Dl + ((Dh & 0x01) << 8));}
    }
    public readonly bool DayOverflow
    {
        get {return ((Dh >> 7) & 0x1) == 1;}
    }
    

    public void Init()
    {
        S = 0;
        M = 0;
        H = 0;
        Dl = 0;
        Dh = 0;
        Time  = 0;
        TimeLaching = false;
        TimeLatched = false;
    }
    public void Update(double delta)
    {
        if (!Halted)
        {
            Time += delta;
            if (!TimeLatched)
            {
                UpdateTimeRegisters();
            }
        }

    }
    public void UpdateTimeRegisters()
    {
        S = (byte)((uint)Time % 60);
        M = (byte)((uint)Time / 60 % 60);
        H = (byte)((uint)Time / 3600 % 24);
        ushort days = (ushort)((uint)Time / 86400);
        Dl = (byte)(days & 0xFF);
        if ((days & 0x100) > 0)
            Dh |= 0x1;
        else
            Dh &= 0xFE;

        if (days >= 512)
            Dh |= 0x80;
        else
            Dh &= 0x7F;
    }
    public void UpdateTimeStamp()
    {
        Time = S + M * 60 + H * 3600 + Days * 86400;
        if (DayOverflow)
        {
            Time += 86400 << 9;
        }
    }
    public void Latch()
    {
        if (!TimeLatched)
        {
            TimeLatched = true;
        }
        else
        {
            TimeLatched = false;
            UpdateTimeRegisters();
        }
    }
}


using System;
public partial class Clock {
    public const UInt64 PerSecondOfTicks = (ulong)1e7;
    public const double ClockRate = 4194304.0;
    public static ulong Ticks() {
        return (ulong)DateTime.Now.Ticks;
    }
}

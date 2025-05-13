
using System;
using System.Drawing;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public partial struct Memory{

    public byte[] MemoryBlock;
    public readonly UInt64 Size;
    public Memory(UInt64 size) {
        MemoryBlock = new byte[size];
        Size = size;
    }

}

using System;
using System.Net.Sockets;

public interface MemoryBus<R, W>
{
    public W Read(ref W[] dataArea, R offset);
    public void Write(ref W[] dataArea, R offset, W data);
    
}

public enum MemoryType {
    None = 0,
    CartridgeRom= 1,
    VRAM  = 2,
    CARTRIDGE_RAM = 3,
    WORKING_RAM = 4
}

public partial class MemoryBus8: MemoryBus<UInt16, byte> {
    public byte Read(ref byte[] dataArea, UInt16 offset) {
        return dataArea[offset];
    }

    public void  Write(ref byte[] dataArea, UInt16 offset, byte data) {
        dataArea[offset] = data;
    }
}

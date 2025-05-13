using System;
using System.Collections.Generic;
using System.IO;

public partial struct Serial 
{
    public byte SB;
    public byte SC;
    bool _transferring;
    public List<byte> OutputBuffer;
    byte _out_byte;
    sbyte _transfer_bit;
    public readonly bool Transferring {
        get => _transferring;
    }

    public void Init()
    {
        SB = 0xff;
        SC = 0x7c;
        _transferring = false;
        OutputBuffer = new List<byte>();
    }

    public readonly bool IsMaster() {
        return (SC & 1) == 1;
    }
    public readonly bool EnableTransfer()
    {
        return ((SC >> 7) & 1) == 1;
    }
    public void BeginTransfer()
    {
        _transferring = true;
        _out_byte = SB;
        _transfer_bit = 7;
    }

    public void ProcessTransfer(Emulator e)
    {
        SB <<= 1;
        // set bit_0 to 1
        ++SB;
        --_transfer_bit;
        if (_transfer_bit < 0)
        {
            _transfer_bit = 0;
            EndTransfer(e);
        }
    }

    public void EndTransfer(Emulator e)
    {
        OutputBuffer.Add(_out_byte);
        SC &= 0x7f;
        _transferring = false;
        e.IntFlags |= Emulator.INT_SERIAL;
    }
    public void Tick(Emulator e)
    {
        if (!Transferring && EnableTransfer() && IsMaster())
        {
            BeginTransfer();
        }
        else if (Transferring)
        {
            ProcessTransfer(e);
        }
    }

    public readonly byte BusRead(UInt16 addr)
    {
        if (addr >= 0xff01 && addr <= 0xff02)
        {
            if (addr == 0xff01) return SB;
            if (addr == 0xff02) return SC;
        }
        throw new InvalidDataException("Serial.BusRead expect address from 0xff01 to 0xff02");
    }
    public void BusWrite(UInt16 addr, byte v)
    {
        if (addr >= 0xff01 && addr <= 0xff02)
        {
            if (addr == 0xff01) {
                SB = v;
                return;
            }

            if (addr == 0xff02) {
                SC = (byte)(0x7c | (v & 0x83));
                return;
            }
        }
        throw new InvalidDataException("Serial.BusRead expect address from 0xff01 to 0xff02");
    }

}

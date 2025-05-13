
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

using Utils;

namespace Apu
{
    struct FrameSequencer 
    {
        byte index;
        byte[] _padding;

        public FrameSequencer()
        {
            _padding = new byte[3];
            index = 0;
        }
    }
    public struct Register
    {
        public byte Sweep;
        public byte LengthTimerDutyCycle;
        public byte VolumeEnvelope;
        public byte PeriodLow;
        public byte PeriodHighControl;
        public void Reset()
        {
            this = new Register();
        }
    }
    public struct Sweep
    {
        public byte IterationCounter;
        public byte IterationPace;
    }
    public struct Envelop
    {
        public bool IterationIncrease;
        public byte IterationCounter;
        public byte IterationPace;
    }
    public struct LengthTimer
    {
        public byte Timer;
    }

    public partial class Channel
    {
        public Register Reg;
        public Sweep Sweep;
        public Envelop Envelop;
        public LengthTimer LengthTimer;
        public byte SampleIdx;
        public byte Volume;
        public ushort PeriodCounter;
        public float OutputSample;

        public bool DacOn { get => (Reg.VolumeEnvelope & 0xF8) > 0;}
        public byte WaveType { get => (byte)((Reg.LengthTimerDutyCycle & 0xC0) >> 6);}
        public byte InitialVolume {get => (byte)((Reg.VolumeEnvelope & 0xF0) >> 4);}
        public ushort Period 
        {
            get => (ushort)(Reg.PeriodLow + ((Reg.PeriodHighControl & 0x07) << 8));
            set
            {
                Reg.PeriodLow = (byte)(value & 0xFF);
                Reg.PeriodHighControl = (byte)((Reg.PeriodHighControl & 0xF8) | ((value >> 8) & 0x07));
            }
        }
        public byte SweepPace { get => (byte)((Reg.Sweep & 0x70) >> 4);}
        public bool SweepSubtraction { get => ((Reg.Sweep >> 3) & 0x1) == 1;}
        public byte SweepIndividualStep { get => (byte)(Reg.Sweep  & 0x07);}
        public byte EnvelopPace { get => (byte)(Reg.VolumeEnvelope & 0x07);}
        public bool EnvelopIncrease {get => ((Reg.VolumeEnvelope >> 3) & 0x1) == 1;}
        public byte InitialLengthImer {get => (byte)(Reg.LengthTimerDutyCycle & 0x3F);}

        public bool LengthEnabled {get => ((Reg.PeriodHighControl >> 6) & 0x1) == 1;}

        public void Tick()
        {
            ++PeriodCounter;
            if (PeriodCounter >= 0x800)
            {
                ++SampleIdx;
                SampleIdx &= 7;
                // The init value
                PeriodCounter = Period;
            }
            byte sample = 0;
            switch (WaveType)
            {
                case 0: sample = Apu.PulseWave0[SampleIdx];break;
                case 1: sample = Apu.PulseWave1[SampleIdx];break;
                case 2: sample = Apu.PulseWave2[SampleIdx];break;
                case 3: sample = Apu.PulseWave3[SampleIdx];break;
            }
            OutputSample = Apu.Dac((byte)(sample * Volume));
        }

        public void TickSweep(Apu apu)
        {
                ++Sweep.IterationCounter;
                if (Sweep.IterationCounter == Sweep.IterationPace)
                {
                    int period = Period;
                    byte step = SweepIndividualStep;
                    period += period / (1 << step) * (SweepSubtraction ? -1 : 1);
                    if (period > 0x07FF || period <= 0)
                        apu.DisableCh1();
                    else
                        Period = (ushort)period;
                    
                    Sweep.IterationCounter = 0;
                    Sweep.IterationPace = SweepPace;
                }
        }
        public void TickEnvelop()
        {
                ++Envelop.IterationCounter;
                if (Envelop.IterationCounter >= Envelop.IterationPace)
                {
                    if (Envelop.IterationIncrease)
                    {
                        if (Volume < 15) ++Volume;
                    }
                    else
                    {
                        if (Volume > 0) --Volume;
                    }
                    Envelop.IterationCounter = 0;
                }
            
        }
        public void TickLength()
        {
            ++LengthTimer.Timer;
        }

        public void Reset()
        {
            SampleIdx = 0;
            Volume = InitialVolume;
            PeriodCounter = Period;

            Sweep.IterationCounter = 0;
            Sweep.IterationPace = SweepPace;
            Envelop.IterationIncrease = EnvelopIncrease;
            Envelop.IterationPace = EnvelopPace;
            Envelop.IterationCounter = 0;
            LengthTimer.Timer = InitialLengthImer;
        }

    }
    public struct WaveRegister
    {
        public byte DacEnabled;
        public byte LengthTimer;
        public byte OutputLevel;
        public byte PeriodLow;
        public byte PeriodHighControl;
    }
    public partial class WaveChannel
    {
        public WaveRegister Reg;
        public byte SampleIdx;
        public ushort PeriodCounter;
        public float OutputSample;
        public LengthTimer LengthTimer;
        public bool DacOn {get => (Reg.DacEnabled >> 7) > 0;}
        public byte InitialLengthImer {get => Reg.LengthTimer;}
        public byte OutputLevel {get => (byte)((Reg.OutputLevel & 0x60) >> 5);}
        public ushort Period {get => (ushort)(Reg.PeriodLow | (Reg.PeriodHighControl & 0x07) << 8);}
        public bool LengthEnabled {get => ((Reg.PeriodHighControl >> 6) & 0x01) == 1;}
        public void Tick(Apu apu)
        {
            for (uint i = 0; i < 2; i++)
            {
                ++PeriodCounter;
                if (PeriodCounter >= 0x800)
                {
                    SampleIdx++;
                    SampleIdx &= 31;
                    PeriodCounter = Period;
                }
            }
            byte wave = apu.WavePattern(SampleIdx);
            byte level = OutputLevel;
            switch (level)
            {
                case 0: wave = 0; break;
                case 1: break;
                case 2: wave >>= 1; break;
                case 3: wave >>= 2; break;
            }
            OutputSample = Apu.Dac(wave);
        }
        public void TickLength(Apu apu)
        {
            if (apu.Ch3Enabled && LengthEnabled)
            {
                ++LengthTimer.Timer;
                if (LengthTimer.Timer == 0)
                {
                    apu.DisableCh3();
                }
            }
        }
        public void Reset()
        {
            PeriodCounter = 0;
            SampleIdx = 1;
            LengthTimer.Timer = InitialLengthImer;
        }
    }
    
    public struct NoiseRegister
    {
        public byte LengthTimer;
        public byte VolumeEnvelope;
        public byte FreqRandomness;
        public byte Control;
    }
    public partial class Noise
    {
        public NoiseRegister Reg;
        public Envelop Envelop;
        public ushort LFSR;
        public byte Volume;
        public uint PeriodCounter; 
        public float OutputSample;
        public byte LengthTimer;
        public byte InitialLengthImer {get => (byte)(Reg.LengthTimer & 0x3F);}
        public byte InitialVolume {get => (byte)((Reg.VolumeEnvelope & 0xF0) >> 4);}
        //public uint Period {get => ()ClockDivider() * (1 << ClockShift() << 4);}

    }

    public partial class Apu
    {
        /// <summary>
        /// 87.5%
        /// </summary>
        public static readonly byte[] PulseWave0 = new byte[8]{1, 1, 1, 1, 1, 1, 1, 0};
        /// <summary>
        /// 75%
        /// </summary>
        public static readonly byte[] PulseWave1 = new byte[8]{0, 1, 1, 1, 1, 1, 1, 0};
        /// <summary>
        /// 50%
        /// </summary>
        public static readonly byte[] PulseWave2 = new byte[8]{0, 1, 1, 1, 1, 0, 0, 0};
        /// <summary>
        /// 25%
        /// </summary>
        public static readonly byte[] PulseWave3 = new byte[8]{1, 0, 0, 0, 0, 0, 0, 1};
        /// <summary>
        /// 0xFF10 ~ 0xFF14
        /// </summary>
        public Channel Ch1;
        public Channel Ch2;
        public WaveChannel Ch3;

        /// <summary>
        /// 0xFF24
        /// </summary>
        public byte Nr50MasterVolumeVinPanning;
        /// <summary>
        /// 0xFF25
        /// </summary>
        public byte Nr51MasterPanning;
        /// <summary>
        /// 0xFF26
        /// </summary>
        public byte Nr52MasterControl;
        public byte[] WavePatternRam = new byte[16];

        public byte LastDiv;
        // The DIV-APU counter, increases every time DIV’s bit 4 goes from 1 to 0.
        public byte Div;
        public CycleQueue<float> _audioBufferL = new CycleQueue<float>((int)AUDIO_BUFFER_MAX_SIZE);
        public CycleQueue<float> _audioBufferR = new CycleQueue<float>((int)AUDIO_BUFFER_MAX_SIZE);
        public const uint AUDIO_BUFFER_MAX_SIZE = 65536;
        public readonly object _audio_buffer_lock = new object();
        public int LeftBufferSize { get => _audioBufferL.Size;}
        public int RightBufferSize { get => _audioBufferR.Size;}
        


        public bool Enabled {get => ((Nr52MasterControl >> 7) & 0x1) == 1;}
        public bool Ch1Enabled {get => (Nr52MasterControl & 0x1) == 1;}
        public bool Ch1REnabled {get => (Nr51MasterPanning & 0x1) == 1;}
        public bool Ch1LEnabled {get => ((Nr51MasterPanning >> 4) & 0x1) == 1;}

        public bool Ch2Enabled {get => ((Nr52MasterControl >> 1) & 0x1) == 1;}
        public bool Ch2REnabled {get => ((Nr51MasterPanning >> 1) & 0x1) == 1;}
        public bool Ch2LEnabled {get => ((Nr51MasterPanning >> 5) & 0x1) == 1;}

        public bool Ch3Enabled {get => ((Nr52MasterControl >> 2) & 0x1) == 1;}
        public bool Ch3REnabled {get => ((Nr51MasterPanning >> 2) & 0x1) == 1;}
        public bool Ch3LEnabled {get => ((Nr51MasterPanning >> 6) & 0x1) == 1;}
        public byte RightVolume {get => (byte)(Nr50MasterVolumeVinPanning & 0x07);}
        public byte LeftVolume {get => (byte)((Nr50MasterVolumeVinPanning & 0x70) >> 4);}


        public void Init()
        {
            Ch1 = new Channel();
            Ch2 = new Channel();
            Ch3 = new WaveChannel();
            Reset();
        }
        public void Tick(Emulator e)
        {
            if (!Enabled) return;
            TickDiv(e);
            if ((e.ClockCycles & 3) == 0)
            {
                if (Ch1Enabled) 
                {
                    if (!Ch1.DacOn) DisableCh1();
                    else Ch1.Tick();
                }
                if (Ch2Enabled)
                {
                    if (!Ch2.DacOn) DisableCh2();
                    else Ch2.Tick();
                }
                if (Ch3Enabled)
                {
                    if (!Ch3.DacOn) DisableCh3();
                    else Ch3.Tick(this);
                }
                float sampleL = .0f;
                float sampleR = .0f;
                if (Ch1.DacOn && Ch1LEnabled) sampleL += Ch1.OutputSample;
                if (Ch1.DacOn && Ch1REnabled) sampleR += Ch1.OutputSample;
                if (Ch2.DacOn && Ch2LEnabled) sampleL += Ch2.OutputSample;
                if (Ch2.DacOn && Ch2REnabled) sampleR += Ch2.OutputSample;
                //if (Ch3.DacOn && Ch3LEnabled) sampleL += Ch3.OutputSample;
                //if (Ch3.DacOn && Ch3REnabled) sampleR += Ch3.OutputSample;
                sampleL /= 4.0f;
                sampleR /= 4.0f;
                sampleL *= LeftVolume / 7.0f;
                sampleR *= RightVolume / 7.0f;
                lock (_audio_buffer_lock)
                {
                    if (_audioBufferL.Size >= AUDIO_BUFFER_MAX_SIZE) _audioBufferL.Dequeue();
                    if (_audioBufferR.Size >= AUDIO_BUFFER_MAX_SIZE) _audioBufferR.Dequeue();
                    _audioBufferL.Enqueue(sampleL);
                    _audioBufferR.Enqueue(sampleR);
                }
            }
        }
        public void TickDiv(Emulator e)
        {
            byte div = e.Timer.ReadDiv();
            if (((LastDiv >> 4) & 0x1) == 1 && ((div >> 4) & 0x1) == 0)
            {
                // 512Hz.
                ++Div;
                // Length is ticked at 256Hz. 
                if ((Div & 0x1) == 0)
                {
                    if (Ch1Enabled && Ch1.LengthEnabled) Ch1.TickLength();
                    if (Ch1.LengthTimer.Timer >= 64) DisableCh1();
                    if (Ch2Enabled && Ch2.LengthEnabled) Ch2.TickLength();
                    if (Ch2.LengthTimer.Timer >= 64) DisableCh2();
                    Ch3.TickLength(this);
                }
                // Sweep is ticked at 128Hz.
                if ((Div & 0x3) == 0)
                {
                    if (Ch1Enabled && Ch1.SweepPace > 0)
                        Ch1.TickSweep(this);
                }
                // Envelope is ticked at 64Hz.
                if ((Div & 0x7) == 0)
                {
                    if (Ch1Enabled && Ch1.Envelop.IterationPace > 0) Ch1.TickEnvelop();
                    if (Ch2Enabled && Ch2.Envelop.IterationPace > 0) Ch2.TickEnvelop();
                }
            }
            LastDiv = div;
        }

        public byte BusRead(ushort addr)
        {
            // Ch1 
            if (addr >= 0xFF10 && addr <= 0xFF14)
            {
                if (addr == 0xFF11)
                {
                    // low 6 bits of NR11 is write-only
                    return (byte)(Ch1.Reg.LengthTimerDutyCycle & 0xC0);
                }
                if (addr == 0xFF14)
                {
                    // only bit 6 is readable
                    return (byte)(Ch1.Reg.PeriodHighControl & 0x40);
                }
                return Converter.StructToBytes(Ch1.Reg)[addr - 0xFF10];
            }
            // Ch2 
            if (addr >= 0xFF16 && addr <= 0xFF19)
            {
                if (addr == 0xFF16)
                {
                    // low 6 bits of NR11 is write-only
                    return (byte)(Ch2.Reg.LengthTimerDutyCycle & 0xC0);
                }
                if (addr == 0xFF19)
                {
                    // only bit 6 is readable
                    return (byte)(Ch2.Reg.PeriodHighControl & 0x40);
                }
                return Converter.StructToBytes(Ch2.Reg)[addr - 0xFF16];
            }
            // Ch3
            if (addr >= 0xFF1A && addr <= 0xFF1E)
            {
                // Readonly
                if (addr == 0xFF1B) return 0;
                if (addr == 0xFF1E) return (byte)(Ch3.Reg.PeriodHighControl & 0x40);
                return Converter.StructToBytes(Ch3.Reg)[addr - 0xFF1A];

            }
            if (addr >= 0xFF24 && addr <= 0xFF26)
            {
                if (addr == 0xFF24)
                {
                    return Nr50MasterVolumeVinPanning;
                } 
                else if (addr == 0xFF25)
                    return Nr51MasterPanning;
                else if (addr == 0xFF26)
                    return Nr52MasterControl;
            }
            if (addr >= 0xFF30 && addr <= 0xFF3F) return WavePatternRam[addr - 0xFF30];
            
            Console.WriteLine("\x1b[31m[Error]: Unsupported bus read address 0x{0:X4}.\x1b[0m", addr);
            return 0XFF;
        }
        public void BusWrite(ushort addr, byte value)
        {
            if (addr >= 0xFF10 && addr <= 0xFF14)
            {
                if (!Enabled)
                {
                    if (addr == 0xFF11)
                    {
                        Ch1.Reg.LengthTimerDutyCycle = value;
                    }
                }
                else
                {
                    if (addr == 0xFF10)
                    {
                        if ((Ch1.Reg.Sweep & 0x70) == 0 && ((value & 0x70) != 0))
                        {
                            Ch1.Sweep.IterationCounter = 0;
                            Ch1.Sweep.IterationPace = (byte)((value & 0x70) >> 4);
                        }
                    }
                    if (addr == 0xFF14 && ((value >> 7) & 0x1) == 1)
                    {
                        EnableCh1();
                        value &= 0x7F;
                    }
                    switch (addr)
                    {
                        case 0xFF10: Ch1.Reg.Sweep = value; break;
                        case 0xFF11: Ch1.Reg.LengthTimerDutyCycle = value; break;
                        case 0xFF12: Ch1.Reg.VolumeEnvelope = value; break;
                        case 0xFF13: Ch1.Reg.PeriodLow = value; break;
                        case 0xFF14: Ch1.Reg.PeriodHighControl= value; break;
                    }
                }
                return;
            }
            if (addr >= 0xFF16 && addr <= 0xFF19)
            {
                if (!Enabled)
                {
                    if (addr == 0xFF16)
                    {
                        Ch2.Reg.LengthTimerDutyCycle = value;
                    }
                }
                else
                {
                    if (addr == 0xFF19 && ((value >> 7) & 0x1) == 1)
                    {
                        EnableCh2();
                        value &= 0x7F;
                    }
                    switch (addr)
                    {
                        case 0xFF16: Ch2.Reg.LengthTimerDutyCycle = value; break;
                        case 0xFF17: Ch2.Reg.VolumeEnvelope = value; break;
                        case 0xFF18: Ch2.Reg.PeriodLow = value; break;
                        case 0xFF19: Ch2.Reg.PeriodHighControl= value; break;
                    }
                }
                return;
            }
            if (addr >= 0xFF1A && addr <= 0xFF1E)
            {
                if (!Enabled)
                {
                    if (addr == 0xFF1B) Ch3.Reg.LengthTimer = value;
                }
                else
                {
                    if (addr == 0xFF1E && ((value >> 7) & 0x1) == 1)
                    {
                        EnableCh3();
                        value &= 0x7F;
                    }
                    switch (addr)
                    {
                        case 0xFF1A: Ch3.Reg.DacEnabled = value; break;
                        case 0xFF1B: Ch3.Reg.LengthTimer = value; break;
                        case 0xFF1C: Ch3.Reg.OutputLevel = value; break;
                        case 0xFF1D: Ch3.Reg.PeriodLow = value; break;
                        case 0xFF1E: Ch3.Reg.PeriodHighControl = value; break;
                    }
                }
                return;
            }
            if (addr >= 0xFF24 && addr <= 0xFF26)
            {
                if (addr == 0xFF26)
                {
                    bool enabled = Enabled;
                    Nr52MasterControl = (byte)((value & 0x80) | (Nr52MasterControl & 0x7F));
                    if (enabled && !Enabled)
                    {
                        Disable();
                    }
                    return;
                }
                // All registers except NR52 is read-only if APU is not enabled.
                if (!Enabled) return;
                switch (addr)
                {
                    case 0xFF24: Nr50MasterVolumeVinPanning = value; break;
                    case 0xFF25: Nr51MasterPanning = value; break;
                }
                return;

            }
            if (addr >= 0xFF30 && addr <= 0xFF3F)
            {
                WavePatternRam[addr - 0xFF30] = value;
                return;
            }
            Console.WriteLine("\x1b[31m[Error]: Unsupported bus write address 0x{0:X4}.\x1b[0m", addr);

        }

        public void Disable()
        {
            Reset();
        }

        public void Reset()
        {
            Ch1.Reg.Reset();
            Nr50MasterVolumeVinPanning = 0;
            Nr51MasterPanning = 0;
            Nr52MasterControl = 0;
        }
        public void EnableCh1()
        {
            Nr52MasterControl |= 0x1;
            Ch1.Reset();
        }
        public void DisableCh1()
        {
            Nr52MasterControl &= 0xFE;
        }
        public void EnableCh2()
        {
            Nr52MasterControl |= 0x2;
            Ch2.Reset();
        }
        public void DisableCh2()
        {
            Nr52MasterControl &= 0xFD;
        }
        public void EnableCh3()
        {
            Nr52MasterControl |= 0x04; ;
            Ch3.Reset();
        }
        public void DisableCh3()
        {
            Nr52MasterControl &= 0xFB;
        }
        public byte WavePattern(byte idx)
        {
            byte _base = (byte)(idx >> 1);
            byte wave = WavePatternRam[_base];
            wave = (byte)(((idx & 0x1) > 0) ? (wave & 0x0F) : ((wave >> 4) & 0x0F));
            return wave;
        }
        public static float Dac(byte sample)
        {
            return Utils.Math.Lerp(-1.0f, 1.0f, (15 - sample) / 15.0f);
        }
    }
}

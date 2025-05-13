using System;
using System.Security.AccessControl; using Godot;

public partial struct Cpu {
    public byte A;
    public byte F;
    public byte B;
    public byte C;
    public byte D;
    public byte E;

    public byte H;
    public byte L;
    public UInt16 SP;
    public UInt16 PC;
    public bool Halted;
    bool _interrupt_master_enabled;
    byte _interrupt_master_enabling_countdown;

    public UInt16 AF {
        readonly get => (UInt16)((UInt16)((UInt16)A << 8) | (UInt16)F);
        set {
            A = (byte)(value >> 8);
            F = (byte)(value & 0xF0);
        }
    }

    public UInt16 BC {
        readonly get => (UInt16)((UInt16)((UInt16)B << 8) | (UInt16)C);
        set {
            B = (byte)(value >> 8);
            C = (byte)(value & 0xFF);
        }
    }

    public UInt16 DE {
        readonly get => (UInt16)((UInt16)((UInt16)D << 8) | (UInt16)E);
        set {
            D = (byte)(value >> 8);
            E = (byte)(value & 0xFF);
        }
    }

    public UInt16 HL {
        readonly get => (UInt16)((UInt16)((UInt16)H << 8) | (UInt16)L);
        set {
            H = (byte)(value >> 8);
            L = (byte)(value & 0xFF);
        }
    }

    public bool FZ { get => (F & 0x80) != 0;}
    public bool FN {get => (F & 0x40) != 0;}
    public bool FH {get => (F & 0x20) != 0;}
    public bool FC {get => (F & 0x10) != 0;}

    public void SetFZ() {
        F |= 0x80;
    }
    public void ResetFZ() {
        F &= 0x7f;
    }

    public void SetFN() {
        F |= 0x40;
    }
    public void ResetFN() {
        F &= 0xbF;
    }

    public void SetFH() {
        F |= 0x20;
    }
    public void ResetFH() {
        F &= 0xdF;
    }

    public void SetFC() {
        F |= 0x10;
    }
    public void ResetFC() {
        F &= 0xeF;
    }

    public void Init() {
         // ref: https://github.com/rockytriton/LLD_gbemu/raw/main/docs/The%20Cycle-Accurate%20Game%20Boy%20Docs.pdf
        AF = 0x01B0;
        BC = 0x0013;
        DE = 0x00D8;
        HL = 0x014D;
        SP = 0xFFFE;
        PC = 0x0100;
        Halted = false;
        _interrupt_master_enabled = false;
        _interrupt_master_enabling_countdown = 0;
    }

    public void Step(Emulator emu) {
        if (!Halted) {
            if (_interrupt_master_enabled && ((emu.IntFlags & emu.IntEnableFlags) > 0)) 
            {
                InterruptService(emu);
            } 
            else 
            {
                byte optCode = emu.BusRead(PC);
                //if (HL >= 0xffff) {
                //    Console.WriteLine("OPT: {9:G} PC: {0:X4}   A: {1:X2}   F: {2:X2}   B: {3:X2}   C: {4:X2}   D: {5:X2}   E: {6:X2}   HL: {7:X4}  SP: {8:X4}",
                //        PC, A, F, B, C, D, E, HL, SP, Instructions.GetOptName(optCode)
                //    );
                //}
                ++PC;
                Action<Emulator> f = INSTRUCTIONS[optCode];
                if (f != null) {
                    f.Invoke(emu);
                } else {
                    Console.WriteLine("\x1b[31mInstruction 0x{0:X4} not present.\x1b[0m", optCode);

                    emu.Pause();
                }
            }
        } else {
            emu.Tick(1);
            if ((emu.IntFlags & emu.IntEnableFlags) > 0)
            {
                Halted = false;
            }
        }

        if (_interrupt_master_enabling_countdown > 0)
        {
            --_interrupt_master_enabling_countdown;
            if (_interrupt_master_enabling_countdown == 0)
            {
                _interrupt_master_enabled = true;
            }
        }

    }
    public void EnableInterruptMaster() {
        _interrupt_master_enabling_countdown = 2;
    }
    public void DisableInterruptMaster() {
        _interrupt_master_enabled = false;
        _interrupt_master_enabling_countdown = 0;
    }
    public void InterruptService(Emulator e) {
        byte int_flags = (byte)(e.IntFlags & e.IntEnableFlags);
        byte int_service = 0;
        if ((int_flags & Emulator.INT_VBLANK) > 0) int_service = Emulator.INT_VBLANK;
        else if ((int_flags & Emulator.INT_LCD_STAT) > 0) int_service = Emulator.INT_LCD_STAT;
        else if ((int_flags & Emulator.INT_TIMER) > 0) int_service = Emulator.INT_TIMER;
        else if ((int_flags & Emulator.INT_SERIAL) > 0) int_service = Emulator.INT_SERIAL;
        else if ((int_flags & Emulator.INT_JOYPAD) > 0) int_service = Emulator.INT_JOYPAD;
        e.IntFlags &= (byte)~int_service;
        e.Cpu.DisableInterruptMaster();
        e.Tick(2);
        Instructions.Push16(e, e.Cpu.PC);
        e.Tick(2);
        switch (int_service)
        {
            case Emulator.INT_VBLANK:
                e.Cpu.PC = 0x40; break;
            case Emulator.INT_LCD_STAT:
                e.Cpu.PC = 0x48; break;
            case Emulator.INT_TIMER:
                e.Cpu.PC = 0x50; break;
            case Emulator.INT_SERIAL:
                e.Cpu.PC = 0x58; break;
            case Emulator.INT_JOYPAD:
                e.Cpu.PC = 0x60; break;
        }
        e.Tick(1);

    }

    public static readonly Action<Emulator>[] INSTRUCTIONS = new Action<Emulator>[256]{
        new(Instructions.X00_Nop), 
        new(Instructions.X01_LD_I162BC),
        new(Instructions.X02_LD_A2MBD), 
        new(Instructions.X03_INC_BC),
        new(Instructions.X04_INC_B),
        new(Instructions.X05_DEC_B),
        new(Instructions.X06_LD_I82B), 
        new(Instructions.X07_RLCA), 
        new(Instructions.X08_LD_SP2MI16), 
        new(Instructions.X09_ADD_HL_BC), 
        new(Instructions.X0A_LD_MBC2A),
        new(Instructions.X0B_DEC_BC),
        new(Instructions.X0C_INC_C),
        new(Instructions.X0D_DEC_C),
        new(Instructions.X0E_LD_I82C), 
        new(Instructions.X0F_RRCA), 

        new(Instructions.X10_STOP),
        new(Instructions.X11_LD_I162DE),
        new(Instructions.X12_LD_A2MDE),
        new(Instructions.X13_INC_DE),
        new(Instructions.X14_INC_D),
        new(Instructions.X15_DEC_D),
        new(Instructions.X16_LD_I82D),
        new(Instructions.X17_RLA),
        new(Instructions.X18_JR_I8),
        new(Instructions.X19_ADD_HL_DE),
        new(Instructions.X1A_LD_MDE2A),
        new(Instructions.X1B_DEC_DE),
        new(Instructions.X1C_INC_E),
        new(Instructions.X1D_DEC_E),
        new(Instructions.X1E_LD_I82E),
        new(Instructions.X1F_RRA),

        new(Instructions.X20_JRNZ_I8),
        new(Instructions.X21_LD_I162HL),
        new(Instructions.X22_LDI_A2MHL),
        new(Instructions.X23_INC_HL),
        new(Instructions.X24_INC_H),
        new(Instructions.X25_DEC_H),
        new(Instructions.X26_LD_I82H),
        new(Instructions.X27_DAA),
        new(Instructions.X28_JRZ_I8),
        new(Instructions.X29_ADD_HL_HL),
        new(Instructions.X2A_LDI_MHL2A),
        new(Instructions.X2B_DEC_HL),
        new(Instructions.X2C_INC_L),
        new(Instructions.X2D_DEC_L),
        new(Instructions.X2E_LD_I82L),
        new(Instructions.X2F_CPL),

        new(Instructions.X30_JRNC_I8),
        new(Instructions.X31_LD_I162SP),
        new(Instructions.X32_LDD_A2MHL),
        new(Instructions.X33_INC_SP),
        new(Instructions.X34_INC_MHL),
        new(Instructions.X35_DEC_MHL),
        new(Instructions.X36_LD_I82MHL),
        new(Instructions.X37_SCF),
        new(Instructions.X38_JRC_I8),
        new(Instructions.X39_ADD_HL_SP),
        new(Instructions.X3A_LDD_MHL2A),
        new(Instructions.X3B_DEC_SP),
        new(Instructions.X3C_INC_A),
        new(Instructions.X3D_DEC_A),
        new(Instructions.X3E_LD_I82A),
        new(Instructions.X3F_CCF),

        new(Instructions.X40_LD_B2B),
        new(Instructions.X41_LD_C2B),
        new(Instructions.X42_LD_D2B),
        new(Instructions.X43_LD_E2B),
        new(Instructions.X44_LD_H2B),
        new(Instructions.X45_LD_L2B),
        new(Instructions.X46_LD_MHL2B),
        new(Instructions.X47_LD_A2B),
        new(Instructions.X48_LD_B2C),
        new(Instructions.X49_LD_C2C),
        new(Instructions.X4A_LD_D2C),
        new(Instructions.X4B_LD_E2C),
        new(Instructions.X4C_LD_H2C),
        new(Instructions.X4D_LD_L2C),
        new(Instructions.X4E_LD_MHL2C),
        new(Instructions.X4F_LD_A2C),

        new(Instructions.X50_LD_B2D),
        new(Instructions.X51_LD_C2D),
        new(Instructions.X52_LD_D2D),
        new(Instructions.X53_LD_E2D),
        new(Instructions.X54_LD_H2D),
        new(Instructions.X55_LD_L2D),
        new(Instructions.X56_LD_MHL2D),
        new(Instructions.X57_LD_A2D),
        new(Instructions.X58_LD_B2E),
        new(Instructions.X59_LD_C2E),
        new(Instructions.X5A_LD_D2E),
        new(Instructions.X5B_LD_E2E),
        new(Instructions.X5C_LD_H2E),
        new(Instructions.X5D_LD_L2E),
        new(Instructions.X5E_LD_MHL2E),
        new(Instructions.X5F_LD_A2E),

        new(Instructions.X60_LD_B2H),
        new(Instructions.X61_LD_C2H),
        new(Instructions.X62_LD_D2H),
        new(Instructions.X63_LD_E2H),
        new(Instructions.X64_LD_H2H),
        new(Instructions.X65_LD_L2H),
        new(Instructions.X66_LD_MHL2H),
        new(Instructions.X67_LD_A2H),
        new(Instructions.X68_LD_B2L),
        new(Instructions.X69_LD_C2L),
        new(Instructions.X6A_LD_D2L),
        new(Instructions.X6B_LD_E2L),
        new(Instructions.X6C_LD_H2L),
        new(Instructions.X6D_LD_L2L),
        new(Instructions.X6E_LD_MHL2L),
        new(Instructions.X6F_LD_A2L),

        new(Instructions.X70_LD_B2MHL),
        new(Instructions.X71_LD_C2MHL),
        new(Instructions.X72_LD_D2MHL),
        new(Instructions.X73_LD_E2MHL),
        new(Instructions.X74_LD_H2MHL),
        new(Instructions.X75_LD_L2MHL),
        new(Instructions.X76_HALT),
        new(Instructions.X77_LD_A2MHL),
        new(Instructions.X78_LD_B2A),
        new(Instructions.X79_LD_C2A),
        new(Instructions.X7A_LD_D2A),
        new(Instructions.X7B_LD_E2A),
        new(Instructions.X7C_LD_H2A),
        new(Instructions.X7D_LD_L2A),
        new(Instructions.X7E_LD_MHL2A),
        new(Instructions.X7F_LD_A2A),

        new(Instructions.X80_ADD_A_B),
        new(Instructions.X81_ADD_A_C),
        new(Instructions.X82_ADD_A_D),
        new(Instructions.X83_ADD_A_E),
        new(Instructions.X84_ADD_A_H),
        new(Instructions.X85_ADD_A_L),
        new(Instructions.X86_ADD_A_MHL),
        new(Instructions.X87_ADD_A_A),
        new(Instructions.X88_ADC_A_B),
        new(Instructions.X89_ADC_A_C),
        new(Instructions.X8A_ADC_A_D),
        new(Instructions.X8B_ADC_A_E),
        new(Instructions.X8C_ADC_A_H),
        new(Instructions.X8D_ADC_A_L),
        new(Instructions.X8E_ADC_A_MHL),
        new(Instructions.X8F_ADC_A_A),

        new(Instructions.X90_SUB_B),
        new(Instructions.X91_SUB_C),
        new(Instructions.X92_SUB_D),
        new(Instructions.X93_SUB_E),
        new(Instructions.X94_SUB_H),
        new(Instructions.X95_SUB_L),
        new(Instructions.X96_SUB_MHL),
        new(Instructions.X97_SUB_A),
        new(Instructions.X98_SBC_A_B),
        new(Instructions.X99_SBC_A_C),
        new(Instructions.X9A_SBC_A_D),
        new(Instructions.X9B_SBC_A_E),
        new(Instructions.X9C_SBC_A_H),
        new(Instructions.X9D_SBC_A_L),
        new(Instructions.X9E_SUC_A_MHL),
        new(Instructions.X9F_SBC_A_A),

        new(Instructions.XA0_AND_B),
        new(Instructions.XA1_AND_C),
        new(Instructions.XA2_AND_D),
        new(Instructions.XA3_AND_E),
        new(Instructions.XA4_AND_H),
        new(Instructions.XA5_AND_L),
        new(Instructions.XA6_AND_MHL),
        new(Instructions.XA7_AND_A),
        new(Instructions.XA8_XOR_B),
        new(Instructions.XA9_XOR_C),
        new(Instructions.XAA_XOR_D),
        new(Instructions.XAB_XOR_E),
        new(Instructions.XAC_XOR_H),
        new(Instructions.XAD_XOR_L),
        new(Instructions.XAE_XOR_MHL),
        new(Instructions.XAF_XOR_A),

        new(Instructions.XB0_OR_B),
        new(Instructions.XB1_OR_C),
        new(Instructions.XB2_OR_D),
        new(Instructions.XB3_OR_E),
        new(Instructions.XB4_OR_H),
        new(Instructions.XB5_OR_L),
        new(Instructions.XB6_OR_I8),
        new(Instructions.XB7_OR_A),
        new(Instructions.XB8_CP_B),
        new(Instructions.XB9_CP_C),
        new(Instructions.XBA_CP_D),
        new(Instructions.XBB_CP_E),
        new(Instructions.XBC_CP_H),
        new(Instructions.XBD_CP_L),
        new(Instructions.XBE_CP_MHL),
        new(Instructions.XBF_CP_B),

        new(Instructions.XC0_RET_NZ),
        new(Instructions.XC1_POP_BC),
        new(Instructions.XC2_JPNZ_I16),
        new(Instructions.XC3_JP_I16),
        new(Instructions.XC4_CALLNZ_I16),
        new(Instructions.XC5_PUSH_BC),
        new(Instructions.XC6_ADD_A_I8),
        new(Instructions.XC7_RST_00H),
        new(Instructions.XC8_RETZ),
        new(Instructions.XC9_RET),
        new(Instructions.XCA_JPZ_I16),
        new(Instructions.XCB_PREFIX_CB),
        new(Instructions.XCC_CALLZ_I16),
        new(Instructions.XCD_CALL_I16),
        new(Instructions.XCE_ADC_A_I8),
        new(Instructions.XCF_RST_08H),

        new(Instructions.XD0_RETNC),
        new(Instructions.XD1_POP_DE),
        new(Instructions.XD2_JPNC_I16),
        null,
        new(Instructions.XD4_CALLNC_I16),
        new(Instructions.XD5_PUSH_DE),
        new(Instructions.XD6_SUB_I8),
        new(Instructions.XD7_RST_10H),
        new(Instructions.XD8_RET_C),
        new(Instructions.XD9_RETI),
        new(Instructions.XDA_JPC_I16),
        null,
        new(Instructions.XDC_CALLC_I16),
        null,
        new(Instructions.XDE_SBC_A_I8),
        new(Instructions.XDF_RST_18H),

        new(Instructions.XE0_LDH_A2OMI8),
        new(Instructions.XE1_POP_HL),
        new(Instructions.XE2_LD_A2OMC),
        null,
        null,
        new(Instructions.XE5_PUSH_HL),
        new(Instructions.XE6_AND_I8),
        new(Instructions.XE7_RST_20H),
        new(Instructions.XE8_ADD_SP_MI8),
        new(Instructions.XE9_JP_HL),
        new(Instructions.XEA_LD_A2MI16),
        null,
        null,
        null,
        new(Instructions.XEE_XOR_I8),
        new(Instructions.XEF_RST_28H),

        new(Instructions.XF0_LDH_OMI82A),
        new(Instructions.XF1_POP_AF),
        new(Instructions.XF2_LD_OMC2A),
        new(Instructions.XF3_DI),
        null,
        new(Instructions.XF5_PUSH_BC),
        new(Instructions.XF6_OR_I8),
        new(Instructions.XF7_RST_30H),
        new(Instructions.XF8_LD_SP_ADD_I8_2_HL),
        new(Instructions.XF9_LD_HL2SP),
        new(Instructions.XFA_LD_MI162A),
        new(Instructions.XFB_EI),
        null,
        null,
        new(Instructions.XFE_CP_I8),
        new(Instructions.XFF_RST_38H),
    };
}
